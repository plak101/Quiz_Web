# Course Progress System - Content-Based Tracking

## ?? T?ng quan

H? th?ng tracking progress khóa h?c ?ã ???c **c?i ti?n** ?? tính ph?n tr?m hoàn thành d?a trên **t?ng s? n?i dung (LessonContent)** thay vì s? l??ng bài h?c (Lesson).

## ?? V?n ?? ban ??u

### Logic c?:
```
Progress = (S? bài h?c ?ã hoàn thành / T?ng s? bài h?c) × 100%
```

**V?n ??**: 
- M?i bài h?c có th? có nhi?u n?i dung (Video, Theory, Flashcard, Test)
- Khi t?o bài h?c m?i v?i nhi?u n?i dung, % hoàn thành không ph?n ánh ?úng ti?n ??
- Ng??i dùng hoàn thành 1 video nh?ng ch?a làm flashcard/test ? V?n coi là hoàn thành toàn b? bài h?c

## ? Logic m?i

### C?i ti?n:
```
Progress = (S? n?i dung ?ã hoàn thành / T?ng s? n?i dung) × 100%
```

**?u ?i?m**:
- ? Tracking chính xác t?ng n?i dung riêng l?
- ? Khi thêm content m?i, % t? ??ng gi?m xu?ng
- ? Khi hoàn thành content, % t?ng lên t??ng ?ng
- ? Ph?n ánh ?úng ti?n ?? h?c t?p th?c t?

## ?? Các thay ??i k? thu?t

### 1. **CourseProgressController.cs** - API Endpoint

#### `GET /api/course-progress/get-progress`

**Tr??c:**
```csharp
// Tính t?ng s? lessons
var totalLessons = course.CourseChapters
    .SelectMany(ch => ch.Lessons)
    .Count();

// Tính lessons ?ã hoàn thành
var completedLessons = await _context.CourseProgresses
    .Where(p => p.CourseId == course.CourseId && p.UserId == userId && p.IsCompleted)
    .Select(p => p.LessonId)
    .Distinct()
    .Count();

// % hoàn thành d?a trên lessons
var completionPercentage = (double)completedLessons / totalLessons * 100;
```

**Sau:**
```csharp
// ? Tính t?ng s? LessonContents trong khóa h?c
var totalContents = course.CourseChapters
    .SelectMany(ch => ch.Lessons)
    .SelectMany(l => l.LessonContents)
    .Count();

// ? Tính s? contents ?ã hoàn thành
var completedContents = await _context.CourseProgresses
    .Where(p => p.CourseId == course.CourseId && p.UserId == userId && p.IsCompleted)
    .Select(p => new { p.LessonId, p.ContentId, p.ContentType })
    .Distinct()
    .ToListAsync();

// ? % hoàn thành d?a trên contents
var completionPercentage = (double)completedContents.Count / totalContents * 100;
```

**Response:**
```json
{
  "success": true,
  "completionPercentage": 35.71,
  "completedContents": 5,
  "totalContents": 14,
  "completedLessons": [1, 2, 3]
}
```

#### `POST /api/course-progress/save-progress`

**C?i ti?n:**
- ? Tìm video content trong lesson
- ? L?u progress v?i `ContentId` chính xác
- ? Track t?ng video content riêng bi?t

#### `POST /api/course-progress/mark-complete`

**C?i ti?n:**
- ? Mark video content as completed
- ? L?u `ContentId` và `ContentType = "Video"`

#### `POST /api/course-progress/mark-content-complete`

**?ã có s?n** - Dùng ?? mark Theory/Flashcard/Test as complete:
```json
{
  "courseSlug": "lap-trinh-web",
  "lessonId": 1,
  "contentId": 5,
  "contentType": "Theory"
}
```

### 2. **course-learn.js** - Frontend Logic

#### Theory Content Auto-Tracking

**Thêm m?i:**
```javascript
function setupTheoryTracking() {
    const theoryItems = document.querySelectorAll('.theory-item');
    
    theoryItems.forEach(item => {
        // ? S? d?ng Intersection Observer
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    // ? ??i 3 giây tr??c khi mark as viewed
                    setTimeout(() => {
                        markTheoryContentViewed(contentId);
                    }, 3000);
                }
            });
        }, {
            threshold: 0.5 // 50% visible
        });
        
        observer.observe(contentPreview);
    });
}
```

**Cách ho?t ??ng:**
1. Khi ng??i dùng scroll ??n Theory content
2. Content xu?t hi?n 50% trong viewport
3. ??i 3 giây ?? ??m b?o ng??i dùng ?ang ??c
4. T? ??ng call API mark as complete

#### Progress Bar Display

**C?i ti?n:**
```javascript
function loadCourseProgress() {
    fetch(`/api/course-progress/get-progress?courseSlug=${courseSlug}`)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // ? Hi?n th? c? % và s? l??ng
                const progressText = progressBar.querySelector('span');
                progressText.textContent = `${Math.round(percentage)}% (${data.completedContents}/${data.totalContents})`;
                
                // ? Log ?? debug
                console.log('Course Progress:', {
                    percentage: data.completionPercentage,
                    completed: data.completedContents,
                    total: data.totalContents
                });
            }
        });
}
```

### 3. **Learn.cshtml** - View Updates

