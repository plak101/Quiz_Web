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
            var userId = GetCurrentUserId();
            var cartItems = await _cartService.GetCartItemsAsync(userId);

            if (!cartItems.Any())
                return Json(new { success = false, message = "Giỏ hàng trống" });

            var total = cartItems.Sum(x => x.Course.Price);

            // 1) Tạo Order
            var order = new Order
            {
                BuyerId = userId,
                TotalAmount = total,
                Status = "Pending"
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // 2) Tạo OrderItems
            foreach (var item in cartItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    CourseId = item.CourseId,
                    Price = item.Course.Price
                });
            }
            await _context.SaveChangesAsync();

            // 3) Tạo Payment
            var orderIdStr = $"ORDER_{order.OrderId}_{DateTime.Now:yyyyMMddHHmmss}";
            var payment = new Payment
            {
                OrderId = order.OrderId,
                Provider = "MoMo",
                Amount = total,
                Currency = "VND",
                Status = "Pending",
                RawPayload = orderIdStr
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // 4) Gọi MoMo
            var momo = await _momoService.CreatePaymentAsync(total, "Thanh toán giỏ hàng", orderIdStr);

            if (momo.resultCode == 0)
            {
                payment.ProviderRef = momo.orderId;
                await _context.SaveChangesAsync();

                return Json(new { success = true, payUrl = momo.payUrl });
            }

            return Json(new { success = false, message = momo.message });
        }

        [HttpPost]
        public async Task<IActionResult> MoMoCallback([FromBody] MoMoIpnRequest ipn)
        {
            if (!_momoService.ValidateSignature(ipn))
                return BadRequest();

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.ProviderRef == ipn.orderId);

            if (payment == null) return NotFound();

            if (ipn.resultCode == 0)
            {
                payment.Status = "Paid";
                payment.PaidAt = DateTime.UtcNow;

                payment.Order.Status = "Paid";

                // Mở quyền truy cập tất cả khóa học
                var items = await _context.OrderItems
                    .Where(i => i.OrderId == payment.OrderId)
                    .ToListAsync();

                foreach (var item in items)
                    await _purchaseService.GrantAccessAsync(payment.Order.BuyerId, item.CourseId);

                // Clear cart
                await _cartService.ClearCartAsync(payment.Order.BuyerId);
            }
            else
            {
                payment.Status = "Failed";
                payment.Order.Status = "Failed";
            }

            await _context.SaveChangesAsync();
            return Ok("success");
        }

		[HttpGet]
		public async Task<IActionResult> MoMoReturn(string orderId, int resultCode, string message)
		{
			var payment = await _context.Payments
				.Include(p => p.Order)
				.FirstOrDefaultAsync(p => p.ProviderRef == orderId);

			if (payment == null)
			{
				ViewBag.Success = false;
				ViewBag.Message = "Không tìm thấy giao dịch";
				return View("PaymentResult");
			}

			var order = payment.Order;

			if (resultCode == 0)
			{
				if (payment.Status == "Pending")
				{
					payment.Status = "Paid";
					payment.PaidAt = DateTime.UtcNow;

					order.Status = "Paid";

					// Lấy danh sách khóa học từ OrderItems
					var items = await _context.OrderItems
						.Where(i => i.OrderId == order.OrderId)
						.ToListAsync();

					foreach (var item in items)
						await _purchaseService.GrantAccessAsync(order.BuyerId, item.CourseId);

					await _context.SaveChangesAsync();
				}

				ViewBag.Success = true;
				ViewBag.Message = "Thanh toán thành công!";
				await _cartService.ClearCartAsync(order.BuyerId);
			}
			else
			{
				payment.Status = "Failed";
				order.Status = "Failed";
				await _context.SaveChangesAsync();

				ViewBag.Success = false;
				ViewBag.Message = "Thanh toán thất bại: " + message;
			}

			return View("PaymentResult");
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