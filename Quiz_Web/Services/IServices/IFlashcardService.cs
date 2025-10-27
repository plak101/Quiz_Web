using Quiz_Web.Models.Entities;

namespace Quiz_Web.Services.IServices
{
    public interface IFlashcardService
    {
        Task<FlashcardSet?> GetFlashcardSetByIdAsync(int setId);
        Task<List<Flashcard>> GetFlashcardsBySetIdAsync(int setId);
        Task<bool> FlashcardSetExistsAsync(int setId);
    }
}
