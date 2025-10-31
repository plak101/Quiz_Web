# ?? Course Builder - Technical Documentation

## Architecture Overview

### Database Schema
```
CourseCategories (Danh m?c khóa h?c)
    ??? Courses (Khóa h?c)
            ??? CourseChapters (Ch??ng)
                    ??? Lessons (Bài h?c)
                            ??? LessonContents (N?i dung)
```

### Key Entities

#### Course
```csharp
public class Course
{
    public int CourseId { get; set; }
    public int OwnerId { get; set; }
    public int? CategoryId { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string? Summary { get; set; }
    public string? CoverUrl { get; set; }
    public decimal Price { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

#### CourseChapter
```csharp
public class CourseChapter
{
    public int ChapterId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
}
```

#### Lesson
```csharp
public class Lesson
{
    public int LessonId { get; set; }
    public int ChapterId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public string Visibility { get; set; } // Public/Private/Course
    public DateTime CreatedAt { get; set; }
}
```

#### LessonContent
```csharp
public class LessonContent
{
    public int ContentId { get; set; }
    public int LessonId { get; set; }
    public string ContentType { get; set; } // Video/Theory/FlashcardSet/Test
    public int? RefId { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public int OrderIndex { get; set; }
}
```

## Service Layer

### ICourseService Methods

```csharp
// Course Builder specific methods
List<CourseCategory> GetAllCategories();
Course? CreateCourseWithStructure(CourseBuilderViewModel model, int ownerId);
Course? UpdateCourseStructure(int courseId, CourseBuilderViewModel model, int ownerId);
Course? GetCourseWithFullStructure(int courseId, int ownerId);
CourseBuilderViewModel? GetCourseBuilderData(int courseId, int ownerId);
bool AutosaveCourse(int? courseId, CourseAutosaveViewModel model, int ownerId);
```

### Transaction Management
The `CreateCourseWithStructure` and `UpdateCourseStructure` methods use database transactions to ensure data consistency:

```csharp
using var transaction = _context.Database.BeginTransaction();
try
{
    // Create Course
    // Create Chapters
    // Create Lessons
    // Create LessonContents
    
    transaction.Commit();
}
catch (Exception ex)
{
    transaction.Rollback();
    // Log error
}
```

## Controller Actions

### GET /courses/builder
**Purpose:** Display course builder form (create or edit mode)

**Parameters:**
- `id` (optional): CourseId for editing

**Returns:**
- View with `CourseBuilderViewModel`
- `ViewBag.Categories`: List of CourseCategories

### POST /courses/builder/autosave
**Purpose:** Autosave course basic info

**Request Body:**
```json
{
  "courseId": 1,
  "title": "Course Title",
  "slug": "course-slug",
  "summary": "HTML content",
  "categoryId": 1,
  "coverUrl": "/uploads/...",
  "price": 199000,
  "isPublished": false
}
```

**Response:**
```json
{
  "success": true,
  "message": "?ã l?u t? ??ng"
}
```

### POST /courses/builder/save
**Purpose:** Create new course with full structure

**Form Data:**
- `jsonData`: JSON string of `CourseBuilderViewModel`
- `coverFile`: IFormFile (optional)

**Process:**
1. Deserialize JSON data
2. Upload cover image (if provided)
3. Sanitize HTML content (Summary, Descriptions, Bodies)
4. Validate slug uniqueness
5. Create course with structure
6. Redirect to course detail page

### POST /courses/builder/update/{id}
**Purpose:** Update existing course and structure

**Similar to save, but:**
- Checks ownership
- Deletes old structure
- Creates new structure
- Updates course info

## ViewModels

### CourseBuilderViewModel
```csharp
public class CourseBuilderViewModel
{
    // Step 1: Course Info
    public string Title { get; set; }
    public string Slug { get; set; }
    public string? Summary { get; set; }
    public int? CategoryId { get; set; }
    public string? CoverUrl { get; set; }
    public decimal? Price { get; set; }
    public bool IsPublished { get; set; }
    
