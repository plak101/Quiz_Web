using Microsoft.AspNetCore.Mvc;
using Quiz_Web.Models.Entities;
using Quiz_Web.Services.IServices;

namespace Quiz_Web.Controllers
{
    public class FlashcardController : Controller
    {
        private readonly IFlashcardService _flashcardService;
        private readonly ILogger<FlashcardController> _logger;

        public FlashcardController(IFlashcardService flashcardService, ILogger<FlashcardController> logger)
        {
            _flashcardService = flashcardService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int setId)
        {
            if (setId <= 0)
            {
                _logger.LogWarning($"Invalid set ID: {setId}");
                return BadRequest("Invalid set ID");
            }

            var flashcardSet = await _flashcardService.GetFlashcardSetByIdAsync(setId);

            if (flashcardSet == null)
            {
                _logger.LogWarning($"Flashcard set with ID {setId} not found");
                return NotFound($"Flashcard set with ID {setId} not found");
            }

            if (flashcardSet.Flashcards == null || !flashcardSet.Flashcards.Any())
            {
                ViewBag.Message = "This flashcard set has no cards.";
                ViewBag.SetTitle = flashcardSet.Title;
                ViewBag.TotalCards = 0;
                return View(new List<Flashcard>());
            }

            ViewBag.SetTitle = flashcardSet.Title;
            ViewBag.TotalCards = flashcardSet.Flashcards.Count;
            ViewBag.SetId = setId;
            
            return View(flashcardSet.Flashcards.ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Finish(int setId)
        {
            if (setId <= 0)
            {
                _logger.LogWarning($"Invalid set ID for finish page: {setId}");
                return RedirectToAction("Index", "Home");
            }

            // Verify the set exists
            var setExists = await _flashcardService.FlashcardSetExistsAsync(setId);
            
            if (!setExists)
            {
                _logger.LogWarning($"Flashcard set with ID {setId} not found for finish page");
                return RedirectToAction("Index", "Home");
            }

            ViewBag.SetId = setId;
            return View();
        }
    }
}
