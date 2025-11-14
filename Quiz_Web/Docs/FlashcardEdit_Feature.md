# Flashcard Edit - Ch?c n?ng ch?nh s?a th? h?c

## T?ng quan
?ã thêm tính n?ng ch?nh s?a các flashcard ?ã t?o vào trang Edit c?a Flashcard Set.

## Các thay ??i

### 1. View - `Quiz_Web/Views/Flashcard/Edit.cshtml`

#### Thêm m?i:
- **Card "Danh sách th? h?c"**: Hi?n th? t?t c? flashcard c?a set
- **Nút "Thêm th? m?i"**: Cho phép thêm th? m?i
- **Flashcard items**: M?i th? hi?n th?:
  - S? th? t? th?
  - Textarea cho m?t tr??c
  - Textarea cho m?t sau
  - Input cho g?i ý (tùy ch?n)
  - Nút xóa th?
  - Icon drag & drop ?? s?p x?p l?i th? t?

#### Tính n?ng JavaScript:
```javascript
// Load flashcards from API
loadExistingFlashcards()

// Render flashcards to UI
renderFlashcards()

// Add new flashcard
addNewFlashcard()

// Update flashcard data
updateFlashcard(index, field, value)

// Remove flashcard
removeFlashcard(index)

// Drag & drop reordering with SortableJS
initializeSortable()
```

### 2. Controller - `Quiz_Web/Controllers/FlashcardController.cs`

#### Method `Edit` (POST) - C?p nh?t:
```csharp
public async Task<IActionResult> Edit(
    int id, 
    EditFlashcardSetViewModel model, 
    IFormFile? coverFile, 
    string? flashcardsJson)
```

**Thêm parameter**: `string? flashcardsJson`

**Logic m?i**:
1. C?p nh?t thông tin flashcard set (title, description, visibility, v.v.)
2. N?u có `flashcardsJson`:
   - Parse JSON thành list FlashcardDataViewModel
   - Xóa t?t c? flashcard c? c?a set
   - T?o l?i flashcard m?i t? d? li?u ?ã ch?nh s?a

### 3. Service - `Quiz_Web/Services/FlashcardService.cs`

#### Method m?i: `DeleteAllFlashcardsInSet`
```csharp
public bool DeleteAllFlashcardsInSet(int setId, int ownerId)
{
    // Verify ownership
    // Get all flashcards in set
    // Remove all flashcards
    // Save changes
}
```

**Ch?c n?ng**: Xóa t?t c? flashcard trong m?t set (dùng khi update)

### 4. Interface - `Quiz_Web/Services/IServices/IFlashcardService.cs`

#### Thêm method signature:
```csharp
bool DeleteAllFlashcardsInSet(int setId, int ownerId);
```

## Quy trình ho?t ??ng

### 1. Load trang Edit
```
User ? /flashcards/edit/{id}
  ?
Controller: L?y thông tin FlashcardSet
  ?
View: Hi?n th? form + placeholder "?ang t?i..."
  ?
JavaScript: Call API /api/flashcards/{setId}
  ?
Render flashcards lên UI
```

### 2. Ch?nh s?a flashcard
```
User thay ??i n?i dung th?
  ?
Event onchange ? updateFlashcard(index, field, value)
  ?
C?p nh?t d? li?u trong array flashcards[]
```

### 3. Thêm/Xóa flashcard
```
User click "Thêm th? m?i"
  ?
addNewFlashcard() ? Push object m?i vào flashcards[]
  ?
renderFlashcards() ? Re-render UI

---

User click "Xóa" trên th?
  ?
removeFlashcard(index) ? Xác nh?n
  ?
flashcards.splice(index, 1)
  ?
renderFlashcards() ? Re-render UI
```

### 4. S?p x?p l?i th? t?
```
User drag & drop th?
  ?
SortableJS onEnd event
  ?
Update array flashcards[] theo th? t? m?i
  ?
Update orderIndex cho m?i th?
  ?
renderFlashcards()
```

### 5. L?u thay ??i
```
User click "C?p nh?t"
  ?
Form submit event
  ?
Validate: Title, Front/Back text c?a t?t c? th?
  ?
Set hidden field FlashcardsJson = JSON.stringify(flashcards)
  ?
Submit form ? Server
  ?
Controller: 
  - Update FlashcardSet info
  - Delete all old flashcards
  - Create new flashcards from JSON
  ?
Redirect to Detail page
```

## API Endpoint

### GET `/api/flashcards/{setId}`

**Response**:
```json
{
  "success": true,
  "flashcards": [
    {
      "cardId": 1,
      "frontText": "Hello",
      "backText": "Xin chào",
      "hint": "Greeting",
      "orderIndex": 0
    },
    ...
  ]
}
```

## Giao di?n

