# Troubleshooting - Course Learning Page

## L?i: "Không tìm th?y bài h?c"

### Tri?u ch?ng
- Khi click nút "Xem tr??c bài h?c" ho?c "B?t ??u h?c", xu?t hi?n thông báo l?i màu ??
- B? redirect v? trang chi ti?t khóa h?c
- Console log: "Lesson not found - ChapterId: {id}, LessonId: {id}"

### Nguyên nhân
1. **Logic ki?m tra quy?n truy c?p quá nghiêm ng?t**
   - Không cho phép ch? khóa h?c xem tr??c
   - Ch? cho phép ng??i ?ã mua

2. **Không có chapters ho?c lessons**
   - Khóa h?c ch?a có c?u trúc n?i dung
   - Database tr?ng CourseChapters ho?c Lessons

3. **Thi?u LessonContents**
   - Lesson t?n t?i nh?ng không có contents

### Gi?i pháp

#### 1. S?a logic ki?m tra quy?n truy c?p

**File**: `Quiz_Web/Controllers/CourseController.cs`

```csharp
// GET: /courses/{slug}/learn
[Authorize]
[Route("/courses/{slug}/learn")]
[HttpGet]
public IActionResult Learn(string slug, int? chapterId = null, int? lessonId = null)
{
    // ...existing code...

    // Check if user has access to this course
    var isOwner = course.OwnerId == userId;
    var hasPurchased = course.CoursePurchases?.Any(p => p.BuyerId == userId && p.Status == "Paid") ?? false;

    // ? FIXED: Allow owner to preview
    if (!isOwner && !hasPurchased)
    {
        TempData["Error"] = "B?n c?n mua khóa h?c này ?? xem n?i dung.";
        return RedirectToAction("Detail", new { slug });
    }

    // ? FIXED: Check for empty course
    if (course.CourseChapters == null || !course.CourseChapters.Any())
    {
        TempData["Error"] = "Khóa h?c này ch?a có n?i dung.";
        if (isOwner)
        {
            TempData["Info"] = "Hãy thêm ch??ng và bài h?c vào khóa h?c c?a b?n.";
            return RedirectToAction("Builder", new { id = course.CourseId });
        }
        return RedirectToAction("Detail", new { slug });
    }

    // ...rest of code...
}
```

**Nh?ng gì ?ã thay ??i**:
- ? Ki?m tra `isOwner` TR??C khi ki?m tra `hasPurchased`
- ? Thêm ki?m tra n?u khóa h?c ch?a có chapters
- ? Redirect ch? khóa h?c ??n Builder ?? thêm n?i dung
- ? Hi?n th? thông báo rõ ràng

#### 2. X? lý tr??ng h?p ch?a có n?i dung

**File**: `Quiz_Web/Views/Course/Learn.cshtml`

```razor
<div class="video-player-wrapper">
    @if (currentLesson?.LessonContents != null && currentLesson.LessonContents.Any())
    {
        <!-- Existing video player code -->
    }
    else
    {
        <div class="no-video-placeholder">
            <i class="fas fa-video-slash fa-4x"></i>
            <p>Bài h?c này ch?a có n?i dung</p>
            @if (isOwner)
            {
                <a asp-controller="Course" asp-action="Builder" asp-route-id="@course.CourseId" 
                   class="btn btn-outline-light mt-3">
                    <i class="fas fa-plus me-2"></i>Thêm n?i dung
                </a>
            }
        </div>
    }
</div>
```

**Tính n?ng**:
- ? Hi?n th? placeholder khi ch?a có n?i dung
- ? Nút "Thêm n?i dung" cho ch? khóa h?c
- ? Icon và message rõ ràng

#### 3. Ki?m tra database

Ch?y query sau ?? ki?m tra c?u trúc khóa h?c:

```sql
-- Ki?m tra course có chapters không
SELECT 
    c.CourseId,
    c.Title,
    COUNT(DISTINCT ch.ChapterId) AS TotalChapters,
    COUNT(DISTINCT l.LessonId) AS TotalLessons,
    COUNT(lc.ContentId) AS TotalContents
FROM Courses c
LEFT JOIN CourseChapters ch ON c.CourseId = ch.CourseId
LEFT JOIN Lessons l ON ch.ChapterId = l.ChapterId
LEFT JOIN LessonContents lc ON l.LessonId = lc.LessonId
WHERE c.Slug = 'lap-trinh-csharp' -- Thay b?ng slug c?a b?n
GROUP BY c.CourseId, c.Title;
```

**K?t qu? mong ??i**:
- `TotalChapters > 0`
- `TotalLessons > 0`
- `TotalContents > 0`

