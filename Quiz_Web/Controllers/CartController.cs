using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quiz_Web.Services.IServices;
using System.Security.Claims;

namespace Quiz_Web.Controllers
{
    [Authorize]
    [Route("api/cart")]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
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

        [HttpGet("items")]
        public async Task<IActionResult> GetCartItems()
        {
            try
            {
                var userId = GetCurrentUserId();
                var items = await _cartService.GetCartItemsAsync(userId);
                var total = await _cartService.GetCartTotalAsync(userId);

                return Json(new
                {
                    success = true,
                    items = items.Select(ci => new
                    {
                        courseId = ci.CourseId,
                        title = ci.Course.Title,
                        coverUrl = ci.Course.CoverUrl,
                        price = ci.Course.Price,
                        instructor = ci.Course.Owner.FullName,
                        addedAt = ci.AddedAt
                    }),
                    total = total,
                    count = items.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart items");
                return Json(new { success = false, message = "Không th? t?i gi? hàng" });
            }
        }

        [HttpPost("add/{courseId}")]
        public async Task<IActionResult> AddToCart(int courseId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _cartService.AddToCartAsync(userId, courseId);

                if (!success)
                {
                    return Json(new { success = false, message = "Không th? thêm khóa h?c vào gi? hàng" });
                }

                var count = await _cartService.GetCartItemCountAsync(userId);
                return Json(new { success = true, message = "?ã thêm vào gi? hàng", count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding course {CourseId} to cart", courseId);
                return Json(new { success = false, message = "?ã x?y ra l?i" });
            }
        }

        [HttpDelete("remove/{courseId}")]
        public async Task<IActionResult> RemoveFromCart(int courseId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _cartService.RemoveFromCartAsync(userId, courseId);

                if (!success)
                {
                    return Json(new { success = false, message = "Không th? xóa khóa h?c kh?i gi? hàng" });
                }

                var count = await _cartService.GetCartItemCountAsync(userId);
                var total = await _cartService.GetCartTotalAsync(userId);

                return Json(new { success = true, message = "?ã xóa kh?i gi? hàng", count = count, total = total });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing course {CourseId} from cart", courseId);
                return Json(new { success = false, message = "?ã x?y ra l?i" });
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _cartService.ClearCartAsync(userId);

                if (!success)
                {
                    return Json(new { success = false, message = "Không th? xóa gi? hàng" });
                }

                return Json(new { success = true, message = "?ã xóa t?t c? khóa h?c kh?i gi? hàng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return Json(new { success = false, message = "?ã x?y ra l?i" });
            }
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCartCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var count = await _cartService.GetCartItemCountAsync(userId);
                return Json(new { success = true, count = count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart count");
                return Json(new { success = false, count = 0 });
            }
        }
    }
}