### Flashcard Item
```html
<div class="flashcard-item" data-index="0">
  <div class="flashcard-header">
    <div>
      <i class="fa-solid fa-grip-vertical drag-handle"></i>
      <span class="flashcard-number">Th? 1</span>
    </div>
    <button onclick="removeFlashcard(0)">
      <i class="fa-solid fa-trash"></i> Xóa
    </button>
  </div>
  
  <div class="row">
    <div class="col-md-6">
      <label>M?t tr??c *</label>
      <textarea onchange="updateFlashcard(0, 'frontText', this.value)">
        Hello
      </textarea>
    </div>
    
    <div class="col-md-6">
      <label>M?t sau *</label>
      <textarea onchange="updateFlashcard(0, 'backText', this.value)">
        Xin chào
      </textarea>
    </div>
  </div>
  
  <div>
    <label>G?i ý (tùy ch?n)</label>
    <input type="text" value="Greeting"
           onchange="updateFlashcard(0, 'hint', this.value)">
  </div>
</div>
```

### No Flashcards State
```html
<div class="no-flashcards">
  <i class="fa-solid fa-layer-group"></i>
  <h5>Ch?a có th? h?c nào</h5>
  <p class="text-muted">Nh?n nút "Thêm th? m?i" ?? b?t ??u t?o flashcard</p>
</div>
```

## Validation

### Client-side (JavaScript)
```javascript
// Ki?m tra khi submit form
- Title không ???c r?ng
- M?i flashcard ph?i có frontText
- M?i flashcard ph?i có backText
```

### Server-side (Controller)
```csharp
// Ki?m tra ownership
- User ph?i là owner c?a FlashcardSet
- SetId ph?i t?n t?i và không b? xóa

// Validate flashcard data
- FrontText: Required, Max 500 chars
- BackText: Required, Max 500 chars
- Hint: Optional, Max 200 chars
```

## CSS Styles

### Flashcard Item
```css
.flashcard-item {
    background: #f8f9fa;
    border: 2px solid #dee2e6;
    border-radius: 12px;
    padding: 1.5rem;
    margin-bottom: 1rem;
    transition: all 0.3s ease;
}

.flashcard-item:hover {
    box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    border-color: #0d6efd;
}

.drag-handle {
    cursor: move;
    color: #6c757d;
}
```

## Dependencies

### SortableJS
```html
<script src="https://cdn.jsdelivr.net/npm/sortablejs@latest/Sortable.min.js"></script>
```

**Ch?c n?ng**: Drag & drop ?? s?p x?p l?i th? t? flashcard

## Test Cases

### TC1: Load existing flashcards
```
1. Truy c?p /flashcards/edit/{id}
2. Ki?m tra: T?t c? flashcard hi?n t?i ???c hi?n th? ?úng
3. Ki?m tra: Th? t? flashcard ?úng v?i orderIndex
```

### TC2: Add new flashcard
```
1. Click "Thêm th? m?i"
2. Nh?p Front text, Back text, Hint
3. Click "C?p nh?t"
4. Ki?m tra: Flashcard m?i ???c l?u thành công
```

### TC3: Edit existing flashcard
```
1. Thay ??i n?i dung m?t flashcard
2. Click "C?p nh?t"
3. Ki?m tra: Thay ??i ???c l?u
4. Ki?m tra: Các flashcard khác không b? ?nh h??ng
```

### TC4: Delete flashcard
```
1. Click "Xóa" trên m?t flashcard
2. Xác nh?n xóa
3. Ki?m tra: Flashcard b? xóa kh?i danh sách
4. Click "C?p nh?t"
5. Ki?m tra: Flashcard ?ã b? xóa kh?i database
```

### TC5: Reorder flashcards
```
1. Drag & drop ?? ??i th? t? flashcard
2. Click "C?p nh?t"
3. Ki?m tra: Th? t? m?i ???c l?u ?úng
```

### TC6: Validation errors
```
1. ?? tr?ng Title ? L?i validation
2. ?? tr?ng Front text c?a th? ? L?i validation
3. ?? tr?ng Back text c?a th? ? L?i validation
```

## L?u ý

1. **Performance**: Khi có nhi?u flashcard (>50 th?), cân nh?c phân trang ho?c lazy loading
2. **Security**: 
   - Validate ownership trên server
   - Sanitize HTML input ?? tránh XSS
3. **UX**: 
   - Auto-scroll ??n th? m?i khi thêm
   - Confirm tr??c khi xóa th?
   - Disable nút "C?p nh?t" khi ?ang submit

## Known Issues

Không có

## Future Improvements

1. **Auto-save**: T? ??ng l?u khi user thay ??i (debounced)
2. **Undo/Redo**: Cho phép hoàn tác thay ??i
3. **Bulk operations**: Xóa nhi?u th? cùng lúc, import/export CSV
4. **Rich text editor**: Cho phép format text, thêm ?nh vào flashcard
5. **Preview mode**: Xem tr??c flashcard tr??c khi l?u

## Demo URL
```
http://localhost:7158/flashcards/mine
? Click "Edit" trên m?t flashcard set
? http://localhost:7158/flashcards/edit/{id}
```
