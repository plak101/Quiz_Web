using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quiz_Web.Models.EF;
using Quiz_Web.Models.Entities;
using System.Security.Claims;

namespace Quiz_Web.Controllers
{
    [Authorize]
    public class QuizController : Controller
    {
        private readonly LearningPlatformContext _context;
        private readonly ILogger<QuizController> _logger;

        public QuizController(LearningPlatformContext context, ILogger<QuizController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Danh sách quiz
        public async Task<IActionResult> Index()
        {
            var tests = await _context.Tests
                .Include(t => t.Owner)
                .Where(t => t.Visibility == "public")
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tests);
        }

        // Chi tiết quiz
        public async Task<IActionResult> Details(int id)
        {
            var test = await _context.Tests
                .Include(t => t.Owner)
                .Include(t => t.Questions)
                    .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(t => t.TestId == id);

            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // Bắt đầu làm quiz
        [HttpPost]
        public async Task<IActionResult> StartAttempt(int testId)
        {
            var userId = GetCurrentUserId();
            
            var attempt = new TestAttempt
            {
                TestId = testId,
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                Status = "in_progress"
            };

            _context.TestAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            return RedirectToAction("TakeQuiz", new { attemptId = attempt.AttemptId });
        }

        // Làm quiz
        public async Task<IActionResult> TakeQuiz(int attemptId)
        {
            var attempt = await _context.TestAttempts
                .Include(a => a.Test)
                    .ThenInclude(t => t.Questions)
                        .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId);

            if (attempt == null || attempt.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            return View(attempt);
        }

        // Nộp bài
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(int attemptId, Dictionary<int, string> answers)
        {
            var attempt = await _context.TestAttempts
                .Include(a => a.Test)
                    .ThenInclude(t => t.Questions)
                        .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId);

            if (attempt == null || attempt.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            // Tính điểm
            decimal totalScore = 0;
            decimal maxScore = 0;

            foreach (var question in attempt.Test.Questions)
            {
                maxScore += question.Points;
                
                if (answers.ContainsKey(question.QuestionId))
                {
                    var userAnswer = answers[question.QuestionId];
                    var correctOption = question.QuestionOptions.FirstOrDefault(o => o.IsCorrect);
                    
                    if (correctOption != null && correctOption.OptionId.ToString() == userAnswer)
                    {
                        totalScore += question.Points;
                    }

                    // Lưu câu trả lời
                    var attemptAnswer = new AttemptAnswer
                    {
                        AttemptId = attemptId,
                        QuestionId = question.QuestionId,
                        AnswerPayload = userAnswer,
                        Score = correctOption != null && correctOption.OptionId.ToString() == userAnswer ? question.Points : 0
                    };
                    _context.AttemptAnswers.Add(attemptAnswer);
                }
            }

            // Cập nhật attempt
            attempt.Score = totalScore;
            attempt.MaxScore = maxScore;
            attempt.SubmittedAt = DateTime.UtcNow;
            attempt.Status = "completed";

            await _context.SaveChangesAsync();

            return RedirectToAction("Result", new { attemptId });
        }

        // Kết quả quiz
        public async Task<IActionResult> Result(int attemptId)
        {
            var attempt = await _context.TestAttempts
                .Include(a => a.Test)
                .Include(a => a.AttemptAnswers)
                    .ThenInclude(aa => aa.Question)
                        .ThenInclude(q => q.QuestionOptions)
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId);

            if (attempt == null || attempt.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            return View(attempt);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            
            // Fallback: tìm user theo username
            var username = User.Identity?.Name;
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            return user?.UserId ?? 0;
        }
    }
}