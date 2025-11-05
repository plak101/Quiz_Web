# Course Learning Page - New Layout Documentation

## T?ng quan

Giao di?n xem bài h?c ?ã ???c thi?t k? l?i v?i layout 2 c?t gi?ng Udemy:
- **C?t trái (70%)**: Video player + Tabs n?i dung (Overview/Reviews/Notes)
- **C?t ph?i (30%)**: Danh sách n?i dung khóa h?c v?i accordion

## Layout Structure

```
???????????????????????????????????????????????????????????????
?                    Navbar (Fixed Top)                        ?
?  [? Thoát] Course Title                    [Mark Complete]  ?
???????????????????????????????????????????????????????????????
?                              ?                              ?
?   VIDEO PLAYER AREA          ?   COURSE SIDEBAR            ?
?   (16:9 Aspect Ratio)        ?   ????????????????????????  ?
?                              ?   ? Progress Bar          ?  ?
?                              ?   ????????????????????????  ?
?                              ?   ? Chapter 1             ?  ?
?                              ?   ?  ? Lesson 1.1        ?  ?
????????????????????????????????   ?  ? Lesson 1.2 ?     ?  ?
?  Lesson Title    [?Prev Next?]?   ?    (Playing)        ?  ?
????????????????????????????????   ? Chapter 2            ?  ?
?  [Overview] [Reviews] [Notes]?   ?  ? Lesson 2.1       ?  ?
?                              ?   ?  ? Lesson 2.2       ?  ?
?  Tab Content Area            ?   ????????????????????????  ?
?  - Course info               ?                              ?
?  - Reviews list              ?                              ?
?  - Notes (coming soon)       ?                              ?
?                              ?                              ?
???????????????????????????????????????????????????????????????
```

## Key Features

### 1. **Video Player Section**
- Full-width 16:9 aspect ratio
- Video.js player v?i controls ??y ??
- Placeholder khi ch?a có video
- Auto-save progress m?i 5 giây

**HTML Structure:**
```html
<div class="video-player-section">
    <video id="videoPlayer" class="video-js vjs-default-skin" 
           controls preload="auto" data-setup='{"fluid": true, "aspectRatio": "16:9"}'>
        <source src="@videoUrl" type="video/mp4">
    </video>
</div>
```

### 2. **Lesson Header**
- Hi?n th? tiêu ?? bài h?c
- Nút Previous/Next ?? navigate
- Fixed position khi scroll

**Features:**
- ? Navigate gi?a các lessons
- ? Disable nút khi ? ??u/cu?i
- ? Toast notification khi hoàn thành

### 3. **Content Tabs**
Có 3 tabs chính:

#### a) Overview Tab (Default Active)
- Gi?i thi?u khóa h?c
- Course stats (rating, reviews)
- Lesson contents:
  - Theory: Hi?n th? n?i dung HTML
  - Flashcard Set: Link ??n practice
  - Test: Link ??n take test

#### b) Reviews Tab
- Danh sách ?ánh giá h?c viên
- Hi?n th? rating stars
- User name, comment, date
- Gi?i h?n 10 reviews m?i nh?t

#### c) Notes Tab
- Tính n?ng ?ang phát tri?n
- Placeholder message

**Tab Navigation:**
```javascript
// Bootstrap tabs ???c kh?i t?o t? ??ng
// Có th? switch b?ng:
const tab = new bootstrap.Tab(document.getElementById('reviews-tab'));
tab.show();
```

### 4. **Course Sidebar (Sticky)**
- Fixed width 30%
- Scrollable content area
- Accordion cho chapters
- Progress bar ? header

**Components:**
- ? Progress bar (0-100%)
- ? Chapters accordion (expandable)
- ? Lessons list v?i icons
- ? Highlight current lesson
- ? Completed lesson checkmark

### 5. **Responsive Design**

#### Desktop (> 1024px)
- Layout 70/30 split
- Sidebar always visible
- Full tab navigation

#### Tablet (768px - 1024px)
- Sidebar hidden
- Tab navigation simplified
- Full-width video

#### Mobile (< 768px)
- Single column layout
- Stacked components
- Touch-optimized controls

## CSS Classes

### Layout Classes
```css
.course-player-container   /* Main container */
.main-content-area        /* Left column (70%) */
.course-sidebar           /* Right column (30%) */
```

### Component Classes
```css
.video-player-section     /* Video area */
.lesson-header            /* Title + navigation */
.lesson-tabs-section      /* Tabs navigation */
.tab-content-wrapper      /* Tab content padding */
.sidebar-header           /* Sidebar title + progress */
.sidebar-content          /* Scrollable lesson list */
```

