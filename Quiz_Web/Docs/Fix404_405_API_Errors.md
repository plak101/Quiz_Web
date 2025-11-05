# Fix 404/405 Errors - Course Progress API

## V?n ??

Khi xem video trong Learn Page, JavaScript g?i các API endpoints nh?ng nh?n l?i:
- **404 Not Found**: API route không tìm th?y
- **405 Method Not Allowed**: HTTP method không ???c phép

### L?i c? th?:
```
Failed to load resource: /api/course-progress/get-progress?courseSlug=lap-trinh-csharp:1
the server responded with a status of 404 ()

Failed to load resource: /api/course-progress/save-progress:1
the server responded with a status of 405 ()

Failed to load resource: /api/course-progress/mark-complete:1
the server responded with a status of 405 ()
```

## Nguyên nhân

### 1. Thi?u `AddControllers()` trong `Program.cs`
ASP.NET Core MVC Controllers (`AddControllersWithViews()`) không t? ??ng h? tr? API Controllers v?i attribute routing.

### 2. Thi?u `MapControllers()` trong routing pipeline
API Controllers c?n ???c mapped riêng bi?t v?i `app.MapControllers()`.

## Gi?i pháp

### B??c 1: Thêm `AddControllers()` vào services

**File**: `Quiz_Web/Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ? ADD THIS LINE
builder.Services.AddControllers();
```

**Gi?i thích**:
- `AddControllersWithViews()`: H? tr? MVC Controllers v?i Views
- `AddControllers()`: H? tr? API Controllers v?i attribute routing (`[ApiController]`, `[Route]`)

### B??c 2: Map API Controllers trong pipeline

**File**: `Quiz_Web/Program.cs`

```csharp
app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// ? ADD THIS LINE - Map API Controllers FIRST
app.MapControllers();

// Then map MVC routes
app.MapControllerRoute(
    name: "onboarding",
    pattern: "Onboarding/{action=Index}/{id?}",
    defaults: new { controller = "Onboarding" });
```

**Gi?i thích**:
- `app.MapControllers()`: Map t?t c? API Controllers v?i attribute routing
- **Quan tr?ng**: Ph?i g?i TR??C các `MapControllerRoute` ?? tránh conflict

### B??c 3: Ki?m tra API Controller structure

**File**: `Quiz_Web/Controllers/API/CourseProgressController.cs`

```csharp
[ApiController]
[Route("api/course-progress")]
[Authorize]
public class CourseProgressController : ControllerBase
{
    // GET: /api/course-progress/get-progress?courseSlug={slug}
    [HttpGet("get-progress")]
    public async Task<IActionResult> GetProgress([FromQuery] string courseSlug)
    {
        // ...
    }

    // POST: /api/course-progress/save-progress
    [HttpPost("save-progress")]
    public async Task<IActionResult> SaveProgress([FromBody] SaveProgressRequest request)
    {
        // ...
    }

    // POST: /api/course-progress/mark-complete
    [HttpPost("mark-complete")]
    public async Task<IActionResult> MarkComplete([FromBody] MarkCompleteRequest request)
    {
        // ...
    }
}
```

**Các ?i?m quan tr?ng**:
- ? `[ApiController]`: B?t API-specific behaviors (auto model validation, problem details)
- ? `[Route("api/course-progress")]`: Base route cho toàn b? controller
- ? `[HttpGet("get-progress")]`: Sub-route + HTTP method
- ? `[FromQuery]`: Bind parameters t? query string
- ? `[FromBody]`: Bind request body to model

## Testing

### 1. Test v?i Browser DevTools

M? F12 ? Console, ch?y:

```javascript
// Test GET
fetch('/api/course-progress/get-progress?courseSlug=lap-trinh-csharp')
  .then(r => r.json())
  .then(console.log);

// Test POST save-progress
fetch('/api/course-progress/save-progress', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    courseSlug: 'lap-trinh-csharp',
    lessonId: 1,
    watchedDuration: 120,
    totalDuration: 300
  })
})
.then(r => r.json())
.then(console.log);

// Test POST mark-complete
fetch('/api/course-progress/mark-complete', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    courseSlug: 'lap-trinh-csharp',
    lessonId: 1,
    watchedDuration: 300
  })
})
.then(r => r.json())
.then(console.log);
```

**Expected Results**:
- Status: `200 OK`
- Response body: `{ success: true, ... }`

### 2. Test v?i Postman/Thunder Client

#### GET Progress
```
GET https://localhost:7158/api/course-progress/get-progress?courseSlug=lap-trinh-csharp
Headers:
  Cookie: [your auth cookie]
```

