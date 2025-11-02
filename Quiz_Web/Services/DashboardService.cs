using Quiz_Web.Models.EF;
using Quiz_Web.Models.ViewModels;
using Quiz_Web.Services.IServices;

namespace Quiz_Web.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly LearningPlatformContext _context;

        public DashboardService(LearningPlatformContext context)
        {
            _context = context;
        }

        public DashboardOverviewViewModel GetOverviewData()
        {
            var totalUsers = _context.Users.Count();
            var totalCourses = _context.Courses.Count();
            var totalTests = _context.Tests.Count();
            var totalRevenue = _context.Payments.Where(p => p.Status == "completed").Sum(p => (decimal?)p.Amount) ?? 0;

            var userGrowthData = _context.Users
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToList()
                .Select(g => new ChartDataPoint
                {
                    Label = $"{g.Month}/{g.Year}",
                    Value = g.Count,
                    Date = new DateTime(g.Year, g.Month, 1)
                }).OrderBy(x => x.Date).ToList();

            var revenueData = _context.Payments
                .Where(p => p.Status == "completed" && p.PaidAt.HasValue)
                .GroupBy(p => new { p.PaidAt.Value.Year, p.PaidAt.Value.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(p => p.Amount) })
                .ToList()
                .Select(g => new ChartDataPoint
                {
                    Label = $"{g.Month}/{g.Year}",
                    Value = g.Total,
                    Date = new DateTime(g.Year, g.Month, 1)
                }).OrderBy(x => x.Date).ToList();

            return new DashboardOverviewViewModel
            {
                TotalUsers = totalUsers,
                TotalCourses = totalCourses,
                TotalTests = totalTests,
                TotalRevenue = totalRevenue,
                UserGrowthData = userGrowthData,
                RevenueData = revenueData
            };
        }

        public UserAnalyticsViewModel GetUserAnalytics()
        {
            var roleData = _context.Users
                .Join(_context.Roles, u => u.RoleId, r => r.RoleId, (u, r) => r.Name)
                .GroupBy(roleName => roleName)
                .Select(g => new { RoleName = g.Key, Count = g.Count() })
                .ToList();

            var colors = new[] { "#dc3545", "#28a745", "#007bff", "#ffc107", "#17a2b8", "#fd7e14", "#6f42c1" };
            var usersByRole = roleData.Select((g, index) => new PieChartData
            {
                Label = g.RoleName ?? "Unknown",
                Value = g.Count,
                Color = index < colors.Length ? colors[index] : "#6c757d"
            }).ToList();

            var newUsersPerMonth = _context.Users
                .Where(u => u.CreatedAt >= DateTime.Now.AddMonths(-12))
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToList()
                .Select(g => new ChartDataPoint
                {
                    Label = $"{g.Month}/{g.Year}",
                    Value = g.Count,
                    Date = new DateTime(g.Year, g.Month, 1)
                }).OrderBy(x => x.Date).ToList();

            var activeUsersData = _context.TestAttempts
                .Where(ta => ta.StartedAt >= DateTime.Now.AddDays(-30))
                .GroupBy(ta => ta.StartedAt.Date)
                .Select(g => new { Date = g.Key, UserIds = g.Select(ta => ta.UserId).Distinct() })
                .ToList()
                .Select(g => new ChartDataPoint
                {
                    Label = g.Date.ToString("dd/MM"),
                    Value = g.UserIds.Count(),
                    Date = g.Date
                }).OrderBy(x => x.Date).ToList();

            return new UserAnalyticsViewModel
            {
                UsersByRole = usersByRole,
                NewUsersPerMonth = newUsersPerMonth,
                ActiveUsersData = activeUsersData,
                TotalActiveUsers = _context.TestAttempts.Where(ta => ta.StartedAt >= DateTime.Now.AddDays(-30)).Select(ta => ta.UserId).Distinct().Count(),
                NewUsersThisMonth = _context.Users.Count(u => u.CreatedAt.Month == DateTime.Now.Month && u.CreatedAt.Year == DateTime.Now.Year)
            };
        }

        public LearningActivitiesViewModel GetLearningActivities()
        {
            var testsCompletedData = _context.TestAttempts
                .Where(ta => ta.SubmittedAt.HasValue && ta.SubmittedAt >= DateTime.Now.AddMonths(-6))
                .GroupBy(ta => new { ta.SubmittedAt.Value.Year, ta.SubmittedAt.Value.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToList()
                .Select(g => new ChartDataPoint
                {
                    Label = $"{g.Month}/{g.Year}",
                    Value = g.Count,
                    Date = new DateTime(g.Year, g.Month, 1)
                }).OrderBy(x => x.Date).ToList();

            var coursesEnrolledData = _context.CoursePurchases
                .Where(cp => cp.PurchasedAt >= DateTime.Now.AddMonths(-6))
                .GroupBy(cp => new { cp.PurchasedAt.Year, cp.PurchasedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToList()
                .Select(g => new ChartDataPoint
                {
                    Label = $"{g.Month}/{g.Year}",
                    Value = g.Count,
                    Date = new DateTime(g.Year, g.Month, 1)
                }).OrderBy(x => x.Date).ToList();

            var popularCourses = _context.CoursePurchases
                .Join(_context.Courses, cp => cp.CourseId, c => c.CourseId, (cp, c) => c.Title)
                .GroupBy(title => title)
                .Select(g => new { Title = g.Key, Count = g.Count() })
                .ToList()
                .Select(g => new BarChartData
                {
                    Label = g.Title ?? "Unknown",
                    Value = g.Count,
                    Category = "Enrollments"
                }).OrderByDescending(x => x.Value).Take(10).ToList();

            return new LearningActivitiesViewModel
            {
                TestsCompletedData = testsCompletedData,
                CoursesEnrolledData = coursesEnrolledData,
                PopularCourses = popularCourses,
                TotalTestsCompleted = _context.TestAttempts.Count(ta => ta.SubmittedAt.HasValue),
                TotalEnrollments = _context.CoursePurchases.Count()
            };
        }

        public RevenuePaymentsViewModel GetRevenuePayments()
        {
            var monthlyRevenue = _context.Payments
                .Where(p => p.Status == "completed" && p.PaidAt.HasValue && p.PaidAt >= DateTime.Now.AddMonths(-12))
                .GroupBy(p => new { p.PaidAt.Value.Year, p.PaidAt.Value.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(p => p.Amount) })
                .ToList()
                .Select(g => new ChartDataPoint
                {
                    Label = $"{g.Month}/{g.Year}",
                    Value = g.Total,
                    Date = new DateTime(g.Year, g.Month, 1)
                }).OrderBy(x => x.Date).ToList();

            var revenueBySource = _context.Payments
                .Where(p => p.Status == "completed")
                .GroupBy(p => p.Provider ?? "Unknown")
                .Select(g => new { Provider = g.Key, Total = g.Sum(p => p.Amount) })
                .ToList()
                .Select(g => new PieChartData
                {
                    Label = g.Provider,
                    Value = g.Total,
                    Color = GetPaymentMethodColor(g.Provider)
                }).ToList();

            var topSellingCourses = _context.CoursePurchases
                .Join(_context.Courses, cp => cp.CourseId, c => c.CourseId, (cp, c) => new { c.Title, cp.PricePaid })
                .GroupBy(x => x.Title)
                .Select(g => new { Title = g.Key, Total = g.Sum(x => x.PricePaid) })
                .ToList()
                .Select(g => new BarChartData
                {
                    Label = g.Title ?? "Unknown",
                    Value = g.Total,
                    Category = "Revenue"
                }).OrderByDescending(x => x.Value).Take(10).ToList();

            var currentMonthRevenue = _context.Payments
                .Where(p => p.Status == "completed" && p.PaidAt.HasValue && p.PaidAt.Value.Month == DateTime.Now.Month && p.PaidAt.Value.Year == DateTime.Now.Year)
                .Sum(p => (decimal?)p.Amount) ?? 0;

            var lastMonthRevenue = _context.Payments
                .Where(p => p.Status == "completed" && p.PaidAt.HasValue && p.PaidAt.Value.Month == DateTime.Now.AddMonths(-1).Month && p.PaidAt.Value.Year == DateTime.Now.AddMonths(-1).Year)
                .Sum(p => (decimal?)p.Amount) ?? 0;

            var monthlyGrowth = lastMonthRevenue > 0 ? ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0;

            return new RevenuePaymentsViewModel
            {
                MonthlyRevenue = monthlyRevenue,
                RevenueBySource = revenueBySource,
                TopSellingCourses = topSellingCourses,
                TotalRevenue = _context.Payments.Where(p => p.Status == "completed").Sum(p => (decimal?)p.Amount) ?? 0,
                MonthlyGrowth = monthlyGrowth
            };
        }

        public LearningResultsViewModel GetLearningResults()
        {
            var testScores = _context.TestAttempts
                .Where(ta => ta.Score.HasValue)
                .Select(ta => ta.Score.Value >= 80 ? "Excellent" : ta.Score.Value >= 60 ? "Good" : "Needs Improvement")
                .GroupBy(category => category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToList()
                .Select(g => new BarChartData
                {
                    Label = g.Category,
                    Value = g.Count,
                    Category = "Test Results"
                }).ToList();

            var certificatesByType = _context.Certificates
                .Join(_context.Courses, c => c.CourseId, course => course.CourseId, (c, course) => course.Title ?? "General")
                .GroupBy(title => title)
                .Select(g => new { Title = g.Key, Count = g.Count() })
                .ToList()
                .Select(g => new PieChartData
                {
                    Label = g.Title,
                    Value = g.Count,
                    Color = GetCategoryColor(g.Title)
                }).ToList();

            var topPerformers = _context.TestAttempts
                .Where(ta => ta.Score.HasValue)
                .Join(_context.Users, ta => ta.UserId, u => u.UserId, (ta, u) => new { u.FullName, ta.Score })
                .GroupBy(x => x.FullName)
                .Select(g => new { Name = g.Key, AvgScore = g.Average(x => x.Score ?? 0) })
                .ToList()
                .Select(g => new TableSummaryData
                {
                    Name = g.Name ?? "Unknown",
                    Value = g.AvgScore.ToString("F1"),
                    Status = g.AvgScore >= 80 ? "Excellent" : "Good",
                    Date = DateTime.Now
                }).OrderByDescending(x => decimal.Parse(x.Value)).Take(10).ToList();

            var avgScore = _context.TestAttempts.Where(ta => ta.Score.HasValue).Select(ta => ta.Score.Value).DefaultIfEmpty(0).Average();

            return new LearningResultsViewModel
            {
                TestScores = testScores,
                CertificatesByType = certificatesByType,
                TopPerformers = topPerformers,
                AverageScore = (double)avgScore,
                TotalCertificates = _context.Certificates.Count()
            };
        }

        public SystemActivityViewModel GetSystemActivity()
        {
            var loginActivity = _context.AuditLogs
                .Where(al => al.Action == "Login" && al.CreatedAt >= DateTime.Now.AddDays(-7))
                .GroupBy(al => al.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToList()
                .Select(g => new BarChartData
                {
                    Label = g.Date.ToString("dd/MM"),
                    Value = g.Count,
                    Category = "Logins"
                }).OrderBy(x => x.Label).ToList();

            var systemLogs = _context.ErrorLogs
                .OrderByDescending(el => el.CreatedAt)
                .Take(10)
                .Select(el => new { el.Message, el.Severity, el.CreatedAt })
                .ToList()
                .Select(el => new TableSummaryData
                {
                    Name = el.Message ?? "Unknown Error",
                    Value = el.Severity ?? "Info",
                    Status = el.Severity == "Error" ? "Critical" : "Normal",
                    Date = el.CreatedAt
                }).ToList();

            var errorRates = _context.ErrorLogs
                .Where(el => el.CreatedAt >= DateTime.Now.AddDays(-7))
                .GroupBy(el => el.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToList()
                .Select(g => new ChartDataPoint
                {
                    Label = g.Date.ToString("dd/MM"),
                    Value = g.Count,
                    Date = g.Date
                }).OrderBy(x => x.Date).ToList();

            return new SystemActivityViewModel
            {
                LoginActivity = loginActivity,
                SystemLogs = systemLogs,
                ErrorRates = errorRates,
                TotalLogins = _context.AuditLogs.Count(al => al.Action == "Login" && al.CreatedAt >= DateTime.Now.AddDays(-30)),
                SystemErrors = _context.ErrorLogs.Count(el => el.CreatedAt >= DateTime.Now.AddDays(-30))
            };
        }

        private string GetRoleColor(string? roleName)
        {
            var role = roleName?.Trim().ToLower();
            return role switch
            {
                "admin" => "#dc3545",     // Đỏ
                "teacher" => "#28a745",   // Xanh lá
                "student" => "#007bff",   // Xanh dương
                "moderator" => "#ffc107", // Vàng
                "guest" => "#17a2b8",     // Xanh cyan
                "instructor" => "#fd7e14", // Cam
                "manager" => "#6f42c1",   // Tím
                _ => "#6c757d"             // Xám
            };
        }

        private string GetPaymentMethodColor(string method) => method switch
        {
            "paypal" => "#ffc107",
            "stripe" => "#007bff",
            "vnpay" => "#28a745",
            "momo" => "#e91e63",
            _ => "#6c757d"
        };

        private string GetCategoryColor(string category) => category switch
        {
            "Technology" => "#007bff",
            "Business" => "#28a745",
            "Design" => "#dc3545",
            "Language" => "#ffc107",
            "Science" => "#17a2b8",
            _ => "#6c757d"
        };
    }
}