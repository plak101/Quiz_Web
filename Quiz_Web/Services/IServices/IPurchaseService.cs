using Quiz_Web.Models.Entities;

namespace Quiz_Web.Services.IServices
{
    public interface IPurchaseService
    {
        Task<bool> HasUserPurchasedCourseAsync(int userId, int courseId);
        Task<CoursePurchase> CreatePurchaseAsync(int userId, List<int> courseIds, decimal totalAmount);
        Task<bool> CompletePurchaseAsync(int purchaseId, string providerRef);
        Task<List<CoursePurchase>> GetUserPurchasesAsync(int userId);
    }
}