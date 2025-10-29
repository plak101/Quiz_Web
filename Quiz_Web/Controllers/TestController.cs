using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quiz_Web.Models.EF;
using Quiz_Web.Services.IServices;
using System.Security.Claims;

namespace Quiz_Web.Controllers
{
    public class TestController : Controller
    {
        private readonly LearningPlatformContext _context;
        private readonly ITestService _testService;

        public TestController(LearningPlatformContext context, ITestService testService)
        {
            _context = context;
            _testService = testService;
        }

        [HttpGet]
        [Route("/admin/Test")]
        public IActionResult Test()
        {
            return View("~/Views/Test/Test.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> UpcomingTests()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                if (userId == 0)
                {
                    return PartialView("_UpcomingTestsPartial", new List<Models.Entities.TestAssignment>());
                }

                var now = DateTime.UtcNow;

                // Get test assignments from classes where user is a student
                // Tests that haven't started yet or are scheduled for the future
                var upcomingTests = await _context.TestAssignments
                    .Include(ta => ta.Test)
                        .ThenInclude(t => t.Owner)
                    .Include(ta => ta.Assignment)
                        .ThenInclude(a => a.Class)
                    .Where(ta => _context.ClassStudents
                        .Any(cs => cs.StudentId == userId && cs.ClassId == ta.Assignment.ClassId)
                        && (ta.StartAt == null || ta.StartAt > now)
                        && ta.Assignment.DueAt > now)
                    .OrderBy(ta => ta.StartAt ?? ta.Assignment.DueAt)
                    .ToListAsync();

                return PartialView("_UpcomingTestsPartial", upcomingTests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error loading upcoming tests");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DueTests()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                if (userId == 0)
                {
                    return PartialView("_DueTestsPartial", new List<Models.Entities.TestAssignment>());
                }

                var now = DateTime.UtcNow;

                // Tests that are currently available but not yet completed
                var dueTests = await _context.TestAssignments
                    .Include(ta => ta.Test)
                        .ThenInclude(t => t.Owner)
                    .Include(ta => ta.Assignment)
                        .ThenInclude(a => a.Class)
                    .Where(ta => _context.ClassStudents
                        .Any(cs => cs.StudentId == userId && cs.ClassId == ta.Assignment.ClassId)
                        && (ta.StartAt == null || ta.StartAt <= now)
                        && ta.Assignment.DueAt > now
                        && !_context.TestAttempts.Any(attempt =>
                            attempt.TestId == ta.TestId
                            && attempt.UserId == userId
                            && attempt.Status == "completed"))
                    .OrderBy(ta => ta.Assignment.DueAt)
                    .ToListAsync();

                return PartialView("_DueTestsPartial", dueTests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error loading due tests");
            }
        }

        [HttpGet]
        public async Task<IActionResult> CompletedTests()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

                if (userId == 0)
                {
                    return PartialView("_CompletedTestsPartial", new List<Models.Entities.TestAttempt>());
                }

                // Get completed test attempts
                var completedTests = await _context.TestAttempts
                    .Include(ta => ta.Test)
                        .ThenInclude(t => t.Owner)
                    .Where(ta => ta.UserId == userId && ta.Status == "completed")
                    .OrderByDescending(ta => ta.SubmittedAt)
                    .ToListAsync();

                return PartialView("_CompletedTestsPartial", completedTests);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error loading completed tests");
            }
        }
    }
}
