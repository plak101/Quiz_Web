# Lesson Contents Section - Feature Documentation

## T?ng quan

?ã thêm section "N?i dung bài h?c" hi?n th? các lo?i n?i dung c?a lesson (Theory, Flashcard, Test) v?i design ??p m?t và t??ng tác t?t.

## Layout

```
???????????????????????????????????????????????????
?           VIDEO PLAYER AREA                      ?
?           (16:9 aspect ratio)                    ?
???????????????????????????????????????????????????
???????????????????????????????????????????????????
?  Lesson Title              [? Prev] [Next ?]   ?
???????????????????????????????????????????????????
???????????????????????????????????????????????????
?  ?? N?i dung bài h?c                            ?
?                                                  ?
?  ???????????????????????????????????????????   ?
?  ? ?? Lý thuy?t                            ?   ?
?  ? N?i dung 1                    [Theory]  ?   ?
?  ?                                          ?   ?
?  ? [HTML Content Preview]                  ?   ?
?  ???????????????????????????????????????????   ?
?                                                  ?
?  ???????????????????????????????????????????   ?
?  ? ?? Flashcard                            ?   ?
?  ? N?i dung 2                 [Flashcard]  ?   ?
?  ?                                          ?   ?
?  ? [? Luy?n t?p Flashcard]                ?   ?
?  ???????????????????????????????????????????   ?
?                                                  ?
?  ???????????????????????????????????????????   ?
?  ? ? Test                                  ?   ?
?  ? Ki?m tra cu?i ch??ng            [Test]  ?   ?
?  ?                                          ?   ?
?  ? [? Làm bài ki?m tra]                   ?   ?
?  ???????????????????????????????????????????   ?
?                                                  ?
???????????????????????????????????????????????????
???????????????????????????????????????????????????
?  [T?ng quan] [?ánh giá] [Ghi chú]              ?
?                                                  ?
?  Tab content here...                            ?
???????????????????????????????????????????????????
```

## Components

### 1. **Lesson Contents Section**

Container cho t?t c? n?i dung bài h?c.

**HTML Structure:**
```html
<div class="lesson-contents-section">
    <h4 class="contents-section-title">
        <i class="fas fa-book-open me-2"></i>N?i dung bài h?c
    </h4>
    <div class="contents-list">
        <!-- Content items here -->
    </div>
</div>
```

**CSS Classes:**
- `.lesson-contents-section`: Container chính
- `.contents-section-title`: Tiêu ?? section
- `.contents-list`: Danh sách các content items

### 2. **Content Item Types**

#### a) Theory Content

Hi?n th? n?i dung lý thuy?t v?i HTML preview.