    // Step 2-3: Structure
    public List<ChapterBuilderViewModel> Chapters { get; set; }
}
```

### ChapterBuilderViewModel
```csharp
public class ChapterBuilderViewModel
{
    public int? ChapterId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public List<LessonBuilderViewModel> Lessons { get; set; }
}
```

### LessonBuilderViewModel
```csharp
public class LessonBuilderViewModel
{
    public int? LessonId { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public string Visibility { get; set; }
    public List<LessonContentBuilderViewModel> Contents { get; set; }
}
```

### LessonContentBuilderViewModel
```csharp
public class LessonContentBuilderViewModel
{
    public int? ContentId { get; set; }
    public string ContentType { get; set; }
    public int? RefId { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public int OrderIndex { get; set; }
}
```

## Frontend Components

### JavaScript Architecture

#### Main Functions

**Step Management:**
- `nextStep()`: Validate and move to next step
- `prevStep()`: Move to previous step
- `updateStepDisplay()`: Update progress bar and active step
- `validateCurrentStep()`: Validate current step data

**Chapter Management:**
- `addChapter()`: Add new chapter
- `removeChapter(chapterId)`: Remove chapter
- `initializeChaptersSortable()`: Enable drag & drop

**Lesson Management:**
- `addLesson(chapterId)`: Add lesson to chapter
- `removeLesson(lessonId)`: Remove lesson
- `initializeLessonsSortable(container)`: Enable drag & drop

**Content Management:**
- `populateLessonSelector()`: Fill lesson dropdown
- `loadLessonContents()`: Load contents for selected lesson
- `addContent()`: Add new content item
- `removeContent(index)`: Remove content

**Data Management:**
- `saveStepData()`: Save current step to courseData object
- `saveCourse(publish)`: Submit form with JSON data
- `performAutosave()`: Auto-save course info

**CKEditor Management:**
- `initializeCKEditor()`: Initialize summary editor
- `initializeChapterDescriptionEditor(chapterId)`: Init chapter desc editor
- `initializeContentBodyEditor(contentId)`: Init content body editor

### CSS Architecture

**Design System:**
```css
:root {
    --primary-purple: #6d28d9;
    --primary-blue: #60a5fa;
    --gradient: linear-gradient(135deg, #6d28d9 0%, #60a5fa 100%);
    --radius: 16px;
    --radius-sm: 8px;
}
```

**Key Components:**
- Progress bar with steps
- Step cards with shadow
- Form grid (2 columns on desktop)
- Toggle switches
- File upload with preview
- Accordion chapters
- Draggable items
- Content type badges
- Modal dialogs

### Libraries Used

**CKEditor 5:**
```html
<script src="https://cdn.ckeditor.com/ckeditor5/41.4.2/classic/ckeditor.js"></script>
```

**SortableJS:**
```html
<script src="https://cdn.jsdelivr.net/npm/sortablejs@1.15.0/Sortable.min.js"></script>
```

**FontAwesome:**
```html
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css">
```

## Security

### HTML Sanitization
All HTML content is sanitized using `Ganss.Xss.HtmlSanitizer`:
- Summary
- Chapter Descriptions
- Lesson Content Bodies

```csharp
if (!string.IsNullOrEmpty(model.Summary))
    model.Summary = sanitizer.Sanitize(model.Summary);
```

### Authorization
All builder endpoints require `[Authorize]` attribute.

### File Upload Validation
```csharp
var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
var ext = Path.GetExtension(coverFile.FileName).ToLowerInvariant();
if (!allowed.Contains(ext))
{
    // Reject upload
}
```

### CSRF Protection
All POST requests include anti-forgery token:
```csharp
[ValidateAntiForgeryToken]
```

## Performance Considerations

### Database Optimization
- Use `Include()` for eager loading related entities
- Single transaction for creating full structure
- Proper indexing on slug field

### Frontend Optimization
- Lazy initialization of CKEditor instances
- Debounced autosave (10 seconds)
- Minimal DOM manipulations
- Event delegation where possible

### Caching
Consider implementing:
- Category list caching
- Course structure caching
- CDN for static assets

## Testing Scenarios

### Unit Tests
- [ ] Slug generation
- [ ] Slug uniqueness validation
- [ ] HTML sanitization
- [ ] Course creation with structure
- [ ] Course update with structure

### Integration Tests
- [ ] Full course creation flow
- [ ] Autosave functionality
- [ ] File upload and storage
- [ ] Edit existing course
- [ ] Delete course

### UI Tests
- [ ] Step navigation
- [ ] Chapter add/remove/reorder
- [ ] Lesson add/remove/reorder
- [ ] Content add/remove
- [ ] CKEditor integration
- [ ] Form validation
- [ ] Preview rendering

## Deployment Checklist

- [ ] Database migrations applied
- [ ] File upload directory writable (`wwwroot/uploads/courses`)
- [ ] CDN links accessible (CKEditor, SortableJS, FontAwesome)
- [ ] HTTPS enabled (for secure cookies)
- [ ] Connection string configured
- [ ] HtmlSanitizer configured in DI
- [ ] Authentication configured
- [ ] Static files middleware enabled

## Future Enhancements

### Phase 2
- [ ] Rich media upload (video, audio)
- [ ] Bulk import lessons
- [ ] Template library
- [ ] Course cloning
- [ ] Version history

### Phase 3
- [ ] Collaborative editing
- [ ] Real-time preview
- [ ] AI-powered content suggestions
- [ ] Analytics dashboard
- [ ] Course marketplace integration

## Troubleshooting Guide

### Issue: CKEditor not loading
**Solution:** Check CDN availability, browser console errors

### Issue: Drag & drop not working
**Solution:** Verify SortableJS loaded, check for JavaScript errors

### Issue: Autosave failing
**Solution:** Check network tab, verify authentication, check server logs

### Issue: File upload failing
**Solution:** Check file size limits, directory permissions, allowed extensions

### Issue: Slug conflict
**Solution:** Implement better slug generation with numeric suffix

## API Response Examples

### Success Response
```json
{
  "success": true,
  "message": "T?o khóa h?c thành công!",
  "courseId": 42,
  "slug": "lap-trinh-web-aspnet-core"
}
```

### Error Response
```json
{
  "success": false,
  "message": "Slug này ?ã t?n t?i"
}
```

## Database Query Examples

### Get Course with Full Structure
```csharp
var course = _context.Courses
    .Include(c => c.Category)
    .Include(c => c.CourseChapters.OrderBy(ch => ch.OrderIndex))
        .ThenInclude(ch => ch.Lessons.OrderBy(l => l.OrderIndex))
            .ThenInclude(l => l.LessonContents.OrderBy(lc => lc.OrderIndex))
    .FirstOrDefault(c => c.CourseId == courseId && c.OwnerId == ownerId);
```

### Create Course with Transaction
```csharp
using var transaction = _context.Database.BeginTransaction();
try
{
    _context.Courses.Add(course);
    _context.SaveChanges();
    
    foreach (var chapter in chapters)
    {
        _context.CourseChapters.Add(chapter);
        _context.SaveChanges();
        
        foreach (var lesson in chapter.Lessons)
        {
            _context.Lessons.Add(lesson);
            _context.SaveChanges();
            
            foreach (var content in lesson.Contents)
            {
                _context.LessonContents.Add(content);
            }
            _context.SaveChanges();
        }
    }
    
    transaction.Commit();
}
catch (Exception ex)
{
    transaction.Rollback();
    throw;
}
```

---

**Version:** 1.0
**Last Updated:** 2024
**Author:** Course Builder Team
