# Flashcard Progress Tracking in Courses

## T?ng quan

Khi h?c viên hoàn thành flashcard t? khóa h?c, h? th?ng s?:
1. Track completion c?a flashcard content
2. Hi?n th? button "Quay v? khóa h?c"
3. T? ??ng reload progress bar khi quay l?i

## Flow

```
Learn Page ? Click "Luy?n t?p Flashcard" (target=_blank)
    ?
Study Page (v?i courseSlug, lessonId, contentId params)
    ?
Finish Page
    ?
    ?? Call API: mark-content-complete
    ?? Set localStorage flag
    ?? Button "Quay v? khóa h?c"
        ?
Learn Page (window focus event)
    ?
    ?? Detect localStorage flag
    ?? Reload progress
    ?? Update UI
```

## Implementation Details

### 1. **Pass Course Context to Flashcard**

#### Learn.cshtml
```csharp
var flashcardUrl = $"/flashcards/study/{content.RefId}?courseSlug={course.Slug}&lessonId={currentLesson.LessonId}&contentId={content.ContentId}";
```

**Query Parameters:**
- `courseSlug`: Slug c?a khóa h?c
- `lessonId`: ID c?a lesson hi?n t?i
- `contentId`: ID c?a flashcard content

### 2. **FlashcardController Updates**

#### Study Action
```csharp
[HttpGet("study/{setId:int}")]
public async Task<IActionResult> Study(int setId, string? courseSlug, int? lessonId, int? contentId)
{
    // ...
    ViewBag.CourseSlug = courseSlug;
    ViewBag.LessonId = lessonId;
    ViewBag.ContentId = contentId;
    return View(flashcardSet.Flashcards.ToList());
}
```

#### Finish Action
```csharp
[HttpGet("finish/{setId:int}")]
public async Task<IActionResult> Finish(int setId, string? courseSlug, int? lessonId, int? contentId)
{
    // ...
    ViewBag.CourseSlug = courseSlug;
    ViewBag.LessonId = lessonId;
    ViewBag.ContentId = contentId;
    return View();
}
```

### 3. **Study View Updates**

#### Pass Data to JavaScript
```javascript
const courseLinkData = {
    courseSlug: '@ViewBag.CourseSlug',
    lessonId: '@ViewBag.LessonId',
    contentId: '@ViewBag.ContentId'
};

const hasCourseLinkData = courseLinkData.courseSlug && 
                           courseLinkData.lessonId && 
                           courseLinkData.contentId;

initializeFlashcards(
    flashcardsData, 
    totalCards, 
    setId,
    hasCourseLinkData ? courseLinkData : null
);
```

### 4. **index.js Updates**

#### Store Course Link Data
```javascript
let courseLinkData = null;

function initializeFlashcards(data, total, setId, linkData) {
    flashcards = data;
    totalCards = total;
    currentSetId = setId || 0;
    courseLinkData = linkData || null; // ? Store for later
    // ...
}
```

#### Redirect with Params
```javascript
function goToFinish() {
    let url = `/flashcards/finish/${currentSetId}`;
    
    if (courseLinkData && courseLinkData.courseSlug && 
        courseLinkData.lessonId && courseLinkData.contentId) {
        const params = new URLSearchParams({
            courseSlug: courseLinkData.courseSlug,
            lessonId: courseLinkData.lessonId,
            contentId: courseLinkData.contentId
        });
        url += `?${params.toString()}`;
    }
    
    window.location.href = url;
}
```

### 5. **Finish View Updates**

#### Conditional Button Display
```razor
@if (hasCourseLinkData)
{
    <a href="/course/@courseSlug/learn?lessonId=@lessonId" 
       class="btn btn-success btn-lg"
       id="returnToCourseBtn">
        <i class="bi bi-arrow-left-circle"></i>
        Quay v? khóa h?c
    </a>
}
else
{
    <a href="/flashcards" class="btn btn-success btn-lg">
        <i class="bi bi-book"></i>
        Xem thêm các flashcard khác
    </a>
}
```

#### Mark Completion via API
```javascript
async function markFlashcardComplete() {
    const response = await fetch('/api/course-progress/mark-content-complete', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            courseSlug: courseSlug,
            lessonId: parseInt(lessonId),
            contentId: parseInt(contentId),
            contentType: 'FlashcardSet'
        })
    });
    
    const data = await response.json();
    
    if (data.success) {
        // ? Set flag ?? course page reload
        localStorage.setItem('courseProgressUpdated', Date.now().toString());
    }
}
```

### 6. **API Endpoint**

#### CourseProgressController.cs
```csharp
[HttpPost("mark-content-complete")]
public async Task<IActionResult> MarkContentComplete([FromBody] MarkContentCompleteRequest request)
{
    // Find or create progress record
    var progress = await _context.CourseProgresses
        .FirstOrDefaultAsync(p => 
            p.CourseId == course.CourseId && 
            p.UserId == userId && 
            p.LessonId == request.LessonId &&
            p.ContentType == request.ContentType &&
            p.ContentId == request.ContentId);

    if (progress == null)
    {
        progress = new CourseProgress
        {
            UserId = userId,
            CourseId = course.CourseId,
            LessonId = request.LessonId,
            ContentType = request.ContentType,
            ContentId = request.ContentId,
            IsCompleted = true,
            CompletionAt = DateTime.UtcNow,
            LastViewedAt = DateTime.UtcNow
        };
        _context.CourseProgresses.Add(progress);
    }
    else
    {
        progress.IsCompleted = true;
        progress.CompletionAt = DateTime.UtcNow;
        progress.LastViewedAt = DateTime.UtcNow;
    }

    await _context.SaveChangesAsync();
    
    return Ok(new { success = true, message = $"{request.ContentType} content marked as complete" });
}
```

