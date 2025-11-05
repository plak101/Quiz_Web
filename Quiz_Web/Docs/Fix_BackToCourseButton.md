# Fix: "Quay v? khóa h?c" Button 404 Error

## Issue
After completing flashcard exercises in a course, clicking the "Quay v? khóa h?c" (Back to course) button resulted in a 404 error with the URL:
```
https://localhost:7158/course/lap-trinh-csharp/learn?lessonId=7
```

## Root Cause
The URL format in `Finish.cshtml` was incorrect. The correct route for the course learning page is:
```
/course/learn/{courseSlug}?lessonId={lessonId}
```

But the code was generating:
```
/course/{courseSlug}/learn?lessonId={lessonId}
```

## Solution
**File**: `Quiz_Web\Views\Flashcard\Finish.cshtml`

**Line 39-44** (Changed):
```razor
@if (hasCourseLinkData)
{
    <a href="/course/learn/@courseSlug?lessonId=@lessonId" 
       class="btn btn-success btn-lg rounded-pill shadow d-flex align-items-center justify-content-center gap-2"
       id="returnToCourseBtn">
        <i class="bi bi-arrow-left-circle"></i>
        <span>Quay v? khóa h?c</span>
    </a>
}
```

**Previous (incorrect)**:
```razor
<a href="/course/@courseSlug/learn?lessonId=@lessonId"
```

## Route Verification
The correct route is defined in `CourseController.cs`:
```csharp
[HttpGet("learn/{slug}")]
public async Task<IActionResult> Learn(string slug, int? lessonId = null)
{
    // ... controller logic
}
```

This maps to URL pattern: `/course/learn/{slug}`

## Testing Steps
1. Navigate to a course that has flashcard content
2. Click on a flashcard lesson
3. Complete the flashcards (click through all cards)
4. Click "Hoàn thành" button
5. On the finish page, click "Quay v? khóa h?c" button
6. ? Should successfully navigate back to the course learning page

## Related Files
- `Quiz_Web\Views\Flashcard\Finish.cshtml` - Fixed button URL
- `Quiz_Web\wwwroot\js\flashcards\index.js` - `goToFinish()` function (working correctly)
- `Quiz_Web\Controllers\FlashcardController.cs` - `Study` and `Finish` actions (working correctly)
- `Quiz_Web\Controllers\CourseController.cs` - `Learn` action route definition

## Additional Notes
- The `goToFinish()` function in `index.js` correctly passes the `courseSlug`, `lessonId`, and `contentId` as query parameters
- The finish page correctly receives and uses these parameters
- The progress tracking API call is also working correctly
- The only issue was the URL format in the button href

## Build Status
? Build successful - No compilation errors
