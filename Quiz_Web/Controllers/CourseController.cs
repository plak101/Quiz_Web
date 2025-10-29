using Microsoft.AspNetCore.Mvc;
using Quiz_Web.Models.ViewModels;
using Quiz_Web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Ganss.Xss;

namespace Quiz_Web.Controllers
{
    public class CourseController : Controller
    {
        private readonly ILogger<CourseController> _logger;
        private readonly ICourseService _courseService;
        private readonly IWebHostEnvironment _env;

        public CourseController(
            ILogger<CourseController> logger,
            ICourseService courseService,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _courseService = courseService;
            _env = env;
        }

        // GET: /courses
        [Route("/courses")]
        [HttpGet]
        public IActionResult Index(string? search, string? category, int page = 1, int pageSize = 12)
        {
            _logger.LogInformation($"Courses Index - Search: {search}, Category: {category}, Page: {page}");

            List<Quiz_Web.Models.Entities.Course> courses;

            if (!string.IsNullOrWhiteSpace(search))
            {
                courses = _courseService.SearchCourses(search);
                ViewBag.SearchKeyword = search;
            }
            else if (!string.IsNullOrWhiteSpace(category))
            {
                courses = _courseService.GetCoursesByCategory(category);
                ViewBag.Category = category;
            }
            else
            {
                courses = _courseService.GetAllPublishedCourses();
            }

            var totalCount = courses.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pagedCourses = courses.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;

            return View(pagedCourses);
        }

        // GET: /courses/search?q=keyword
        [Route("/courses/search")]
        [HttpGet]
        public IActionResult Search(string q, int page = 1, int pageSize = 12)
        {
            _logger.LogInformation($"Course Search - Query: {q}");
            if (string.IsNullOrWhiteSpace(q))
                return RedirectToAction(nameof(Index));

            var courses = _courseService.SearchCourses(q);

            var totalCount = courses.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var pagedCourses = courses.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.SearchKeyword = q;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Category = null;

            return View("Index", pagedCourses);
        }

        // GET: /courses/create
        [Authorize]
        [Route("/courses/create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /courses/create
        [Authorize]
        [Route("/courses/create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCourseViewModel model, IFormFile? coverFile, [FromServices] HtmlSanitizer sanitizer)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.Description = sanitizer.Sanitize(model.Description ?? string.Empty);

            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Challenge();
            }

            if (coverFile is { Length: > 0 })
            {
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(coverFile.FileName).ToLowerInvariant();
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError(nameof(model.CoverUrl), "Định dạng ảnh không hợp lệ (jpg, jpeg, png, gif, webp).");
                    return View(model);
                }

                var folder = $"uploads/courses/{DateTime.UtcNow:yyyy/MM}";
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

            if (!_courseService.IsSlugUnique(model.Slug))
            {
                ModelState.AddModelError("Slug", "Slug này đã tồn tại. Vui lòng chọn slug khác.");
                return View(model);
            }

            var course = _courseService.CreateCourse(model, userId);

            if (course == null)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tạo khóa học";
                return View(model);
            }

            TempData["Success"] = "Tạo khóa học thành công!";
            return RedirectToAction("Detail", new { slug = course.Slug });
        }

        // GET: /courses/mine - list all courses created by current user
        [Authorize]
        [Route("/courses/mine")]
        [HttpGet]
        public IActionResult My()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Challenge();

            var courses = _courseService.GetCoursesByOwner(userId);
            return View("My", courses);
        }

        // GET: /courses/edit/{id}
        [Authorize]
        [Route("/courses/edit/{id:int}")]
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Challenge();

            var course = _courseService.GetOwnedCourse(id, userId);
            if (course == null) return NotFound();

            var vm = new EditCourseViewModel
            {
                CourseId = course.CourseId,
                Title = course.Title,
                Slug = course.Slug,
                Description = course.Summary,
                Price = course.Price,
                //Currency = course.Currency,
                IsPublished = course.IsPublished,
                CoverUrl = course.CoverUrl
            };
            return View("Edit", vm);
        }

        // POST: /courses/edit/{id}
        [Authorize]
        [Route("/courses/edit/{id:int}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCourseViewModel model, IFormFile? coverFile, [FromServices] HtmlSanitizer sanitizer)
        {
            if (id != model.CourseId) return BadRequest();

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
                    ModelState.AddModelError(nameof(model.CoverUrl), "Định dạng ảnh không hợp lệ (jpg, jpeg, png, gif, webp).");
                    return View("Edit", model);
                }

                var folder = $"uploads/courses/{DateTime.UtcNow:yyyy/MM}";
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

            // slug unique check excluding current course
            var slugClash = _courseService
                .GetCoursesByOwner(userId)
                .Any(c => c.Slug == model.Slug && c.CourseId != model.CourseId);
            if (slugClash)
            {
                ModelState.AddModelError(nameof(model.Slug), "Slug này đã tồn tại.");
                return View("Edit", model);
            }

            var sanitized = sanitizer.Sanitize(model.Description ?? string.Empty);
            var updated = _courseService.UpdateCourse(model, userId, sanitized);
            if (updated == null)
            {
                TempData["Error"] = "Không thể cập nhật khóa học.";
                return View("Edit", model);
            }

            TempData["Success"] = "Cập nhật khóa học thành công!";
            return RedirectToAction("Detail", new { slug = updated.Slug });
        }

        // GET: /courses/category/{category}
        [Route("/courses/category/{category}")]
        [HttpGet]
        public IActionResult Category(string category)
        {
            _logger.LogInformation($"Course Category - Category: {category}");

            if (string.IsNullOrWhiteSpace(category))
            {
                return RedirectToAction(nameof(Index));
            }

            var courses = _courseService.GetCoursesByCategory(category);
            ViewBag.Category = category;

            return View("Index", courses);
        }

        // GET: /courses/{slug}
        [Route("/courses/{slug}")]
        [HttpGet]
        public IActionResult Detail(string slug)
        {
            _logger.LogInformation($"Course Detail - Slug: {slug}");
            if (string.IsNullOrWhiteSpace(slug))
                return RedirectToAction(nameof(Index));

            var course = _courseService.GetCourseBySlug(slug);
            if (course == null)
                return RedirectToAction(nameof(Index));

            var isOwner = false;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
                isOwner = course.OwnerId == userId;

            ViewBag.IsOwner = isOwner;
            return View(course);
        }

        // POST: /courses/{id}/enroll (Future feature)
        [Route("/courses/{id:int}/enroll")]
        [HttpPost]
        public IActionResult Enroll(int id)
        {
            _logger.LogInformation($"Enroll attempt for course ID: {id}");
            TempData["Info"] = "Tính năng đăng ký khóa học đang được phát triển!";
            return RedirectToAction(nameof(Detail), new { id });
        }

        // POST: /courses/delete/{id}
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/courses/delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Challenge();

            var ok = _courseService.DeleteCourse(id, userId, _env.WebRootPath);
            if (!ok)
                TempData["Error"] = "Không thể xóa khóa học.";
            else
                TempData["Success"] = "Đã xóa khóa học.";

            return RedirectToAction(nameof(My));
        }
    }
}