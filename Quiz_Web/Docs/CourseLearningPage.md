# Tính n?ng Xem Bài H?c - Course Learning Page

## T?ng quan

Tính n?ng xem bài h?c ???c thi?t k? gi?ng giao di?n c?a Udemy, cho phép ng??i dùng xem video, ??c lý thuy?t, làm bài test và flashcard trong m?t giao di?n th?ng nh?t.

## Các file ?ã t?o

### 1. Controller & Action
- **File**: `Quiz_Web/Controllers/CourseController.cs`
- **Action**: `Learn(string slug, int? chapterId, int? lessonId)`
- **Ch?c n?ng**: 
  - Ki?m tra quy?n truy c?p (?ã mua ho?c là ch? khóa h?c)
  - Load thông tin khóa h?c, ch??ng, bài h?c
  - T? ??ng chuy?n ??n bài h?c ??u tiên n?u không ch? ??nh

### 2. View
- **File**: `Quiz_Web/Views/Course/Learn.cshtml`
- **Layout**: S? d?ng `_LayoutLearn.cshtml` (layout riêng không có header/footer)
- **C?u trúc**:
  - Video player area (bên trái)
  - Sidebar v?i danh sách bài h?c (bên ph?i)
  - Tabs n?i dung bên d??i (T?ng quan, Ghi chú, H?i ?áp)

### 3. Layout riêng
- **File**: `Quiz_Web/Views/Shared/_LayoutLearn.cshtml`
- **??c ?i?m**:
  - Navbar ??n gi?n (ch? nút thoát và tiêu ??)
  - Không có header/footer
  - Toàn màn hình cho video

### 4. CSS
- **File**: `Quiz_Web/wwwroot/css/course/course-learn.css`
- **Tính n?ng**:
  - Responsive design
  - Dark theme gi?ng Udemy
  - Sidebar collapse/expand
  - Video player styles

### 5. JavaScript
- **File**: `Quiz_Web/wwwroot/js/course/course-learn.js`
- **Ch?c n?ng**:
  - Initialize Video.js player
  - Track video progress
  - Navigation gi?a các bài h?c
  - Mark lesson as complete
  - Save/load progress

## C?u trúc Database

### B?ng liên quan:
1. **Courses** - Thông tin khóa h?c
2. **CourseChapters** - Các ch??ng trong khóa h?c
3. **Lessons** - Các bài h?c trong ch??ng
4. **LessonContents** - N?i dung c?a bài h?c (Video, Theory, Test, Flashcard)
5. **CourseProgress** - Ti?n ?? h?c c?a ng??i dùng

### Schema LessonContents:
```sql
CREATE TABLE dbo.LessonContents (
    ContentId   INT IDENTITY(1,1) PRIMARY KEY,
    LessonId    INT NOT NULL,
    ContentType VARCHAR(20) NOT NULL, -- 'Video', 'Theory', 'FlashcardSet', 'Test'
    RefId       INT NULL, -- ID tham chi?u ??n FlashcardSet ho?c Test
    Title       NVARCHAR(200) NULL,
    Body        NVARCHAR(MAX) NULL, -- N?i dung lý thuy?t
    VideoUrl    NVARCHAR(500) NULL, -- URL video
    OrderIndex  INT NOT NULL DEFAULT (0),
    CreatedAt   DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
);
```

## Các tính n?ng chính

### 1. Video Player
- S? d?ng **Video.js** cho player chuyên nghi?p
- H? tr? các ??nh d?ng: MP4, WebM, OGG
- Auto-save progress m?i 5 giây
- Mark complete khi xem h?t video

### 2. Sidebar Danh sách Bài h?c
- Hi?n th? c?u trúc: Ch??ng ? Bài h?c
- Badge hi?n th? lo?i n?i dung (Video, Test, Flashcard)
- Highlight bài h?c hi?n t?i
- Icon check cho bài h?c ?ã hoàn thành
- Collapse/Expand các ch??ng

### 3. Navigation
- Nút Previous/Next ?? chuy?n bài h?c
- Auto navigation sang ch??ng ti?p theo khi h?t bài
- Keyboard shortcuts (TODO)

### 4. Progress Tracking
- Progress bar hi?n th? % hoàn thành
- Save ti?n ?? xem video
- Mark lesson complete
- API endpoints (TODO - c?n implement):
  - `POST /api/course-progress/save-progress`
  - `POST /api/course-progress/mark-complete`
  - `GET /api/course-progress/get-progress`