**N?u = 0**: C?n thêm n?i dung qua Course Builder

#### 4. Debug v?i logging

Thêm logging ?? trace v?n ??:

```csharp
_logger.LogInformation($"Course Learn - Slug: {slug}, ChapterId: {chapterId}, LessonId: {lessonId}");
_logger.LogInformation($"Course found: {course?.Title}, Owner: {course?.OwnerId}, IsOwner: {isOwner}");
_logger.LogInformation($"Total chapters: {course?.CourseChapters?.Count ?? 0}");
_logger.LogInformation($"Total lessons: {course?.CourseChapters?.Sum(c => c.Lessons?.Count ?? 0) ?? 0}");
```

Xem logs trong Debug Console ho?c Output window.

## Các l?i khác

### 1. Video không phát

**Tri?u ch?ng**: Player hi?n th? nh?ng video không load

**Ki?m tra**:
```sql
SELECT ContentId, VideoUrl, ContentType 
FROM LessonContents 
WHERE LessonId = {id};
```

**Gi?i pháp**:
- ??m b?o `VideoUrl` không null và ?úng ??nh d?ng
- Ki?m tra file t?n t?i trong `wwwroot/uploads/videos/`
- Ki?m tra MIME type: `video/mp4`, `video/webm`, etc.

### 2. Sidebar không hi?n th?

**Ki?m tra**:
- CSS loaded: F12 ? Network ? `course-learn.css`
- JavaScript loaded: F12 ? Console ? Không có l?i
- Data có chapters: `ViewBag.Course.CourseChapters`

### 3. Navigation không ho?t ??ng

**Ki?m tra JavaScript**:
```javascript
// F12 ? Console
console.log('Current lesson:', window.location.search);
document.querySelector('.lesson-list-item.current-lesson');
```

**Gi?i pháp**: ??m b?o `course-learn.js` loaded

## Testing Checklist

### Tr??c khi test
- [ ] Course có ít nh?t 1 chapter
- [ ] Chapter có ít nh?t 1 lesson
- [ ] Lesson có ít nh?t 1 content (Video/Theory/Test/Flashcard)
- [ ] User là owner HO?C ?ã mua course

### Test cases
1. **Owner xem tr??c**:
   - [ ] URL: `/courses/{slug}/learn`
   - [ ] Không b? block b?i "c?n mua khóa h?c"
   - [ ] Hi?n th? video/n?i dung
   - [ ] Sidebar hi?n th? ?úng
   - [ ] Navigation prev/next ho?t ??ng

2. **User ?ã mua**:
   - [ ] Gi?ng nh? owner test
   - [ ] Progress ???c save
   - [ ] Mark complete ho?t ??ng

3. **User ch?a mua**:
   - [ ] B? redirect v? Detail page
   - [ ] Thông báo "c?n mua khóa h?c"

4. **Course ch?a có n?i dung**:
   - [ ] Owner: Redirect ??n Builder
   - [ ] User: Thông báo "ch?a có n?i dung"

5. **Lesson ch?a có content**:
   - [ ] Hi?n th? placeholder
   - [ ] Owner: Nút "Thêm n?i dung"

## Debug Tools

### Browser DevTools
```javascript
// Check course data
console.log('Course:', window.courseData);

// Check current lesson
console.log('Lesson:', {
    chapterId: new URLSearchParams(window.location.search).get('chapterId'),
    lessonId: new URLSearchParams(window.location.search).get('lessonId')
});

// Check video player
console.log('Video player:', videojs.getPlayer('videoPlayer'));
```

### SQL Queries
```sql
-- Get full course structure
SELECT 
    c.Title AS CourseName,
    ch.Title AS ChapterName,
    l.Title AS LessonName,
    lc.ContentType,
    lc.VideoUrl
FROM Courses c
INNER JOIN CourseChapters ch ON c.CourseId = ch.CourseId
INNER JOIN Lessons l ON ch.ChapterId = l.ChapterId
LEFT JOIN LessonContents lc ON l.LessonId = lc.LessonId
WHERE c.Slug = 'your-slug'
ORDER BY ch.OrderIndex, l.OrderIndex, lc.OrderIndex;
```

## K?t lu?n

L?i "Không tìm th?y bài h?c" ?ã ???c s?a b?ng cách:
1. ? Cho phép owner xem tr??c
2. ? Ki?m tra course empty và redirect h?p lý
3. ? Hi?n th? UI rõ ràng khi ch?a có n?i dung
4. ? Logging ??y ?? ?? debug

Sau khi áp d?ng các fix này, tính n?ng Learn Page s? ho?t ??ng ?n ??nh cho c? owner và users.
