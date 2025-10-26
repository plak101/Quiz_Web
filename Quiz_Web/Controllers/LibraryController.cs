﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quiz_Web.Models.EF;
using Quiz_Web.Services.IServices;
using System.Security.Claims;

namespace Quiz_Web.Controllers
{
    public class LibraryController : Controller
    {
        private readonly LearningPlatformContext _context;
        private readonly ILibraryService _libraryService;

        public LibraryController(LearningPlatformContext context, ILibraryService libraryService)
        {
            _context = context;
            _libraryService = libraryService;
        }

        [HttpGet]
        [Route("/admin/Library")]
        public IActionResult Library()
        {
            return View("~/Views/Library/Library.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> AllCourses()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                
                if (userId == 0)
                {
                    return PartialView("_AllCoursesPartial", new List<Models.Entities.Course>());
                }

                var purchasedCourses = await _context.CoursePurchases
                    .Where(cp => cp.BuyerId == userId && cp.Status == "completed")
                    .Include(cp => cp.Course)
                        .ThenInclude(c => c.Owner)
                    .Select(cp => cp.Course)
                    .ToListAsync();

                return PartialView("_AllCoursesPartial", purchasedCourses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error loading courses");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Wishlist()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                
                if (userId == 0)
                {
                    return PartialView("_WishlistPartial", new List<Models.Entities.Course>());
                }

                var wishlistCourses = await _context.SavedItems
                    .Where(si => si.Library.OwnerId == userId && si.ContentType == "course")
                    .Include(si => si.Library)
                    .Select(si => _context.Courses
                        .Include(c => c.Owner)
                        .FirstOrDefault(c => c.CourseId == si.ContentId))
                    .Where(c => c != null)
                    .ToListAsync();

                return PartialView("_WishlistPartial", wishlistCourses);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error loading wishlist");
            }
        }
    }
}
