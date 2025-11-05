# Course Progress API - Implementation Guide

## T?ng quan

API Controller ?? tracking ti?n ?? h?c c?a user trong khóa h?c:
- Save video progress (auto-save m?i 5s)
- Mark lesson complete
- Get overall course progress

## Endpoints

### 1. Get Progress
**Endpoint**: `GET /api/course-progress/get-progress?courseSlug={slug}`

**Authorization**: Required (Bearer token)

**Parameters**:
- `courseSlug` (query string): Slug c?a khóa h?c

**Response**:
```json
{
  "success": true,
  "completionPercentage": 45.5,
  "completedLessons": [1, 2, 5, 8],
  "totalLessons": 10
}
```

**Use Case**:
- Load khi vào trang Learn
- Update progress bar
- Highlight completed lessons

**Example**:
```javascript
fetch('/api/course-progress/get-progress?courseSlug=lap-trinh-csharp')
  .then(res => res.json())
  .then(data => {
    console.log('Completion:', data.completionPercentage + '%');
    console.log('Completed:', data.completedLessons);
  });
```

---

### 2. Save Progress
**Endpoint**: `POST /api/course-progress/save-progress`

**Authorization**: Required

**Request Body**:
```json
{
  "courseSlug": "lap-trinh-csharp",
  "lessonId": 5,
  "watchedDuration": 120,
  "totalDuration": 300
}
```

**Response**:
```json
{
  "success": true,
  "message": "Progress saved"
}
```

**Use Case**:
- Auto-save video progress m?i 5 giây
- Save khi user pause/leave page
- Track xem ??n ?âu ?? resume sau

**Example**:
```javascript
function saveVideoProgress() {
  fetch('/api/course-progress/save-progress', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      courseSlug: 'lap-trinh-csharp',
      lessonId: 5,
      watchedDuration: 120,
      totalDuration: 300
    })
  });
}
```

---

### 3. Mark Complete
**Endpoint**: `POST /api/course-progress/mark-complete`

**Authorization**: Required

**Request Body**:
```json
{
  "courseSlug": "lap-trinh-csharp",
  "lessonId": 5,
  "watchedDuration": 300
}
```

**Response**:
```json
{
  "success": true,
  "message": "Lesson marked as complete"
}
```

**Use Case**:
- User click "Mark as Complete"
- Auto-mark khi video ended
- Update completed lessons list

**Example**:
```javascript
function markLessonComplete() {
  fetch('/api/course-progress/mark-complete', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      courseSlug: 'lap-trinh-csharp',
      lessonId: 5,
      watchedDuration: 300
    })
  })
  .then(res => res.json())
  .then(data => {
    if (data.success) {
      toastr.success('?ã hoàn thành bài h?c!');
    }
  });
}
```

---

## Database Schema

### CourseProgress Table
```sql
CREATE TABLE dbo.CourseProgress (
    ProgressId    INT IDENTITY(1,1) PRIMARY KEY,
    UserId        INT NOT NULL,
    CourseId      INT NOT NULL,
    LessonId      INT NULL,
    ContentType   VARCHAR(20) NOT NULL,
    ContentId     INT NOT NULL,
    IsCompleted   BIT NOT NULL DEFAULT (0),
    CompletionAt  DATETIME2(7) NULL,
    LastViewedAt  DATETIME2(7) DEFAULT SYSUTCDATETIME(),
    Score         DECIMAL(6,2) NULL,
    DurationSec   INT NULL,
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    FOREIGN KEY (CourseId) REFERENCES dbo.Courses(CourseId)
);
```

**Fields**:
- `UserId`: ID ng??i dùng
- `CourseId`: ID khóa h?c
- `LessonId`: ID bài h?c (nullable)
- `ContentType`: Lo?i n?i dung (Video/Theory/Test/FlashcardSet)
- `ContentId`: ID c?a n?i dung c? th?
- `IsCompleted`: ?ã hoàn thành ch?a
- `CompletionAt`: Th?i ?i?m hoàn thành
- `LastViewedAt`: L?n xem cu?i
- `DurationSec`: Th?i gian ?ã xem (giây)

---

## Implementation Details

### Controller Structure
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CourseProgressController : ControllerBase
{
    private readonly LearningPlatformContext _context;
    private readonly ILogger<CourseProgressController> _logger;

    // Get progress
    [HttpGet("get-progress")]
    public async Task<IActionResult> GetProgress([FromQuery] string courseSlug)
    {
        // 1. Get userId from claims
        // 2. Find course by slug
        // 3. Count total lessons
        // 4. Count completed lessons
        // 5. Calculate percentage
        // 6. Return data
    }

    // Save progress
    [HttpPost("save-progress")]
    public async Task<IActionResult> SaveProgress([FromBody] SaveProgressRequest request)
    {
        // 1. Get userId from claims
        // 2. Find course by slug
        // 3. Find or create progress record
        // 4. Update LastViewedAt and DurationSec
        // 5. Save to database
    }

    // Mark complete
    [HttpPost("mark-complete")]
    public async Task<IActionResult> MarkComplete([FromBody] MarkCompleteRequest request)
    {
        // 1. Get userId from claims
        // 2. Find course by slug
        // 3. Find or create progress record
        // 4. Set IsCompleted = true, CompletionAt = now
        // 5. Save to database
    }
}
```

### Request Models
```csharp
public class SaveProgressRequest
{
    public string CourseSlug { get; set; } = string.Empty;
    public int LessonId { get; set; }
    public int WatchedDuration { get; set; }
    public int TotalDuration { get; set; }
}