#### Request Model
```csharp
public class MarkContentCompleteRequest
{
    public string CourseSlug { get; set; } = string.Empty;
    public int LessonId { get; set; }
    public int ContentId { get; set; }
    public string ContentType { get; set; } = string.Empty; // FlashcardSet, Test
}
```

### 7. **Progress Reload on Learn Page**

#### course-learn.js
```javascript
document.addEventListener('DOMContentLoaded', function() {
    initializeVideoPlayer();
    setupLessonNavigation();
    setupMarkComplete();
    loadCourseProgress();
    
    // ? Listen for storage events
    window.addEventListener('storage', function(e) {
        if (e.key === 'courseProgressUpdated') {
            loadCourseProgress();
            localStorage.removeItem('courseProgressUpdated');
        }
    });
    
    // ? Reload when window regains focus
    window.addEventListener('focus', function() {
        loadCourseProgress();
    });
});
```

## Database Schema

### CourseProgress Table
```sql
CREATE TABLE CourseProgress (
    ProgressId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    CourseId INT NOT NULL,
    LessonId INT NULL,
    ContentType VARCHAR(20) NOT NULL, -- 'Video', 'Theory', 'FlashcardSet', 'Test'
    ContentId INT NOT NULL,
    IsCompleted BIT NOT NULL DEFAULT 0,
    CompletionAt DATETIME2 NULL,
    LastViewedAt DATETIME2 DEFAULT SYSUTCDATETIME(),
    DurationSec INT NULL
);
```

**Key Points:**
- `ContentType`: Phân bi?t lo?i content (Video/FlashcardSet/Test)
- `ContentId`: ID c?a content c? th?
- `IsCompleted`: Track completion status
- `CompletionAt`: Timestamp khi complete

## User Experience Flow

### Scenario 1: H?c Flashcard t? Course

1. **User clicks "Luy?n t?p Flashcard"**
   - Opens in new tab
   - URL: `/flashcards/study/123?courseSlug=lap-trinh-web&lessonId=5&contentId=42`

2. **User completes all flashcards**
   - Redirects to Finish page with same params
   - Finish page calls API to mark complete
   - Sets localStorage flag

3. **User clicks "Quay v? khóa h?c"**
   - Returns to Learn page (original tab)
   - Window focus event triggers
   - Progress bar reloads
   - Shows updated completion %

### Scenario 2: H?c Flashcard Standalone

1. **User clicks flashcard from Index**
   - URL: `/flashcards/study/123` (no params)

2. **User completes**
   - Finish page shows normal buttons
   - No API call for course progress
   - Shows "Xem thêm các flashcard khác"

## Benefits

### For Learners
- ? Seamless experience h?c flashcard trong course
- ? Progress ???c track t? ??ng
- ? Easy navigation quay l?i course
- ? Real-time progress updates

### For System
- ? Accurate progress tracking
- ? Completion data cho analytics
- ? Proper separation: flashcard standalone vs course-embedded
- ? No page reload required

## Testing Checklist

### Flashcard in Course
- [ ] Click "Luy?n t?p Flashcard" opens in new tab
- [ ] URL contains courseSlug, lessonId, contentId
- [ ] Complete all flashcards
- [ ] Finish page shows "Quay v? khóa h?c" button
- [ ] API call marks content complete
- [ ] Return to course updates progress bar
- [ ] Completion % increases correctly

### Flashcard Standalone
- [ ] Click flashcard from /flashcards
- [ ] URL has no course params
- [ ] Complete flashcards
- [ ] Finish page shows normal buttons
- [ ] No "Quay v? khóa h?c" button
- [ ] No course progress API call

### Edge Cases
- [ ] User closes tab before completing
- [ ] User navigates away mid-flashcard
- [ ] Multiple flashcards in same lesson
- [ ] Browser back/forward navigation
- [ ] Storage event in same tab
- [ ] Focus event timing

## API Endpoints

### POST /api/course-progress/mark-content-complete

**Request:**
```json
{
  "courseSlug": "lap-trinh-web",
  "lessonId": 5,
  "contentId": 42,
  "contentType": "FlashcardSet"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "FlashcardSet content marked as complete"
}
```

**Response (Error):**
```json
{
  "success": false,
  "message": "Course not found"
}
```

## Future Enhancements

### Short-term
- [ ] Show completion checkmark on flashcard content
- [ ] Track time spent on flashcard
- [ ] Show flashcard stats in progress
- [ ] Badge/achievement for completing all flashcards

### Long-term
- [ ] Spaced repetition algorithm
- [ ] Flashcard review reminders
- [ ] Progress analytics dashboard
- [ ] Social features (share progress)

## Troubleshooting

### Progress không update
1. Check localStorage flag ???c set
2. Check window focus event fires
3. Check API response success
4. Check CourseProgress table có record
5. Verify ContentId matches

### Button không hi?n th?
1. Check query params trong URL
2. Check hasCourseLinkData condition
3. Verify ViewBag values
4. Inspect browser console for errors

### API call fails
1. Check user authentication
2. Verify courseSlug exists
3. Check lessonId và contentId valid
4. Look at server logs
5. Check database connection

## Conclusion

System gi? ?ã hoàn thi?n v?i:
- ? Flashcard progress tracking trong courses
- ? Button "Quay v? khóa h?c" khi h?c t? course
- ? Auto-reload progress bar khi return
- ? Proper completion marking
- ? Seamless UX between tabs
- ? Accurate analytics data

H?c viên có th? h?c flashcard và quay l?i course v?i progress ???c update t? ??ng! ??