**Thêm `data-content-id`:**
```razor
@if (content.ContentType == "Theory" && !string.IsNullOrEmpty(content.Body))
{
    <div class="content-item theory-item" data-content-id="@content.ContentId">
        <!-- content -->
    </div>
}
```

**M?c ?ích:**
- JavaScript có th? l?y ContentId t? DOM
- Dùng ?? call API mark-content-complete

## ?? Database Schema

### CourseProgress Table

| Column | Type | Description |
|--------|------|-------------|
| ProgressId | int | Primary Key |
| UserId | int | User ID |
| CourseId | int | Course ID |
| LessonId | int? | Lesson ID (nullable) |
| ContentType | string | "Video", "Theory", "FlashcardSet", "Test" |
| ContentId | int | ID c?a LessonContent |
| IsCompleted | bool | ?ã hoàn thành? |
| CompletionAt | DateTime? | Th?i gian hoàn thành |
| LastViewedAt | DateTime? | L?n xem cu?i |
| Score | decimal? | ?i?m (cho Test) |
| DurationSec | int? | Th?i l??ng (cho Video) |

### Ví d? d? li?u:

```
UserId=1, CourseId=5, LessonId=1, ContentType="Video", ContentId=10, IsCompleted=true
UserId=1, CourseId=5, LessonId=1, ContentType="Theory", ContentId=11, IsCompleted=true
UserId=1, CourseId=5, LessonId=1, ContentType="FlashcardSet", ContentId=12, IsCompleted=true
UserId=1, CourseId=5, LessonId=2, ContentType="Test", ContentId=15, IsCompleted=true, Score=85
```

## ?? User Flow

### Khi ng??i dùng h?c bài:

1. **Xem Video:**
   - Video player track progress
   - Khi xem h?t ? Call `POST /api/course-progress/mark-complete`
   - Progress bar t?ng lên

2. **??c Theory:**
   - Intersection Observer detect content visible
   - ??i 3 giây
   - Auto call `POST /api/course-progress/mark-content-complete`
   - Progress bar t?ng lên

3. **Làm Flashcard:**
   - Click "Luy?n t?p Flashcard"
   - Hoàn thành h?t cards
   - Click "Hoàn thành" ? Call API
   - Progress bar t?ng lên

4. **Làm Test:**
   - Click "Làm bài ki?m tra"
   - Tr? l?i câu h?i
   - Click "N?p bài" ? Call API
   - Progress bar t?ng lên

### Khi gi?ng viên thêm content m?i:

1. Gi?ng viên vào Course Builder
2. Thêm 3 flashcard sets và 2 tests vào bài h?c
3. T?ng contents: **10 ? 15**
4. Completed contents: **7 (không ??i)**
5. Progress: **70% ? 46.67%** ? T? ??ng gi?m

## ?? Testing

### Test Case 1: Khóa h?c m?i

```
Initial:
- Total Contents: 0
- Completed: 0
- Progress: 0%

After adding 4 lessons with 2 contents each:
- Total Contents: 8
- Completed: 0
- Progress: 0%

After completing 4 contents:
- Total Contents: 8
- Completed: 4
- Progress: 50%
```

### Test Case 2: Thêm content vào khóa h?c ?ang h?c

```
Before:
- Total Contents: 10
- Completed: 8
- Progress: 80%

After adding 5 new contents:
- Total Contents: 15
- Completed: 8
- Progress: 53.33% ?

After completing 3 new contents:
- Total Contents: 15
- Completed: 11
- Progress: 73.33%
```

### Test Case 3: Multiple content types

```
Lesson 1:
- Video (ContentId=1) ? Completed
- Theory (ContentId=2) ? Completed
- Flashcard (ContentId=3) ? Not completed
- Test (ContentId=4) ? Not completed

Progress: 2/4 = 50%
```

## ?? Debugging

### Console Logs

```javascript
// Trong course-learn.js
console.log('Course Progress:', {
    percentage: data.completionPercentage,
    completed: data.completedContents,
    total: data.totalContents
});

// Khi mark theory content
console.log('Theory content marked as viewed:', contentId);
```

### API Testing v?i Postman

```http
GET /api/course-progress/get-progress?courseSlug=lap-trinh-web
Authorization: Bearer {token}

Response:
{
  "success": true,
  "completionPercentage": 42.86,
  "completedContents": 3,
  "totalContents": 7,
  "completedLessons": [1, 2]
}
```

## ?? Notes

1. **Performance:**
   - Theory auto-tracking ch? ch?y 1 l?n m?i content
   - Intersection Observer t? ??ng unobserve sau khi mark

2. **Edge Cases:**
   - N?u lesson không có content ? Progress = 0%
   - N?u course không có lesson ? Progress = 0%
   - N?u user ?ã hoàn thành content ? API không duplicate record

3. **Future Improvements:**
   - [ ] Track reading time cho Theory content
   - [ ] Track video watch percentage (not just completion)
   - [ ] Add progress animation khi % thay ??i
   - [ ] Show progress per chapter

## ?? Deployment Checklist

- [x] Update CourseProgressController.cs
- [x] Update course-learn.js
- [x] Update Learn.cshtml
- [x] Test API endpoints
- [x] Test frontend tracking
- [x] Verify progress calculation
- [x] Check responsive design
- [x] Write documentation

---

**L?u ý:** H? th?ng này hoàn toàn backward compatible. D? li?u progress c? v?n ho?t ??ng bình th??ng, ch? có cách tính % m?i thay ??i.
