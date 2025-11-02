using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Quiz_Web.Models.EF;
using Quiz_Web.Services;
using Quiz_Web.Services.IServices;
using Ganss.Xss;
using Quiz_Web.Models.MoMoPayment;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure file upload limits for video upload (500MB)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 524_288_000; // 500MB
    options.ValueLengthLimit = 524_288_000;
    options.MultipartHeadersLengthLimit = 524_288_000;
});

// Configure Kestrel server limits
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 524_288_000; // 500MB
});

//session
builder.Services.AddSession(options=>
{
    options.IdleTimeout = TimeSpan.FromHours(3);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.Name = "Quiz";
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromHours(3);
        options.LoginPath = "/login";
    });

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddDbContext<LearningPlatformContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")),
    ServiceLifetime.Scoped);

// Configure MoMo settings
builder.Services.Configure<MoMoSettings>(builder.Configuration.GetSection("MoMoSettings"));

// Register HttpClient for MoMoPaymentService
builder.Services.AddHttpClient<IMoMoPaymentService, MoMoPaymentService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILibraryService, LibraryService>();
builder.Services.AddScoped<ITestService, TestService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IFlashcardService, FlashcardService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICreateTestService, CreateTestService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ICartService, CartService>();

// Register background service for course recommendations
builder.Services.AddHostedService<CourseRecommendationService>();

// Html sanitizer for CKEditor content
builder.Services.AddSingleton(sp =>
{
    var s = new HtmlSanitizer();
    s.AllowedSchemes.Add("data"); // allow data URLs if you paste images
    return s;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Add explicit route for Onboarding
app.MapControllerRoute(
    name: "onboarding",
    pattern: "Onboarding/{action=Index}/{id?}",
    defaults: new { controller = "Onboarding" });

// Route m?c ??nh tr? ??n Welcome action ?? x? l? logic
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}")
    .WithStaticAssets();

app.Run();
