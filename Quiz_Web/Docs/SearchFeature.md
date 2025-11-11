# Tính n?ng Tìm ki?m Khóa h?c

## T?ng quan
H? th?ng tìm ki?m khóa h?c cho phép ng??i dùng tìm ki?m các khóa h?c theo t? khóa **ngay t? thanh tìm ki?m trên header** c?a trang web. Không c?n vào trang riêng ?? tìm ki?m.

## ? C?p nh?t m?i nh?t

### Improvement: Centralized Search in Header (11/01/2025)
**Thay ??i**: Di chuy?n tìm ki?m vào header layout thay vì trong t?ng trang
**L?i ích**:
- ? Consistent UX - Thanh tìm ki?m luôn có m?t trên m?i trang
- ? Gi?m duplicate code - Không c?n thanh tìm ki?m riêng trong trang
- ? Better navigation - Tìm ki?m t? b?t k? ?âu trên website
- ? Cleaner UI - Trang k?t qu? t?p trung vào filters và results

**Files changed**:
- `_Layout.cshtml`: Thanh tìm ki?m trong header (?ã có s?n)
- `Course/Index.cshtml`: Xóa duplicate search section
- `course-index.css`: Xóa unused search styles

### Fix: Thanh tìm ki?m không click ???c (11/01/2025)
**V?n ??**: Icon search trong header không th? click, ch? có th? search b?ng cách nh?n Enter
**Gi?i pháp**: 
- Chuy?n icon search t? `<span>` thành `<button type="submit">`
- Thêm hover effect cho button search
- Gi? nguyên kh? n?ng search b?ng Enter key

**Thay ??i**:
```razor
<!-- Tr??c -->
<span class="input-group-text">
    <i class="bi bi-search"></i>
</span>

<!-- Sau -->
<button type="submit" class="input-group-text bg-transparent border-0" style="cursor: pointer;" title="Tìm ki?m">
    <i class="bi bi-search"></i>
</button>
```

**CSS m?i**:
```css
.header-search button.input-group-text:hover {
    color: #5624d0;
}

.header-search button.input-group-text:active {
    color: #401b9c;
}
```

## Cách s? d?ng

### T? phía ng??i dùng
1. **T? b?t k? trang nào** - Nh?p t? khóa vào search bar trên header
2. **Cách 1**: Nh?p t? khóa ? Nh?n Enter
3. **Cách 2**: Nh?p t? khóa ? Click vào icon search (??)
4. H? th?ng s? chuy?n ??n trang k?t qu? v?i b? l?c và s?p x?p

### Tính n?ng tìm ki?m
- **Tìm ki?m theo tiêu ??**: Tìm ki?m các khóa h?c có tiêu ?? ch?a t? khóa
- **Tìm ki?m theo mô t?**: Tìm ki?m trong ph?n mô t? tóm t?t c?a khóa h?c
- **B? l?c k?t qu?**: Ng??i dùng có th? l?c k?t qu? theo:
  - ?ánh giá (Rating)
  - Giá (Mi?n phí/Tr? phí)
  - S?p x?p (M?i nh?t, ?ánh giá cao nh?t, Giá)
- **Phân trang**: K?t qu? ???c phân trang ?? d? dàng xem

## Lu?ng k? thu?t

### 1. Frontend (Layout Header)
**File**: `Quiz_Web/Views/Shared/_Layout.cshtml`

```razor
<form asp-controller="Course" asp-action="Search" method="get" class="d-flex flex-grow-1 my-2 my-lg-0 mx-lg-4" role="search">
    <div class="input-group header-search w-100">
        <button type="submit" class="input-group-text bg-transparent border-0" style="cursor: pointer;" title="Tìm ki?m">
            <i class="bi bi-search"></i>
        </button>
        <input class="form-control" type="search" name="q" placeholder="Tìm ki?m khóa h?c..." aria-label="Search" autocomplete="off">
    </div>
</form>
```

**??c ?i?m**:
- Form submit ??n `Course/Search` v?i ph??ng th?c GET
- Tham s? tìm ki?m: `q` (query)
- S? d?ng ASP.NET Core Tag Helpers ?? generate URL
- **Có m?t trên m?i trang** s? d?ng `_Layout.cshtml`

### 2. Controller Action
**File**: `Quiz_Web/Controllers/CourseController.cs`

