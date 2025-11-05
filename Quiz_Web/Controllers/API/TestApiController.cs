using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quiz_Web.Models.EF;
using System.Security.Claims;

namespace Quiz_Web.Controllers.API
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TestApiController : ControllerBase
    {
        private readonly LearningPlatformContext _context;
        private readonly ILogger<TestApiController> _logger;

        public TestApiController(LearningPlatformContext context, ILogger<TestApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/TestApi/{testId}
        [HttpGet("{testId}")]
        public async Task<IActionResult> GetTestQuestions(int testId)
        {
            try
            {
                var test = await _context.Tests
                    .Include(t => t.Questions.OrderBy(q => q.OrderIndex))
                        .ThenInclude(q => q.QuestionOptions.OrderBy(o => o.OrderIndex))
                    .FirstOrDefaultAsync(t => t.TestId == testId && !t.IsDeleted);

                if (test == null)
                {
                    return NotFound(new { success = false, message = "Không tìm th?y bài ki?m tra" });
                }

                // Check if test is open (if OpenAt and CloseAt are set)
                if (test.OpenAt.HasValue && DateTime.UtcNow < test.OpenAt.Value)
                {
                    return BadRequest(new { success = false, message = "Bài ki?m tra ch?a m?" });
                }

                if (test.CloseAt.HasValue && DateTime.UtcNow > test.CloseAt.Value)
                {
                    return BadRequest(new { success = false, message = "Bài ki?m tra ?ã ?óng" });
                }

                // Check max attempts
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var attemptCount = await _context.TestAttempts
                    .CountAsync(ta => ta.TestId == testId && ta.UserId == userId);

                if (test.MaxAttempts.HasValue && attemptCount >= test.MaxAttempts.Value)
                {
                    return BadRequest(new { success = false, message = "B?n ?ã h?t l??t làm bài" });
                }

                var response = new
                {
                    success = true,
                    test = new
                    {
                        testId = test.TestId,
                        title = test.Title,
                        description = test.Description,
                        timeLimitSec = test.TimeLimitSec,
                        maxScore = test.MaxScore,
                        questionCount = test.Questions.Count
                    },
                    questions = test.Questions.Select(q => new
                    {
                        questionId = q.QuestionId,
                        type = q.Type,
                        stemText = q.StemText,
                        points = q.Points,
                        orderIndex = q.OrderIndex,
                        options = q.QuestionOptions.Select(o => new
                        {
                            optionId = o.OptionId,
                            optionText = o.OptionText,
                            orderIndex = o.OrderIndex
                        }).ToList()
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading test {TestId}", testId);
                return StatusCode(500, new { success = false, message = "Có l?i x?y ra khi t?i bài ki?m tra" });
            }
        }

        // POST: api/TestApi/submit
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitTest([FromBody] SubmitTestRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                
                var test = await _context.Tests
                    .Include(t => t.Questions)
                        .ThenInclude(q => q.QuestionOptions)
                    .FirstOrDefaultAsync(t => t.TestId == request.TestId && !t.IsDeleted);

                if (test == null)
                {
                    return NotFound(new { success = false, message = "Không tìm th?y bài ki?m tra" });
                }

                // Create test attempt
                var attempt = new Models.Entities.TestAttempt
                {
                    TestId = request.TestId,
                    UserId = userId,
                    StartedAt = DateTime.UtcNow.AddSeconds(-request.TimeSpentSec),
                    SubmittedAt = DateTime.UtcNow,
                    Status = "Graded",
                    TimeSpentSec = request.TimeSpentSec,
                    MaxScore = test.MaxScore ?? test.Questions.Sum(q => q.Points)
                };

                _context.TestAttempts.Add(attempt);
                await _context.SaveChangesAsync();

                // Grade answers
                decimal totalScore = 0;
                int correctAnswersCount = 0;
                int totalQuestions = test.Questions.Count;

                foreach (var answer in request.Answers)
                {
                    var question = test.Questions.FirstOrDefault(q => q.QuestionId == answer.QuestionId);
                    if (question == null) continue;

                    bool isCorrect = false;
                    decimal questionScore = 0;

                    // ? NORMALIZE QUESTION TYPE
                    string questionType = NormalizeQuestionType(question.Type);

                    _logger.LogInformation("Grading QuestionId={0}, Type={1}, NormalizedType={2}", 
                        question.QuestionId, question.Type, questionType);

                    if (questionType == "MCQ_Single" || questionType == "TrueFalse")
                    {
                        var correctOption = question.QuestionOptions.FirstOrDefault(o => o.IsCorrect);
                        if (correctOption != null)
                        {
                            _logger.LogInformation("CorrectOptionId={0}, SelectedOptionId={1}", 
                                correctOption.OptionId, answer.SelectedOptionId);
                            
                            if (answer.SelectedOptionId == correctOption.OptionId)
                            {
                                isCorrect = true;
                                questionScore = question.Points;
                                correctAnswersCount++; // ? ??m câu ?úng
                            }
                        }
                    }
                    else if (questionType == "MCQ_Multi")
                    {
                        var correctOptionIds = question.QuestionOptions
                            .Where(o => o.IsCorrect)
                            .Select(o => o.OptionId)
                            .OrderBy(id => id)
                            .ToList();

                        var selectedIds = answer.SelectedOptionIds?
                            .OrderBy(id => id)
                            .ToList() ?? new List<int>();

                        _logger.LogInformation("CorrectOptions=[{0}], SelectedOptions=[{1}]", 
                            string.Join(",", correctOptionIds), string.Join(",", selectedIds));

                        if (correctOptionIds.SequenceEqual(selectedIds))
                        {
                            isCorrect = true;
                            questionScore = question.Points;
                            correctAnswersCount++; // ? ??m câu ?úng
                        }
                    }

                    totalScore += questionScore;

                    // Save attempt answer
                    var attemptAnswer = new Models.Entities.AttemptAnswer
                    {
                        AttemptId = attempt.AttemptId,
                        QuestionId = question.QuestionId,
                        AnswerPayload = System.Text.Json.JsonSerializer.Serialize(answer),
                        IsCorrect = isCorrect,
                        Score = questionScore,
                        AutoGraded = true,
                        GradedAt = DateTime.UtcNow
                    };

                    _context.AttemptAnswers.Add(attemptAnswer);
                }

                attempt.Score = totalScore;
                await _context.SaveChangesAsync();

                // Calculate percentage based on correct answers
                decimal percentage = totalQuestions > 0
                    ? ((decimal)correctAnswersCount / totalQuestions) * 100
                    : 0;

                _logger.LogInformation("Test {0} submitted: CorrectAnswers={1}/{2}, Score={3}/{4}, Percentage={5}%", 
                    test.TestId, correctAnswersCount, totalQuestions, totalScore, attempt.MaxScore, percentage);

                return Ok(new
                {
                    success = true,
                    attemptId = attempt.AttemptId,
                    score = totalScore,
                    maxScore = attempt.MaxScore,
                    correctAnswers = correctAnswersCount, // ? S? câu ?úng
                    totalQuestions = totalQuestions, // ? T?ng s? câu
                    percentage = Math.Round(percentage, 1),
                    message = percentage >= 60 ? "Chúc m?ng! B?n ?ã ??t!" : "B?n ch?a ??t. Hãy c? g?ng l?n sau!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting test");
                return StatusCode(500, new { success = false, message = "Có l?i x?y ra khi n?p bài" });
            }
        }

        /// <summary>
        /// Normalize question type to standard format
        /// </summary>
        private string NormalizeQuestionType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return "MCQ_Single";

            // Convert to lowercase for comparison
            string lowerType = type.Trim().ToLower();

            return lowerType switch
            {
                "mcq_single" or "multiple_choice" or "single_choice" or "tr?c nghi?m (1 ?áp án)" => "MCQ_Single",
                "mcq_multi" or "multiple_answer" or "multi_choice" or "tr?c nghi?m (nhi?u ?áp án)" => "MCQ_Multi",
                "truefalse" or "true_false" or "?úng/sai" or "?úng sai" => "TrueFalse",
                "shorttext" or "short_text" or "short_answer" or "câu h?i ng?n" => "ShortText",
                "cloze" or "fill_in_blank" or "?i?n khuy?t" => "Cloze",
                "range" or "number_range" => "Range",
                _ => type // Return original if no match
            };
        }
    }

    public class SubmitTestRequest
    {
        public int TestId { get; set; }
        public int TimeSpentSec { get; set; }
        public List<TestAnswerDto> Answers { get; set; } = new();
    }

    public class TestAnswerDto
    {
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }
        public List<int>? SelectedOptionIds { get; set; }
    }
}
