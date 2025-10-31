# Dropdown Menu "Khám phá" - H??ng d?n

## T?ng quan
Component dropdown menu "Khám phá" hi?n th? danh sách các danh m?c khóa h?c ???c l?y t? database b?ng `CourseCategories`.

## C?u trúc

### 1. Database
- **B?ng**: `dbo.CourseCategories`
- **Các tr??ng quan tr?ng**:
  - `CategoryId`: ID duy nh?t
  - `Name`: Tên danh m?c (hi?n th? trong menu)
  - `Slug`: URL-friendly identifier
  - `IconUrl`: ???ng d?n ??n icon
  - `DisplayOrder`: Th? t? hi?n th?

### 2. Backend (Service Layer)
- **Interface**: `ICourseService.GetAllCategories()`
- **Implementation**: `CourseService.GetAllCategories()`
- Tr? v? danh sách categories ???c s?p x?p theo `DisplayOrder`, sau ?ó theo `Name`

### 3. Frontend (View)
- **File**: `Views/Shared/_Layout.cshtml`
- **Dependency Injection**: `@inject ICourseService CourseService`
- **Render**: S? d?ng `foreach` loop ?? t?o menu items t? database

### 4. Styling (CSS)
- **File**: `wwwroot/css/site.css`
- **Classes chính**:
  - `.explore-dropdown`: Container
  - `.explore-dropdown-menu`: Dropdown panel
  - `.explore-item`: Menu item
  - `.explore-icon`: Category icon

## Tính n?ng

### Giao di?n
- ? Dropdown panel hình ch? nh?t, n?n tr?ng
- ? Bo góc 8px
- ? Box shadow ?? t?o hi?u ?ng n?i
- ? Width c? ??nh: 280px
- ? Padding: 8px

### Menu Items
- ? Icon 20x20px t? IconUrl
- ? Margin-right: 12px gi?a icon và text
- ? Font-size: 16px
- ? Padding: 10px 16pxchỉnh
- ? Border-radius: 4px

### T??ng tác
- ? Hover: Background chuy?n sang #F5F5F5
- ? Cursor: pointer
- ? Smooth transitions
- ? Dropdown animation (fadeIn)

### Responsive
- ? ?n trên mobile (< 992px)
- ? Scrollbar custom khi có nhi?u items
- ? Max-height: 500px v?i overflow-y: auto

## C?u hình Icons

### Cách 1: S? d?ng Bootstrap Icons (hi?n t?i)
```sql
UPDATE dbo.CourseCategories
SET IconUrl = 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/icons/code-slash.svg'
WHERE Name = N'L?p trình';
```

### Cách 2: S? d?ng custom icons
1. ??t icon vào `wwwroot/asset/icons/categories/`
2. Update database:
```sql
UPDATE dbo.CourseCategories
SET IconUrl = '/asset/icons/categories/lap-trinh.svg'
WHERE Slug = 'lap-trinh';
```

### Cách 3: S? d?ng Font Awesome
```sql
UPDATE dbo.CourseCategories
SET IconUrl = 'fa-solid fa-code'
WHERE Name = N'L?p trình';
```
Sau ?ó c?p nh?t view ?? render `<i>` tag thay vì `<img>`.

## Thêm Category M?i

### 1. Thêm vào Database
```sql
INSERT INTO dbo.CourseCategories (Name, Slug, Description, IconUrl, DisplayOrder)
VALUES 
(N'Thi?t k?', 'thiet-ke', N'H?c thi?t k? ?? h?a, UI/UX', 
 'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/icons/palette.svg', 5);
```

### 2. T? ??ng hi?n th?
Không c?n thay ??i code! Menu s? t? ??ng c?p nh?t khi:
- Refresh trang
- Ho?c restart application (n?u có caching)

## Routing
Khi click vào m?t category, user s? ???c redirect ??n:
```
/Course/Category/{slug}
```
Ví d?: `/Course/Category/lap-trinh`

## Troubleshooting

### Icon không hi?n th?
1. Ki?m tra `IconUrl` trong database
2. ??m b?o URL accessible
3. Check CORS n?u dùng external icons

### Menu không hi?n th?
1. Ki?m tra `CourseService` ?ã ???c inject ?úng
2. Verify database connection
3. Check console ?? xem errors

### Th? t? không ?úng
Update `DisplayOrder`:
```sql
UPDATE dbo.CourseCategories
SET DisplayOrder = 1
WHERE Name = N'L?p trình';
```

## M? r?ng

### Thêm Sub-categories
Có th? m? r?ng ?? h? tr? sub-menu:
1. Thêm column `ParentCategoryId` vào b?ng
2. Update service ?? load hierarchical data
3. Render nested dropdown trong view

### Thêm Badge/Label
Hi?n th? "New" ho?c s? l??ng courses:
```html
<a class="dropdown-item explore-item" href="...">
    <img src="..." class="explore-icon" />
    <span>L?p trình</span>
    <span class="badge bg-primary ms-auto">New</span>
</a>
```

### Lazy Loading
N?u có nhi?u categories, có th? implement lazy loading ho?c pagination.

## Performance

### Caching
Nên implement caching cho `GetAllCategories()` vì data ít thay ??i:
```csharp
private static List<CourseCategory>? _cachedCategories;
private static DateTime _cacheTime;

public List<CourseCategory> GetAllCategories()
{
    if (_cachedCategories == null || 
        DateTime.Now - _cacheTime > TimeSpan.FromMinutes(30))
    {
        _cachedCategories = _context.CourseCategories
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToList();
        _cacheTime = DateTime.Now;
    }
    return _cachedCategories;
}
```

### Database Indexing
??m b?o có index trên `DisplayOrder` và `Slug`:
```sql
CREATE INDEX IX_CourseCategories_DisplayOrder ON dbo.CourseCategories(DisplayOrder);
CREATE INDEX IX_CourseCategories_Slug ON dbo.CourseCategories(Slug);
```

## Demo Data
Ch?y script `Database/UpdateCategoryIcons.sql` ?? thêm icons cho categories có s?n.