```csharp
[Route("/courses/search")]
[HttpGet]
public IActionResult Search(
    string q, 
    int page = 1, 
    int pageSize = 12,
    decimal? minRating = null,
    decimal? maxRating = null,
    bool? isFree = null,
    string? sortBy = null)
{
    // Validate search query
    if (string.IsNullOrWhiteSpace(q))
        return RedirectToAction(nameof(Index));

    // Get filtered courses from service
    var courses = _courseService.GetFilteredAndSortedCourses(
        searchKeyword: q,
        categorySlug: null,
        minRating: minRating,
        maxRating: maxRating,
        isFree: isFree,
        sortBy: sortBy);

    // Set ViewBag data for view
    ViewBag.SearchKeyword = q;
    ViewBag.MinRating = minRating;
    ViewBag.MaxRating = maxRating;
    ViewBag.IsFree = isFree;
    ViewBag.SortBy = sortBy;

    // Pagination
    var totalCount = courses.Count;
    var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    var pagedCourses = courses.Skip((page - 1) * pageSize).Take(pageSize).ToList();

    ViewBag.TotalCount = totalCount;
    ViewBag.TotalPages = totalPages;
    ViewBag.CurrentPage = page;
    ViewBag.PageSize = pageSize;

    return View("Index", pagedCourses);
}
```

### 3. Service Layer
**File**: `Quiz_Web/Services/CourseService.cs`

Ph??ng th?c `GetFilteredAndSortedCourses`:
- Tìm ki?m theo `searchKeyword` trong `Title` và `Summary`
- L?c theo `minRating`, `maxRating`, `isFree`
- S?p x?p theo `sortBy` (newest, rating, price_asc, price_desc)
- Tr? v? danh sách khóa h?c ?ã l?c

### 4. View - Trang k?t qu?
**File**: `Quiz_Web/Views/Course/Index.cshtml`

View này hi?n th?:
- ~~Thanh tìm ki?m riêng trong trang k?t qu?~~ **REMOVED**
- Sidebar b? l?c (?ánh giá, giá)
- Danh sách khóa h?c d?ng grid
- Phân trang
- T?ng s? k?t qu?

**L?u ý**: Không còn duplicate search bar trong trang này n?a.

## URL Routes

### Tìm ki?m c? b?n
```
GET /courses/search?q=javascript
```

### Tìm ki?m v?i b? l?c
```
GET /courses/search?q=javascript&minRating=4.0&isFree=true&page=1
```

### Tìm ki?m v?i s?p x?p
```
GET /courses/search?q=python&sortBy=rating
```

## Styling

### CSS Files
1. **Header Search Bar**: `Quiz_Web/wwwroot/css/site.css`
   - Class: `.header-search`
   - Responsive design
   - Hover và focus states
   - Button click states

2. **Search Results Page**: `Quiz_Web/wwwroot/css/course/course-index.css`
   - ~~Search wrapper~~ **REMOVED**
   - Filter sidebar
   - Course cards
   - Pagination

## Responsive Design

### Desktop (?992px)
- Search bar m? r?ng ??y ?? gi?a logo và các menu bên ph?i
- Hi?n th? ??y ?? b? l?c sidebar
- Grid 3 c?t cho khóa h?c

### Tablet (768px - 991px)
- Search bar v?n hi?n th? trong header
- B? l?c có th? collapse
- Grid 2 c?t

### Mobile (<768px)
- Search bar collapse vào menu hamburger
- B? l?c ? trên danh sách khóa h?c
- Grid 1 c?t

## User Flow

### Scenario 1: Search from Homepage
1. User ? trang ch?
2. Nh?p "javascript" vào search bar header
3. Click icon ho?c nh?n Enter
4. ? Redirect: `/courses/search?q=javascript`
5. Hi?n th? k?t qu? v?i filters

### Scenario 2: Search from Any Page
1. User ?ang ? b?t k? trang nào (Course Detail, Account, etc.)
2. Header search bar luôn có m?t
3. Nh?p t? khóa và search
4. ? Redirect ??n trang k?t qu?

### Scenario 3: Search + Filter
1. Search "python"
2. Apply filter: Rating 4.0+, Free only
3. URL: `/courses/search?q=python&minRating=4.0&isFree=true`

## Testing

### Test Cases
1. ? Tìm ki?m v?i t? khóa h?p l?
2. ? Tìm ki?m v?i t? khóa r?ng (redirect v? Index)
3. ? Tìm ki?m không có k?t qu?
4. ? Tìm ki?m + b? l?c rating
5. ? Tìm ki?m + b? l?c giá
6. ? Tìm ki?m + s?p x?p
7. ? Phân trang k?t qu?
8. ? Responsive trên mobile/tablet
9. ? **Search from any page** (NEW)
10. ? **No duplicate search bars** (NEW)