**Features:**
- ? Icon màu xanh d??ng (#1976d2)
- ? Badge "Lý thuy?t"
- ? HTML content preview (scrollable)
- ? Border trái màu xanh d??ng

**Example:**
```html
<div class="content-item theory-item">
    <div class="content-header">
        <div class="content-icon theory-icon">
            <i class="fas fa-book"></i>
        </div>
        <div class="content-info">
            <h5 class="content-title">Lý thuy?t v? OOP</h5>
            <span class="content-type-badge theory">Lý thuy?t</span>
        </div>
    </div>
    <div class="content-preview ck-content">
        <!-- HTML content here -->
    </div>
</div>
```

#### b) Flashcard Content

Hi?n th? link ?? practice flashcard set.

**Features:**
- ? Icon màu cam (#f57c00)
- ? Badge "Flashcard"
- ? Button "Luy?n t?p Flashcard" màu primary
- ? Border trái màu cam

**Example:**
```html
<div class="content-item flashcard-item">
    <div class="content-header">
        <div class="content-icon flashcard-icon">
            <i class="fas fa-layer-group"></i>
        </div>
        <div class="content-info">
            <h5 class="content-title">T? v?ng ch??ng 1</h5>
            <span class="content-type-badge flashcard">Flashcard</span>
        </div>
    </div>
    <div class="content-action">
        <a href="/flashcard/study/123" class="btn btn-primary content-action-btn">
            <i class="fas fa-play me-2"></i>Luy?n t?p Flashcard
        </a>
    </div>
</div>
```

#### c) Test Content

Hi?n th? link ?? làm bài ki?m tra.

**Features:**
- ? Icon màu xanh lá (#388e3c)
- ? Badge "Ki?m tra"
- ? Button "Làm bài ki?m tra" màu success
- ? Border trái màu xanh lá

**Example:**
```html
<div class="content-item test-item">
    <div class="content-header">
        <div class="content-icon test-icon">
            <i class="fas fa-clipboard-check"></i>
        </div>
        <div class="content-info">
            <h5 class="content-title">Ki?m tra cu?i ch??ng</h5>
            <span class="content-type-badge test">Ki?m tra</span>
        </div>
    </div>
    <div class="content-action">
        <a href="/test/take/456" class="btn btn-success content-action-btn">
            <i class="fas fa-pencil-alt me-2"></i>Làm bài ki?m tra
        </a>
    </div>
</div>
```

## Design Details

### Color Scheme

| Content Type | Icon Background | Icon Color | Badge | Border |
|--------------|----------------|------------|-------|---------|
| **Theory** | `#e3f2fd` | `#1976d2` | Blue | `#1976d2` |
| **Flashcard** | `#fff3e0` | `#f57c00` | Orange | `#f57c00` |
| **Test** | `#e8f5e9` | `#388e3c` | Green | `#388e3c` |

### Spacing & Sizing

- **Section padding**: `2rem`
- **Item gap**: `1rem`
- **Item padding**: `1.5rem`
- **Border radius**: `12px`
- **Icon size**: `48x48px` (desktop), `40x40px` (mobile)
- **Border left**: `4px` solid

### Hover Effects

```css
.content-item:hover {
    border-color: #d1d7dc;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
    transform: translateY(-2px);
}
```

### Button Hover

```css
.content-action-btn:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}
```

## Responsive Design

### Desktop (>768px)
- Icon: 48x48px
- Section padding: 2rem
- Full content preview height

### Mobile (?768px)
- Icon: 40x40px
- Section padding: 1rem
- Reduced button padding
- Smaller font sizes

## Logic & Conditions

### When to Display

Section ch? hi?n th? khi:
```csharp
@if (currentLesson?.LessonContents != null && 
     currentLesson.LessonContents.Any(c => c.ContentType != "Video"))
{
    // Display section
}
```

### Filter Logic

```csharp
// Theory: Hi?n th? n?u có Body
@if (content.ContentType == "Theory" && !string.IsNullOrEmpty(content.Body))
{
    // Render theory item
}

// Flashcard: Hi?n th? n?u có RefId
@if (content.ContentType == "FlashcardSet" && content.RefId.HasValue)
{
    // Render flashcard item with link
}

// Test: Hi?n th? n?u có RefId
@if (content.ContentType == "Test" && content.RefId.HasValue)
{
    // Render test item with link
}
```

## User Interactions

### 1. **Theory Content**
- Ng??i dùng ??c preview
- Scroll ?? xem thêm n?i dung
- No direct action button

### 2. **Flashcard Content**
- Click button "Luy?n t?p Flashcard"
- Open trong tab m?i
- Navigate to `/Flashcard/Study/{setId}`

### 3. **Test Content**
- Click button "Làm bài ki?m tra"
- Open trong tab m?i
- Navigate to `/Test/Take/{id}`

## Integration v?i Course Builder

Content ???c t?o trong Course Builder:

### Step 3: Lesson Contents
```javascript
// Course Builder cho phép t?o các lo?i content:
- Theory: CKEditor ?? nh?p HTML
- FlashcardSet: T?o các flashcards
- Test: T?o câu h?i và ?áp án
```

### Database Schema
```sql
CourseContents:
- ContentId
- LessonId
- ContentType (Theory/Video/FlashcardSet/Test)
- RefId (for FlashcardSet/Test)
- Title
- Body (for Theory)
- VideoUrl (for Video)
- OrderIndex
```

## Testing Checklist

### Visual
- [ ] Theory items hi?n th? ?úng HTML preview
- [ ] Flashcard items có button màu primary
- [ ] Test items có button màu success
- [ ] Icons màu s?c ?úng theo lo?i
- [ ] Badges hi?n th? ?úng text
- [ ] Border left màu ?úng
- [ ] Hover effects ho?t ??ng

### Functional
- [ ] Theory content scrollable
- [ ] Flashcard button m? ?úng link
- [ ] Test button m? ?úng link
- [ ] Links m? trong tab m?i
- [ ] OrderIndex ???c respect

### Responsive
- [ ] Desktop: layout ??p
- [ ] Tablet: không b? v?
- [ ] Mobile: icon và text size phù h?p
- [ ] Scroll works trên mobile

## Example Data

### Theory Content
```json
{
  "contentType": "Theory",
  "title": "Gi?i thi?u v? OOP",
  "body": "<h2>Object-Oriented Programming</h2><p>OOP là...</p>",
  "orderIndex": 0
}
```

### Flashcard Content
```json
{
  "contentType": "FlashcardSet",
  "title": "T? v?ng ch??ng 1",
  "refId": 123,
  "orderIndex": 1
}
```

### Test Content
```json
{
  "contentType": "Test",
  "title": "Ki?m tra cu?i ch??ng",
  "refId": 456,
  "orderIndex": 2
}
```

## Future Enhancements

### Short-term
- [ ] Show flashcard count
- [ ] Show test duration
- [ ] Progress indicators
- [ ] Completion checkmarks

### Long-term
- [ ] Inline flashcard practice
- [ ] Inline test preview
- [ ] Content bookmarking
- [ ] Content notes
- [ ] Print-friendly view

## Troubleshooting

### Content không hi?n th?
1. Check `LessonContents` có data không
2. Check `ContentType` ?úng format
3. Check `RefId` t?n t?i (for Flashcard/Test)
4. Check `Body` không empty (for Theory)

### Style b? v?
1. Check `course-learn.css` loaded
2. Check Bootstrap classes
3. Check Font Awesome icons
4. Inspect browser DevTools

### Link không ho?t ??ng
1. Check `RefId` correct
2. Check routes trong controller
3. Check authorization
4. Check target="_blank"

## Best Practices

### Content Creation
1. **Theory**: Keep HTML clean và semantic
2. **Flashcard**: Descriptive titles
3. **Test**: Clear instructions
4. **Order**: Logical progression

### UX
1. Group similar content types
2. Clear visual hierarchy
3. Consistent button styles
4. Smooth transitions

### Performance
1. Limit HTML preview height
2. Lazy load images in theory
3. Optimize CKEditor output
4. Cache content queries

## Conclusion

Section "N?i dung bài h?c" cung c?p:
- ? Hi?n th? tr?c quan các lo?i n?i dung
- ? Easy navigation ??n Flashcard và Test
- ? Preview n?i dung Theory
- ? Responsive design
- ? Hover effects m??t mà
- ? Color coding theo lo?i content

Giúp h?c viên d? dàng truy c?p và t??ng tác v?i t?t c? n?i dung c?a bài h?c!