**Expected Response**:
```json
{
  "success": true,
  "completionPercentage": 25.5,
  "completedLessons": [1, 3, 5],
  "totalLessons": 10
}
```

#### POST Save Progress
```
POST https://localhost:7158/api/course-progress/save-progress
Headers:
  Content-Type: application/json
  Cookie: [your auth cookie]
Body:
{
  "courseSlug": "lap-trinh-csharp",
  "lessonId": 5,
  "watchedDuration": 120,
  "totalDuration": 300
}
```

**Expected Response**:
```json
{
  "success": true,
  "message": "Progress saved"
}
```

#### POST Mark Complete
```
POST https://localhost:7158/api/course-progress/mark-complete
Headers:
  Content-Type: application/json
  Cookie: [your auth cookie]
Body:
{
  "courseSlug": "lap-trinh-csharp",
  "lessonId": 5,
  "watchedDuration": 300
}
```

**Expected Response**:
```json
{
  "success": true,
  "message": "Lesson marked as complete"
}
```

## Common Issues

### Issue 1: 401 Unauthorized

**Error**:
```json
{
  "success": false,
  "message": "Unauthorized"
}
```

**Cause**: User not logged in or authentication cookie expired

**Solution**:
1. Check if user is logged in
2. Check cookie expiration time
3. Re-login if necessary

### Issue 2: 404 Not Found

**Error**: `Failed to load resource: the server responded with a status of 404 ()`

**Cause**: 
- Missing `AddControllers()` in `Program.cs`
- Missing `MapControllers()` in pipeline
- Wrong route in controller

**Solution**: Apply fixes from above

### Issue 3: 405 Method Not Allowed

**Error**: `the server responded with a status of 405 ()`

**Cause**: 
- Using GET instead of POST
- Missing HTTP verb attribute (`[HttpPost]`)

**Solution**: Check HTTP method matches attribute

### Issue 4: 400 Bad Request

**Error**:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "courseSlug": ["The courseSlug field is required."]
  }
}
```

**Cause**: Missing required parameters in request

**Solution**: Ensure all required fields are sent

## Debugging Tips

### 1. Check API is registered

Add logging in `Program.cs`:

```csharp
var app = builder.Build();

// Log all registered routes
var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();
foreach (var endpoint in endpointDataSource.Endpoints)
{
    if (endpoint is RouteEndpoint routeEndpoint)
    {
        Console.WriteLine($"Route: {routeEndpoint.RoutePattern.RawText}");
    }
}

app.Run();
```

Look for: `/api/course-progress/get-progress`, etc.

### 2. Enable detailed errors

In `Program.cs`:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
```

### 3. Check Network tab in DevTools

- Status code
- Request headers
- Request payload
- Response body

### 4. Add logging in controller

```csharp
[HttpPost("save-progress")]
public async Task<IActionResult> SaveProgress([FromBody] SaveProgressRequest request)
{
    _logger.LogInformation("SaveProgress called: {@Request}", request);
    // ...
}
```

## Architecture Notes

### Why separate API Controllers?

**MVC Controllers** (`Controller`):
- Return Views
- Handle HTML forms
- Session management
- ViewBag, ViewData

**API Controllers** (`ControllerBase`):
- Return JSON/XML
- RESTful endpoints
- Stateless
- Used by JavaScript fetch/AJAX

### Route Precedence

Routes are matched in order:
1. Attribute routes (`[Route]`, `[HttpGet]`)
2. Conventional routes (`MapControllerRoute`)

API Controllers with `[Route]` take precedence over conventional routes.

## Summary

### Changes Made

| File | Change | Reason |
|------|--------|--------|
| `Program.cs` | Added `AddControllers()` | Enable API Controllers support |
| `Program.cs` | Added `MapControllers()` | Map attribute-routed controllers |
| `CourseProgressController.cs` | Verified route attributes | Ensure correct URL patterns |

### Results

- ? **404 Fixed**: API endpoints now accessible
- ? **405 Fixed**: HTTP methods properly mapped
- ? **Progress Tracking**: Auto-save every 5s works
- ? **Mark Complete**: Button functionality works
- ? **Progress Bar**: Updates correctly

## Next Steps

1. **Test thoroughly**: Try all 3 endpoints
2. **Check logs**: Look for any errors
3. **Monitor database**: Verify records are created
4. **User feedback**: Toast notifications work

If issues persist, check:
- Database connection string
- `CourseProgress` table exists
- User is authenticated
- Lesson IDs are correct

---

**Last Updated**: 2024
**Status**: ? Fixed
**Tested**: Chrome, Firefox, Edge