### Test URLs
```
/courses/search?q=web
/courses/search?q=javascript&minRating=4.0
/courses/search?q=python&isFree=true
/courses/search?q=design&sortBy=newest&page=2
```

## C?i ti?n trong t??ng lai

### G?i ý (Suggestions)
1. **Autocomplete**: G?i ý t? khóa trong khi gõ
2. **Recent Searches**: L?u l?ch s? tìm ki?m
3. **Popular Searches**: Hi?n th? các t? khóa ph? bi?n

### Nâng cao tìm ki?m
1. **Full-text Search**: S? d?ng SQL Server Full-Text Search
2. **Search by Instructor**: Tìm theo tên gi?ng viên
3. **Advanced Filters**: Thêm b? l?c theo danh m?c, th?i l??ng, ngôn ng?
4. **Search Analytics**: Theo dõi các t? khóa ???c tìm nhi?u nh?t

### UX Improvements
1. **Highlight Keywords**: Làm n?i b?t t? khóa trong k?t qu?
2. **Search Suggestions**: "Có ph?i b?n mu?n tìm..."
3. **Filter Badges**: Hi?n th? các filter ?ang active
4. **Quick Filters**: One-click filters ph? bi?n

## Troubleshooting

### V?n ?? th??ng g?p

**1. Search không ho?t ??ng**
- Ki?m tra form có `asp-controller="Course"` và `asp-action="Search"`
- Ki?m tra input có `name="q"`
- Ki?m tra route trong `CourseController.cs`

**2. Không tìm th?y k?t qu?**
- Ki?m tra d? li?u trong database
- Ki?m tra logic trong `GetFilteredAndSortedCourses`
- Xem log trong `CourseController.Search`

**3. CSS không hi?n th? ?úng**
- Xóa cache browser (Ctrl+Shift+Delete)
- Ki?m tra file CSS ?ã ???c include trong `_Layout.cshtml`
- Ki?m tra `asp-append-version="true"` ?? force reload

**4. Duplicate search bars**
- **FIXED**: Ch? có 1 search bar trong header
- Không còn search bar trong trang Index n?a

## Liên quan

### Files liên quan
- Controller: `Quiz_Web/Controllers/CourseController.cs`
- Service: `Quiz_Web/Services/CourseService.cs`
- View: `Quiz_Web/Views/Course/Index.cshtml`
- Layout: `Quiz_Web/Views/Shared/_Layout.cshtml` ? **MAIN SEARCH**
- CSS: `Quiz_Web/wwwroot/css/site.css` (Header search)
- CSS: `Quiz_Web/wwwroot/css/course/course-index.css` (Results page)

### Tính n?ng t??ng t?
- Category browsing: `/courses/category/{slug}`
- All courses: `/courses`
- Course detail: `/courses/{slug}`

## Architecture

```
???????????????????????????????????????????
?         _Layout.cshtml (Header)         ?
?    ????????????????????????????????    ?
?    ?   Search Bar (Always Visible)?    ?
?    ?   ? Submit to Course/Search   ?    ?
?    ????????????????????????????????    ?
???????????????????????????????????????????
                    ?
???????????????????????????????????????????
?      CourseController.Search()          ?
?  ?? Validate query                      ?
?  ?? Call GetFilteredAndSortedCourses()  ?
?  ?? Return View("Index", courses)       ?
???????????????????????????????????????????
                    ?
???????????????????????????????????????????
?         Course/Index.cshtml             ?
?  ?? NO search bar (removed)             ?
?  ?? Filter sidebar                      ?
?  ?? Course grid                         ?
?  ?? Pagination                          ?
???????????????????????????????????????????
```

## Benefits of Centralized Search

### ? Pros
1. **Consistency**: Search looks and behaves the same everywhere
2. **Accessibility**: Search always available, no need to navigate to specific page
3. **Less Code**: No duplicate search implementations
4. **Better UX**: Users know where to search from any page
5. **Cleaner Pages**: Results page focuses on filters and results

### ?? Considerations
1. Header always takes up space (but worth it for UX)
2. Mobile: Search in collapsed menu (acceptable pattern)
3. Must maintain search keyword across filters (? implemented)
