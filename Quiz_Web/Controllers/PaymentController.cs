using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quiz_Web.Models.EF;
using Quiz_Web.Models.Entities;
using Quiz_Web.Models.MoMoPayment;
using Quiz_Web.Services.IServices;
using System.Security.Claims;
using System.Text.Json;

namespace Quiz_Web.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IMoMoPaymentService _momoService;
        private readonly ICartService _cartService;
        private readonly IPurchaseService _purchaseService;
        private readonly LearningPlatformContext _context;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IMoMoPaymentService momoService,
            ICartService cartService,
            IPurchaseService purchaseService,
            LearningPlatformContext context,
            ILogger<PaymentController> logger)
        {
            _momoService = momoService;
            _cartService = cartService;
            _purchaseService = purchaseService;
            _context = context;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateMoMoPayment()
        {
            try
            {
                var userId = GetCurrentUserId();
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                
                if (!cartItems.Any())
                {
                    return Json(new { success = false, message = "Giỏ hàng trống" });
                }

                var total = cartItems.Sum(x => x.Course.Price);
                var orderId = $"ORDER_{userId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
                var orderInfo = $"Thanh toan khoa hoc - {cartItems.Count} khoa hoc";
                var courseIds = cartItems.Select(x => x.CourseId).ToList();

                // Tạo purchase records
                var purchase = await _purchaseService.CreatePurchaseAsync(userId, courseIds, total);

                // Tạo payment record
                var payment = new Payment
                {
                    PurchaseId = purchase.PurchaseId,
                    Provider = "MoMo",
                    Amount = total,
                    Currency = "VND",
                    Status = "pending",
                    RawPayload = JsonSerializer.Serialize(new { orderId, courseIds })
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Tạo MoMo payment
                var momoResponse = await _momoService.CreatePaymentAsync(total, orderInfo, orderId);

                if (momoResponse.resultCode == 0)
                {
                    // Cập nhật payment với provider reference
                    payment.ProviderRef = momoResponse.orderId;
                    await _context.SaveChangesAsync();

                    return Json(new { 
                        success = true, 
                        payUrl = momoResponse.payUrl,
                        qrCodeUrl = momoResponse.qrCodeUrl 
                    });
                }

                return Json(new { success = false, message = momoResponse.message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MoMo payment");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo thanh toán" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MoMoCallback([FromBody] MoMoIpnRequest ipnRequest)
        {
            try
            {
                _logger.LogInformation($"MoMo IPN received: {JsonSerializer.Serialize(ipnRequest)}");

                // Validate signature
                if (!_momoService.ValidateSignature(ipnRequest))
                {
                    _logger.LogWarning("Invalid MoMo signature");
                    return BadRequest("Invalid signature");
                }

                // Tìm payment record
                var payment = await _context.Payments
                    .Include(p => p.Purchase)
                    .ThenInclude(p => p.Buyer)
                    .FirstOrDefaultAsync(p => p.ProviderRef == ipnRequest.orderId);

                if (payment == null)
                {
                    _logger.LogWarning($"Payment not found for orderId: {ipnRequest.orderId}");
                    return NotFound("Payment not found");
                }

                // Cập nhật payment status
                payment.Status = ipnRequest.resultCode == 0 ? "completed" : "failed";
                payment.PaidAt = ipnRequest.resultCode == 0 ? DateTime.UtcNow : null;
                payment.RawPayload = JsonSerializer.Serialize(ipnRequest);

                // Cập nhật purchase status
                payment.Purchase.Status = ipnRequest.resultCode == 0 ? "Paid" : "Failed";

                if (ipnRequest.resultCode == 0)
                {
                    // Thanh toán thành công - hoàn thành purchase
                    await _purchaseService.CompletePurchaseAsync(payment.PurchaseId, ipnRequest.orderId);
                }

                await _context.SaveChangesAsync();

                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo callback");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> MoMoReturn(string orderId, int resultCode, string message)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(p => p.Purchase)
                    .FirstOrDefaultAsync(p => p.ProviderRef == orderId);

                if (payment == null)
                {
                    ViewBag.Message = "Không tìm thấy thông tin thanh toán";
                    ViewBag.Success = false;
                    return View("PaymentResult");
                }

                if (resultCode == 0)
                {
                    // Cập nhật payment status nếu chưa được cập nhật
                    if (payment.Status == "pending")
                    {
                        payment.Status = "completed";
                        payment.PaidAt = DateTime.UtcNow;
                        
                        // Cập nhật purchase status
                        payment.Purchase.Status = "Paid";
                        
                        // Hoàn thành purchase
                        await _purchaseService.CompletePurchaseAsync(payment.PurchaseId, orderId);
                        
                        await _context.SaveChangesAsync();
                    }
                    
                    ViewBag.Message = "Thanh toán thành công! Bạn đã có thể truy cập khóa học.";
                    ViewBag.Success = true;
                    
                    // Clear cart sau khi thanh toán thành công
                    await _cartService.ClearCartAsync(payment.Purchase.BuyerId);
                }
                else
                {
                    // Cập nhật payment status thất bại
                    if (payment.Status == "pending")
                    {
                        payment.Status = "failed";
                        payment.Purchase.Status = "Failed";
                        await _context.SaveChangesAsync();
                    }
                    
                    ViewBag.Message = $"Thanh toán thất bại: {message}";
                    ViewBag.Success = false;
                }

                return View("PaymentResult");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MoMo return");
                ViewBag.Message = "Có lỗi xảy ra khi xử lý kết quả thanh toán";
                ViewBag.Success = false;
                return View("PaymentResult");
            }
        }



        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CheckCourseAccess(int courseId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var hasAccess = await _purchaseService.HasUserPurchasedCourseAsync(userId, courseId);
                return Json(new { hasAccess });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking course access");
                return Json(new { hasAccess = false });
            }
        }

        // Endpoint để kiểm tra và cập nhật status pending purchases (chỉ dùng cho testing)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdatePendingPurchases()
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Tìm các purchase pending của user trong 24h qua
                var pendingPurchases = await _context.CoursePurchases
                    .Where(p => p.BuyerId == userId && 
                               p.Status == "Pending" && 
                               p.PurchasedAt >= DateTime.UtcNow.AddHours(-24))
                    .ToListAsync();
                
                if (!pendingPurchases.Any())
                {
                    return Json(new { success = false, message = "Không có giao dịch pending nào" });
                }
                
                // Cập nhật tất cả thành Paid
                foreach (var purchase in pendingPurchases)
                {
                    purchase.Status = "Paid";
                }
                
                await _context.SaveChangesAsync();
                
                return Json(new { 
                    success = true, 
                    message = $"Đã cập nhật {pendingPurchases.Count} giao dịch thành completed",
                    count = pendingPurchases.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pending purchases");
                return Json(new { success = false, message = "Có lỗi xảy ra" });
            }
        }
    }
}