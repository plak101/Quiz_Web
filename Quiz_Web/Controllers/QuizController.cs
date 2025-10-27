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
                .Where(t => t.Visibility == "public" && !t.IsDeleted)
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
                .FirstOrDefaultAsync(t => t.TestId == id && !t.IsDeleted);

            if (test == null)
            {
                return NotFound();
            }

            return View(test);
        }

        // Bắt đầu làm quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartAttempt(int testId)
        {
            var userId = GetCurrentUserId();
            
            if (userId == 0)
            {
                return Unauthorized();
            }

            var test = await _context.Tests.FindAsync(testId);
            if (test == null)
            {
                return NotFound();
            }

            var attempt = new TestAttempt
            {
                TestId = testId,
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                Status = "InProgress" // Changed from "in_progress" to match constraint
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

            if (attempt.Status == "Submitted")
            {
                return RedirectToAction("Result", new { attemptId = attempt.AttemptId });
            }

            return View(attempt);
        }

        // Nộp bài
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(int attemptId, Dictionary<int, string> answers)
        {
            try
            {
                _logger.LogInformation("SubmitQuiz called with attemptId: {AttemptId}, answers count: {AnswersCount}", 
                    attemptId, answers?.Count ?? 0);

                var attempt = await _context.TestAttempts
                    .Include(a => a.Test)
                        .ThenInclude(t => t.Questions)
                            .ThenInclude(q => q.QuestionOptions)
                    .FirstOrDefaultAsync(a => a.AttemptId == attemptId);

                if (attempt == null)
                {
                    _logger.LogWarning("Attempt not found: {AttemptId}", attemptId);
                    return Json(new { success = false, message = "Quiz attempt not found." });
                }

                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("Current user ID: {UserId}, Attempt user ID: {AttemptUserId}", 
                    currentUserId, attempt.UserId);

                if (attempt.UserId != currentUserId)
                {
                    _logger.LogWarning("Access denied. Current user: {CurrentUserId}, Attempt user: {AttemptUserId}", 
                        currentUserId, attempt.UserId);
                    return Json(new { success = false, message = "Access denied." });
                }

                if (attempt.Status == "Submitted")
                {
                    _logger.LogWarning("Quiz already submitted: {AttemptId}", attemptId);
                    return Json(new { success = false, message = "This quiz has already been submitted." });
                }

                // Tính điểm
                decimal totalScore = 0;
                decimal maxScore = 0;
                int questionsCount = attempt.Test.Questions.Count;

                _logger.LogInformation("Processing {QuestionsCount} questions", questionsCount);

                foreach (var question in attempt.Test.Questions)
                {
                    maxScore += question.Points;
                    
                    if (answers != null && answers.ContainsKey(question.QuestionId))
                    {
                        var userAnswer = answers[question.QuestionId];
                        var correctOption = question.QuestionOptions.FirstOrDefault(o => o.IsCorrect);
                        
                        bool isCorrect = correctOption != null && correctOption.OptionId.ToString() == userAnswer;
                        decimal questionScore = isCorrect ? question.Points : 0;
                        
                        if (isCorrect)
                        {
                            totalScore += question.Points;
                        }

                        _logger.LogDebug("Question {QuestionId}: User answer: {UserAnswer}, Correct: {IsCorrect}, Score: {Score}", 
                            question.QuestionId, userAnswer, isCorrect, questionScore);

                        // Lưu câu trả lời
                        var attemptAnswer = new AttemptAnswer
                        {
                            AttemptId = attemptId,
                            QuestionId = question.QuestionId,
                            AnswerPayload = userAnswer,
                            IsCorrect = isCorrect,
                            Score = questionScore,
                            AutoGraded = true,
                            GradedAt = DateTime.UtcNow
                        };
                        _context.AttemptAnswers.Add(attemptAnswer);
                    }
                }

                // Cập nhật attempt
                attempt.Score = totalScore;
                attempt.MaxScore = maxScore;
                attempt.SubmittedAt = DateTime.UtcNow;
                attempt.Status = "Submitted"; // Changed from "Completed" to "Submitted"
                attempt.TimeSpentSec = (int)(DateTime.UtcNow - attempt.StartedAt).TotalSeconds;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Quiz submitted successfully. AttemptId: {AttemptId}, Score: {Score}/{MaxScore}", 
                    attemptId, totalScore, maxScore);

                return Json(new { 
                    success = true, 
                    score = totalScore, 
                    maxScore = maxScore,
                    attemptId = attemptId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting quiz for attemptId: {AttemptId}", attemptId);
                return Json(new { success = false, message = "An error occurred while submitting the quiz. Error: " + ex.Message });
            }
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
            // Try to get UserId from NameIdentifier claim
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }

            // Fallback 1: NameIdentifier might contain username
            if (!string.IsNullOrEmpty(userIdClaim))
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == userIdClaim);
                if (user != null) return user.UserId;
            }

            // Fallback 2: Try to get from Name claim (FullName)
            var fullName = User.FindFirst(ClaimTypes.Name)?.Value;
            if (!string.IsNullOrEmpty(fullName))
            {
                var user = _context.Users.FirstOrDefault(u => u.FullName == fullName);
                if (user != null) return user.UserId;
            }

            // Fallback 3: Try Identity.Name
            var identityName = User.Identity?.Name;
            if (!string.IsNullOrEmpty(identityName))
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == identityName || u.FullName == identityName);
                if (user != null) return user.UserId;
            }

            return 0;
        }
    }
}