public class MarkCompleteRequest
{
    public string CourseSlug { get; set; } = string.Empty;
    public int LessonId { get; set; }
    public int WatchedDuration { get; set; }
}
```

---

## Error Handling

### Common Errors

#### 401 Unauthorized
```json
{
  "success": false,
  "message": "Unauthorized"
}
```
**Cause**: User not logged in or token expired
**Solution**: Redirect to login page

#### 404 Not Found
```json
{
  "success": false,
  "message": "Course not found"
}
```
**Cause**: Invalid courseSlug or lessonId
**Solution**: Check URL parameters

#### 500 Internal Server Error
```json
{
  "success": false,
  "message": "Internal server error"
}
```
**Cause**: Database error, exception
**Solution**: Check server logs

---

## JavaScript Integration

### Auto-save every 5 seconds
```javascript
player.on('timeupdate', function() {
    watchedTime = player.currentTime();
    totalTime = player.duration();
    
    if (Math.floor(watchedTime) % 5 === 0) {
        saveVideoProgress();
    }
});
```

### Mark complete on video end
```javascript
player.on('ended', function() {
    markLessonComplete();
});
```

### Load progress on page load
```javascript
document.addEventListener('DOMContentLoaded', function() {
    loadCourseProgress();
});
```

### Save before leaving
```javascript
window.addEventListener('beforeunload', function() {
    if (player && watchedTime > 0) {
        saveVideoProgress();
    }
});
```

---

## Testing

### Manual Testing

1. **Test Get Progress**:
```bash
curl -X GET "https://localhost:7158/api/course-progress/get-progress?courseSlug=lap-trinh-csharp" \
  -H "Authorization: Bearer {token}"
```

2. **Test Save Progress**:
```bash
curl -X POST "https://localhost:7158/api/course-progress/save-progress" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "courseSlug": "lap-trinh-csharp",
    "lessonId": 5,
    "watchedDuration": 120,
    "totalDuration": 300
  }'
```

3. **Test Mark Complete**:
```bash
curl -X POST "https://localhost:7158/api/course-progress/mark-complete" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "courseSlug": "lap-trinh-csharp",
    "lessonId": 5,
    "watchedDuration": 300
  }'
```

### Browser DevTools Testing
```javascript
// Test in browser console
fetch('/api/course-progress/get-progress?courseSlug=lap-trinh-csharp')
  .then(r => r.json())
  .then(console.log);
```

### Database Verification
```sql
-- Check saved progress
SELECT * FROM CourseProgress 
WHERE UserId = 1 AND CourseId = 1
ORDER BY LastViewedAt DESC;

-- Check completion rate
SELECT 
    c.Title,
    COUNT(DISTINCT cp.LessonId) as CompletedLessons,
    (SELECT COUNT(*) FROM Lessons l 
     INNER JOIN CourseChapters ch ON l.ChapterId = ch.ChapterId 
     WHERE ch.CourseId = c.CourseId) as TotalLessons
FROM Courses c
LEFT JOIN CourseProgress cp ON c.CourseId = cp.CourseId AND cp.IsCompleted = 1
WHERE c.CourseId = 1
GROUP BY c.CourseId, c.Title;
```

---

## Performance Considerations

### Optimization Tips

1. **Debouncing**: Save progress max once per 5 seconds
2. **Batch Updates**: Consider batching if many users
3. **Caching**: Cache total lessons count
4. **Indexing**: Add indexes on commonly queried fields

### Recommended Indexes
```sql
CREATE INDEX IX_CourseProgress_UserCourse 
ON CourseProgress(UserId, CourseId, LessonId);

CREATE INDEX IX_CourseProgress_Completed 
ON CourseProgress(CourseId, UserId, IsCompleted);
```

---

## Future Enhancements

### Short-term
- [ ] Resume from last watched position
- [ ] Track watch time per day/week
- [ ] Streak tracking (days in a row)

### Long-term
- [ ] Real-time progress sync across devices
- [ ] Progress notifications
- [ ] Leaderboard based on completion
- [ ] Badges/Achievements for milestones

---

## Troubleshooting

### Progress not saving
1. Check user is authenticated
2. Verify API endpoint is correct
3. Check database connection
4. Look for errors in server logs

### Progress bar not updating
1. Check `loadCourseProgress()` is called
2. Verify API returns correct data
3. Check DOM element exists: `#courseProgressBar`
4. Inspect browser console for errors

### Completed lessons not showing
1. Verify `IsCompleted = 1` in database
2. Check `completedLessons` array in response
3. Ensure lesson IDs match
4. Check CSS class is applied: `.completed`

---

## Conclusion

API ?ã ???c implement v?i:
- ? Get progress v?i completion percentage
- ? Save progress (auto-save every 5s)
- ? Mark lesson complete
- ? Error handling ??y ??
- ? Authentication & Authorization
- ? Logging cho debugging

JavaScript trên trang Learn ?ã tích h?p s?n, ch? c?n ch?y là ho?t ??ng!
