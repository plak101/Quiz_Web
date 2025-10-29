using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Quiz_Web.Models.Entities;
using Quiz_Web.Models.ViewModels;
using Quiz_Web.Services.IServices;

namespace Quiz_Web.Controllers
{
    [Route("flashcards")]
    public class FlashcardController : Controller
    {
        private readonly IFlashcardService _flashcardService;
        private readonly ILogger<FlashcardController> _logger;
        private readonly IWebHostEnvironment _env;

        public FlashcardController(
            IFlashcardService flashcardService, 
            ILogger<FlashcardController> logger,
            IWebHostEnvironment env)
        {
            _flashcardService = flashcardService;
            _logger = logger;
            _env = env;
        }

        // GET: /flashcards - List all public flashcard sets
        [HttpGet("")]
        public IActionResult Index(string? search, int page = 1, int pageSize = 12)
        {
            _logger.LogInformation($"Flashcards Index - Search: {search}, Page: {page}");

            List<FlashcardSet> flashcardSets;

            if (!string.IsNullOrWhiteSpace(search))
            {
                flashcardSets = _flashcardService.SearchFlashcardSets(search);
                ViewBag.SearchKeyword = search;
            }
            else
            {
                flashcardSets = _flashcardService.GetAllPublishedFlashcardSets();
            }

            var totalCount = flashcardSets.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pagedSets = flashcardSets.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;

            return View(pagedSets);
        }

        // GET: /flashcards/search?q=keyword
        [HttpGet("search")]
        public IActionResult Search(string q, int page = 1, int pageSize = 12)
        {
            _logger.LogInformation($"Flashcard Search - Query: {q}");
            if (string.IsNullOrWhiteSpace(q))
                return RedirectToAction(nameof(Index));

            var flashcardSets = _flashcardService.SearchFlashcardSets(q);

            var totalCount = flashcardSets.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pagedSets = flashcardSets.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.SearchKeyword = q;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;

            return View("Index", pagedSets);
        }

        // GET: /flashcards/mine - List all flashcard sets created by current user
        [Authorize]
        [HttpGet("mine")]
        public IActionResult My()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Challenge();

            var flashcardSets = _flashcardService.GetFlashcardSetsByOwner(userId);
            return View("My", flashcardSets);
        }

        // GET: /flashcards/create
        [Authorize]
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /flashcards/create
        [Authorize]
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateFlashcardSetViewModel model, IFormFile? coverFile)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Challenge();
            }

            // Handle cover image upload
            if (coverFile is { Length: > 0 })
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(coverFile.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError(nameof(model.CoverUrl), "??nh d?ng ?nh không h?p l? (jpg, jpeg, png, gif, webp).");
                    return View(model);
                }

                var folder = $"uploads/flashcards/{DateTime.UtcNow:yyyy/MM}";
                var physical = Path.Combine(_env.WebRootPath, folder);
                Directory.CreateDirectory(physical);

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(physical, fileName);

                await using (var stream = System.IO.File.Create(fullPath))
                {
                    await coverFile.CopyToAsync(stream);
                }

                model.CoverUrl = "/" + Path.Combine(folder, fileName).Replace("\\", "/");
            }

            var flashcardSet = _flashcardService.CreateFlashcardSet(model, userId);

            if (flashcardSet == null)
            {
                TempData["Error"] = "Có l?i x?y ra khi t?o b? flashcard";
                return View(model);
            }

            TempData["Success"] = "T?o b? flashcard thành công!";
            return RedirectToAction("Detail", new { id = flashcardSet.SetId });
        }

        // GET: /flashcards/{id} - View flashcard set detail
        [HttpGet("{id:int}")]
        public IActionResult Detail(int id)
        {
            _logger.LogInformation($"Flashcard Set Detail - ID: {id}");

            var flashcardSet = _flashcardService.GetFlashcardSetById(id);
            if (flashcardSet == null)
                return NotFound();

            var isOwner = false;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
                isOwner = flashcardSet.OwnerId == userId;

            ViewBag.IsOwner = isOwner;
            return View(flashcardSet);
        }

        // GET: /flashcards/edit/{id}
        [Authorize]
        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Challenge();

            var flashcardSet = _flashcardService.GetOwnedFlashcardSet(id, userId);
            if (flashcardSet == null) return NotFound();

            var vm = new EditFlashcardSetViewModel
            {
                SetId = flashcardSet.SetId,
                Title = flashcardSet.Title,
                Description = flashcardSet.Description,
                Visibility = flashcardSet.Visibility,
                CoverUrl = flashcardSet.CoverUrl,
                TagsText = flashcardSet.TagsText,
                Language = flashcardSet.Language
            };
            return View("Edit", vm);
        }

        // POST: /flashcards/edit/{id}
        [Authorize]
        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditFlashcardSetViewModel model, IFormFile? coverFile)
        {
            if (id != model.SetId) return BadRequest();

            if (!ModelState.IsValid)
                return View("Edit", model);

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Challenge();

            // upload new cover if provided
            if (coverFile is { Length: > 0 })
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(coverFile.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError(nameof(model.CoverUrl), "??nh d?ng ?nh không h?p l? (jpg, jpeg, png, gif, webp).");
                    return View("Edit", model);
                }

                var folder = $"uploads/flashcards/{DateTime.UtcNow:yyyy/MM}";
                var physical = Path.Combine(_env.WebRootPath, folder);
                Directory.CreateDirectory(physical);

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(physical, fileName);
                await using (var stream = System.IO.File.Create(fullPath))
                {
                    await coverFile.CopyToAsync(stream);
                }

                model.CoverUrl = "/" + Path.Combine(folder, fileName).Replace("\\", "/");
            }

            var updated = _flashcardService.UpdateFlashcardSet(model, userId);
            if (updated == null)
            {
                TempData["Error"] = "Không th? c?p nh?t b? flashcard.";
                return View("Edit", model);
            }

            TempData["Success"] = "C?p nh?t b? flashcard thành công!";
            return RedirectToAction("Detail", new { id = updated.SetId });
        }

        // POST: /flashcards/delete/{id}
        [Authorize]
        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Challenge();

            var ok = _flashcardService.DeleteFlashcardSet(id, userId, _env.WebRootPath);
            if (!ok)
                TempData["Error"] = "Không th? xóa b? flashcard.";
            else
                TempData["Success"] = "?ã xóa b? flashcard.";

            return RedirectToAction(nameof(My));
        }

        // Route: /flashcards/study/{setId}
        // Example: /flashcards/study/5
        [HttpGet]
        [Route ("/flashcard/study/{setId}")]
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

        // Route: /flashcards/finish/{setId}
        // Example: /flashcards/finish/5
        [HttpGet]
        [Route("/flashcards/finish/{setId}")]
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

        [HttpGet]
        [Route("/flashcard/explore")]
        public async Task<IActionResult> Explore()
        {
            var publicFlashcardSets = await _flashcardService.GetPublicFlashcardSetsAsync();
            return View(publicFlashcardSets);
        }
    }
}
