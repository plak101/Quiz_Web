using Microsoft.EntityFrameworkCore;
using Quiz_Web.Models.EF;
using Quiz_Web.Models.Entities;
using Quiz_Web.Services.IServices;

namespace Quiz_Web.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly LearningPlatformContext _context;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(LearningPlatformContext context, ILogger<PurchaseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> HasUserPurchasedCourseAsync(int userId, int courseId)
        {
            return await _context.CoursePurchases
                .AnyAsync(cp => cp.BuyerId == userId && 
                               cp.CourseId == courseId && 
                               cp.Status == "completed");
        }

        public async Task<CoursePurchase> CreatePurchaseAsync(int userId, List<int> courseIds, decimal totalAmount)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tạo purchase chính cho course đầu tiên
                var mainPurchase = new CoursePurchase
                {
                    BuyerId = userId,
                    CourseId = courseIds.First(),
                    PricePaid = totalAmount,
                    Currency = "VND",
                    Status = "pending",
                    PurchasedAt = DateTime.UtcNow
                };

                _context.CoursePurchases.Add(mainPurchase);
                await _context.SaveChangesAsync();

                // Tạo purchase cho các course còn lại (nếu có)
                foreach (var courseId in courseIds.Skip(1))
                {
                    var purchase = new CoursePurchase
                    {
                        BuyerId = userId,
                        CourseId = courseId,
                        PricePaid = 0, // Đã tính trong purchase chính
                        Currency = "VND",
                        Status = "pending",
                        PurchasedAt = DateTime.UtcNow
                    };
                    _context.CoursePurchases.Add(purchase);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return mainPurchase;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating purchase for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CompletePurchaseAsync(int purchaseId, string providerRef)
        {
            try
            {
                var purchase = await _context.CoursePurchases
                    .FirstOrDefaultAsync(p => p.PurchaseId == purchaseId);

                if (purchase == null)
                {
                    _logger.LogWarning("Purchase not found: {PurchaseId}", purchaseId);
                    return false;
                }

                purchase.Status = "completed";

                // Cập nhật tất cả purchase cùng user và cùng thời điểm (trong vòng 1 phút)
                var relatedPurchases = await _context.CoursePurchases
                    .Where(p => p.BuyerId == purchase.BuyerId &&
                               p.Status == "pending" &&
                               Math.Abs(EF.Functions.DateDiffSecond(p.PurchasedAt, purchase.PurchasedAt)) <= 60)
                    .ToListAsync();

                foreach (var relatedPurchase in relatedPurchases)
                {
                    relatedPurchase.Status = "completed";
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing purchase {PurchaseId}", purchaseId);
                return false;
            }
        }

        public async Task<List<CoursePurchase>> GetUserPurchasesAsync(int userId)
        {
            return await _context.CoursePurchases
                .Include(cp => cp.Course)
                .Where(cp => cp.BuyerId == userId && cp.Status == "completed")
                .OrderByDescending(cp => cp.PurchasedAt)
                .ToListAsync();
        }
    }
}