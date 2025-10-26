using Microsoft.EntityFrameworkCore;
using Quiz_Web.Models.EF;
using Quiz_Web.Models.Entities;
using Quiz_Web.Services.IServices;

namespace Quiz_Web.Services
{
    public class FlashcardService : IFlashcardService
    {
        private readonly LearningPlatformContext _context;
        private readonly ILogger<FlashcardService> _logger;

        public FlashcardService(LearningPlatformContext context, ILogger<FlashcardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<FlashcardSet?> GetFlashcardSetByIdAsync(int setId)
        {
            try
            {
                var flashcardSet = await _context.FlashcardSets
                    .Include(fs => fs.Flashcards.OrderBy(f => f.OrderIndex))
                    .FirstOrDefaultAsync(fs => fs.SetId == setId);

                return flashcardSet;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting flashcard set {setId}: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Flashcard>> GetFlashcardsBySetIdAsync(int setId)
        {
            try
            {
                var flashcards = await _context.Flashcards
                    .Where(f => f.SetId == setId)
                    .OrderBy(f => f.OrderIndex)
                    .ToListAsync();

                return flashcards;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting flashcards for set {setId}: {ex.Message}");
                return new List<Flashcard>();
            }
        }

        public async Task<bool> FlashcardSetExistsAsync(int setId)
        {
            try
            {
                return await _context.FlashcardSets.AnyAsync(fs => fs.SetId == setId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking flashcard set existence {setId}: {ex.Message}");
                return false;
            }
        }
    }
}