### State Classes
```css
.current-lesson           /* Active lesson */
.completed                /* Completed lesson */
.current-chapter          /* Active chapter */
```

## JavaScript Functions

### Video Player
```javascript
initializeVideoPlayer()   // Initialize Video.js
saveVideoProgress()       // Auto-save every 5s
```

### Navigation
```javascript
navigateToPreviousLesson() // Go to prev
navigateToNextLesson()     // Go to next
```

### Progress Tracking
```javascript
markLessonComplete()      // Mark as done
loadCourseProgress()      // Load user progress
```

## Color Scheme

### Main Colors
- Primary: `#5624d0` (Purple)
- Dark Text: `#1c1d1f`
- Light Gray: `#f7f9fa`
- Border: `#d1d7dc`
- Success: `#28a745`
- Warning: `#ffc107`

### Text Colors
- Heading: `#1c1d1f`
- Body: `#2d2f31`
- Muted: `#6a6f73`

## API Endpoints (To Implement)

### Save Progress
```
POST /api/course-progress/save-progress
Body: {
    courseSlug: string,
    lessonId: number,
    watchedDuration: number,
    totalDuration: number
}
```

### Mark Complete
```
POST /api/course-progress/mark-complete
Body: {
    courseSlug: string,
    lessonId: number,
    watchedDuration: number
}
```

### Get Progress
```
GET /api/course-progress/get-progress?courseSlug={slug}
Response: {
    success: boolean,
    completionPercentage: number,
    completedLessons: number[]
}
```

## Usage Example

### 1. Navigate to Learn Page
```
/courses/lap-trinh-csharp/learn
/courses/lap-trinh-csharp/learn?chapterId=1&lessonId=1
```

### 2. Video Auto-play (Optional)
```javascript
player.on('loadedmetadata', function() {
    // Resume from last watched position
    if (lastPosition > 0) {
        player.currentTime(lastPosition);
    }
});
```

### 3. Custom Event Handling
```javascript
// Listen for lesson complete
document.addEventListener('lessonComplete', function(e) {
    console.log('Lesson completed:', e.detail.lessonId);
    // Auto navigate to next
    navigateToNextLesson();
});
```

## Testing Checklist

### Visual
- [ ] Video player hi?n th? ?úng t? l? 16:9
- [ ] Sidebar fixed bên ph?i
- [ ] Tabs navigation ho?t ??ng
- [ ] Progress bar hi?n th? ?úng
- [ ] Current lesson ???c highlight
- [ ] Completed lessons có checkmark

### Functional
- [ ] Video play/pause
- [ ] Previous/Next navigation
- [ ] Tab switching
- [ ] Accordion expand/collapse
- [ ] Progress auto-save
- [ ] Mark complete
- [ ] Responsive trên mobile

### Integration
- [ ] Load course data ?úng
- [ ] Load user progress
- [ ] Save progress API
- [ ] Mark complete API
- [ ] Navigate gi?a lessons

## Troubleshooting

### Video không phát
1. Ki?m tra `VideoUrl` trong database
2. Ki?m tra file path: `/uploads/videos/...`
3. Check MIME type: `video/mp4`
4. Video.js loaded: `<script src="...video.min.js">`

### Sidebar không hi?n th?
1. Check CSS loaded: `course-learn.css`
2. Data có chapters: `@course.CourseChapters`
3. Bootstrap JS loaded
4. Width > 1024px (desktop)

### Tabs không switch
1. Bootstrap JS loaded
2. Tab IDs match: `#overview-tab` ? `#overview`
3. `data-bs-toggle="tab"` present
4. No JavaScript errors

### Progress không save
1. API endpoints implemented
2. User authenticated
3. CourseProgress table exists
4. Database connection OK

## Future Enhancements

### Short-term
- [ ] Keyboard shortcuts (Space = play/pause, ? = next)
- [ ] Resume from last position
- [ ] Picture-in-picture mode
- [ ] Playback speed control

### Long-term
- [ ] Notes system with timestamps
- [ ] Q&A forum integration
- [ ] Transcript/Subtitles
- [ ] Offline download
- [ ] Multi-language support

## Conclusion

Giao di?n m?i v?i layout 2 c?t cung c?p:
- ? Tr?i nghi?m xem video t?t h?n
- ? Navigation d? dàng gi?a lessons
- ? Overview rõ ràng v? course
- ? Progress tracking tr?c quan
- ? Responsive design

Ng??i dùng có th? t?p trung vào video trong khi v?n d? dàng truy c?p danh sách bài h?c và theo dõi ti?n ??.
