# Fix: API 404 Errors - Missing Endpoint

## ?? V?n ??

Console hi?n th? l?i 404 khi load trang learn flashcard:

```
Failed to load resource: POST /api/course-progress/mark-complete:1 - 404 (Not Found)
Failed to load resource: GET /api/course-progress?contentId=2002:1 - 404 (Not Found)
Error checking flashcard completion: SyntaxError: Failed to execute 'json' on 'Response': Unexpected end of JSON input
```

## ?? Root Cause Analysis

### Missing API Endpoint

JavaScript code trong `course-learn.js` g?i API endpoint:

```javascript
async function checkFlashcardCompletion(contentId, courseSlug, lessonId) {
    const response = await fetch(
        `/api/course-progress/check-content-completion?courseSlug=${courseSlug}&lessonId=${lessonId}&contentId=${contentId}`
    );
    // ...
}
```

**Nh?ng endpoint này KHÔNG T?N T?I trong `CourseProgressController.cs`!**

### Available Endpoints Before Fix

```csharp
? GET  /api/course-progress/get-progress
? POST /api/course-progress/save-progress
? POST /api/course-progress/mark-complete
? POST /api/course-progress/mark-content-complete
? GET  /api/course-progress/debug-progress
? POST /api/course-progress/cleanup-progress

? GET  /api/course-progress/check-content-completion  <-- MISSING!
```

### Error Flow

1. User loads flashcard in course learn page
2. JavaScript calls `loadFlashcardSet()`
3. At end of load, calls `checkFlashcardCompletion()`
4. Tries to fetch `/api/course-progress/check-content-completion`
5. **404 Not Found** because endpoint doesn't exist
6. JavaScript tries to parse response as JSON
7. **Unexpected end of JSON input** because 404 page is HTML, not JSON

## ? Solution

### Add Missing Endpoint

**File**: `Quiz_Web/Controllers/API/CourseProgressController.cs`

```csharp
// GET: /api/course-progress/check-content-completion
[HttpGet("check-content-completion")]
public async Task<IActionResult> CheckContentCompletion(
    [FromQuery] string courseSlug, 
    [FromQuery] int lessonId, 
    [FromQuery] int contentId)
{
    try
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { success = false, message = "Unauthorized" });
        }

        // Get course by slug
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Slug == courseSlug);

        if (course == null)
        {
            return NotFound(new { success = false, message = "Course not found" });
        }

        // Check if content completion exists for this user
        var isCompleted = await _context.CourseProgresses
            .AnyAsync(p => 
                p.CourseId == course.CourseId && 
                p.UserId == userId && 
                p.LessonId == lessonId &&
                p.ContentId == contentId &&
                p.IsCompleted);

        return Ok(new 
        { 
            success = true, 
            isCompleted = isCompleted 
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error checking content completion for contentId: {ContentId}", contentId);
        return StatusCode(500, new { success = false, message = "Internal server error" });
    }
}
```

## ?? Complete API Endpoints After Fix

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | `/api/course-progress/get-progress` | Get overall course progress |
| POST | `/api/course-progress/save-progress` | Save video watch progress |
| POST | `/api/course-progress/mark-complete` | Mark video lesson complete |
| POST | `/api/course-progress/mark-content-complete` | Mark any content type complete |
| GET | `/api/course-progress/check-content-completion` | **Check if content is completed** |
| GET | `/api/course-progress/debug-progress` | Debug progress data |
| POST | `/api/course-progress/cleanup-progress` | Remove invalid records |

## ?? Request/Response

### Request

```
GET /api/course-progress/check-content-completion?courseSlug=adadad&lessonId=1002&contentId=2002
Authorization: Bearer {token}
```

### Response - Success

```json
{
    "success": true,
    "isCompleted": true
}
```

### Response - Not Completed

```json
{
    "success": true,
    "isCompleted": false
}
```

### Response - Error

```json
{
    "success": false,
    "message": "Course not found"
}
```

## ?? Use Cases

### 1. Load Flashcard Set

When user opens flashcard player:

```javascript
async function loadFlashcardSet(contentId, flashcardSetId) {
    // ... load flashcards ...
    
    // Check if already completed
    await checkFlashcardCompletion(contentId, courseSlug, lessonId);
}
```

### 2. Show Complete Button State

If flashcard set is already completed:

```javascript
if (data.success && data.isCompleted) {
    // Show button in completed state
    const completeBtn = document.getElementById(`btnComplete-${contentId}`);
    completeBtn.disabled = true;
    completeBtn.innerHTML = '<i class="fas fa-check-circle me-2"></i>?ã hoàn thành';
    completeBtn.style.background = 'linear-gradient(135deg, #2e7d32 0%, #43a047 100%)';
}
```

### 3. Prevent Duplicate Completion

If content is already completed, don't allow completing again (or show different UI).

## ?? Common Errors & Solutions

### Error: 404 Not Found

**Cause**: Endpoint doesn't exist or wrong route

**Solution**: 
- Check `[Route("api/course-progress")]` at controller level
- Check `[HttpGet("check-content-completion")]` at method level
- Full URL should be: `/api/course-progress/check-content-completion`

### Error: Unexpected end of JSON input

**Cause**: Trying to parse HTML error page as JSON

**Solution**: 
- Check endpoint exists first
- Verify response status before parsing JSON:

```javascript
const response = await fetch(url);
if (!response.ok) {
    throw new Error(`HTTP error! status: ${response.status}`);
}
const data = await response.json();
```

### Error: 401 Unauthorized

**Cause**: User not logged in or token expired

**Solution**:
- Check `[Authorize]` attribute at controller level
- Ensure cookie authentication is working
- User must be logged in to access this endpoint

## ?? Testing

### Manual Testing

1. **Login** to the system
2. **Navigate** to a course with flashcard content
3. **Open** flashcard player
4. **Open DevTools** > Console
5. **Verify**: No 404 errors
6. **Complete** flashcard set
7. **Reload** page
8. **Verify**: Button shows "?ã hoàn thành" state

### API Testing (Postman/Swagger)

```bash
# Test endpoint exists
GET https://localhost:7158/api/course-progress/check-content-completion?courseSlug=test-course&lessonId=1&contentId=1

# Expected: 200 OK with JSON response
# Not: 404 Not Found with HTML
```

### Console Debug

```javascript
// In browser console
const courseSlug = 'adadad';
const lessonId = 1002;
const contentId = 2002;

fetch(`/api/course-progress/check-content-completion?courseSlug=${courseSlug}&lessonId=${lessonId}&contentId=${contentId}`)
    .then(r => r.json())
    .then(data => console.log('Result:', data))
    .catch(err => console.error('Error:', err));
```

## ?? Related Issues

- [Fix 404/405 API Errors](./Fix404_405_API_Errors.md)
- [Flashcard Progress Tracking](./FlashcardProgressTracking.md)
- [Course Progress API](./CourseProgressAPI.md)

## ? Checklist

- [x] Endpoint added to controller
- [x] Route attribute correct
- [x] Authorization working
- [x] Returns proper JSON response
- [x] Error handling implemented
- [x] Build successful
- [x] No 404 errors in console
- [x] Flashcard completion check works
- [x] Button state updates correctly

---

**Status**: ? Fixed
**Date**: 2024-01-08
**Version**: 1.0