### 5. Tabs N?i dung
- **T?ng quan**: Hi?n th? mô t?, lý thuy?t, links ??n test/flashcard
- **Ghi chú**: TODO - cho phép ng??i dùng ghi chú
- **H?i ?áp**: TODO - Q&A forum

## Cách s? d?ng

### 1. Truy c?p trang h?c
**URL Pattern**: `/courses/{slug}/learn?chapterId={id}&lessonId={id}`

**Ví d?**: 
```
/courses/sql-co-ban/learn
/courses/sql-co-ban/learn?chapterId=1&lessonId=1
```

### 2. ?i?u ki?n truy c?p
- Ng??i dùng ph?i ??ng nh?p
- Ph?i mua khóa h?c HO?C là ch? khóa h?c
- N?u không th?a ?i?u ki?n ? redirect v? trang chi ti?t

### 3. T? trang chi ti?t khóa h?c
- N?u ?ã mua: Hi?n th? nút "B?t ??u h?c"
- N?u là ch?: Hi?n th? nút "Xem tr??c bài h?c"

## Responsive Design

### Desktop (> 1024px)
- Video chi?m 70% màn hình
- Sidebar c? ??nh bên ph?i (400px)
- Full controls visible

### Tablet (768px - 1024px)
- Sidebar overlay (có th? toggle)
- Video full width khi sidebar ?n

### Mobile (< 768px)
- Sidebar 320px overlay
- Controls simplified
- Video controls minimal

## API Endpoints c?n implement

### 1. Save Progress
```csharp
[HttpPost]
[Route("/api/course-progress/save-progress")]
public IActionResult SaveProgress([FromBody] SaveProgressRequest request)
{
    // Save watched duration
    // Update LastViewedAt
    return Json(new { success = true });
}
```

### 2. Mark Complete
```csharp
[HttpPost]
[Route("/api/course-progress/mark-complete")]
public IActionResult MarkComplete([FromBody] MarkCompleteRequest request)
{
    // Create/Update CourseProgress
    // Set IsCompleted = true, CompletionAt = DateTime.UtcNow
    return Json(new { success = true });
}
```

### 3. Get Progress
```csharp
[HttpGet]
[Route("/api/course-progress/get-progress")]
public IActionResult GetProgress(string courseSlug)
{
    // Calculate completion percentage
    // Return list of completed lessons
    return Json(new { 
        success = true, 
        completionPercentage = 50,
        completedLessons = [1, 2, 3]
    });
}
```

## Testing Checklist

- [ ] Video player hi?n th? ?úng
- [ ] Navigation gi?a các bài h?c
- [ ] Sidebar collapse/expand
- [ ] Responsive trên mobile
- [ ] Progress tracking
- [ ] Mark complete
- [ ] Access control
- [ ] Video upload và playback
- [ ] Theory content hi?n th? ?úng
- [ ] Links ??n Test/Flashcard ho?t ??ng

## C?i ti?n trong t??ng lai

1. **Video Controls**:
   - Playback speed
   - Quality selector
   - Subtitles/CC
   - Picture-in-picture

2. **Notes System**:
   - Add notes at specific timestamps
   - Edit/Delete notes
   - Search notes

3. **Q&A Forum**:
   - Ask questions at specific timestamps
   - Instructor/Student answers
   - Upvote/Downvote

4. **Progress Features**:
   - Resume from last watched
   - Download certificate
   - Course completion badge
   - Learning streak

5. **Social Features**:
   - Share progress on social media
   - Study groups
   - Leaderboard

## Troubleshooting

### Video không phát
- Ki?m tra `VideoUrl` trong database
- Ki?m tra file t?n t?i trong `wwwroot/uploads/videos/`
- Ki?m tra MIME type và format

### Sidebar không hi?n th?
- Ki?m tra data có CourseChapters và Lessons
- Ki?m tra CSS load ?úng
- Ki?m tra JavaScript không có l?i

### Progress không save
- Implement các API endpoints
- Ki?m tra authentication
- Ki?m tra database constraints

## K?t lu?n

Tính n?ng Course Learning Page ?ã ???c implement v?i ??y ?? UI/UX gi?ng Udemy. Các API endpoints cho progress tracking c?n ???c implement ?? tính n?ng hoàn ch?nh.
