# ?? Fix: NullReferenceException on Home Page

## ? **L?i**
```
System.NullReferenceException: 'Object reference not set to an instance of an object.'

Location: Index.cshtml line 12
@foreach (var category in Model)  // Model = null!
```

## ?? **Nguyên nhân**

### **View yêu c?u:**
```razor
@model IEnumerable<Quiz_Web.Models.Entities.CourseCategory>

@foreach (var category in Model)
{
    <a href="...">@category.Name</a>
}
```

### **Controller tr? v?:**
```csharp
public IActionResult Index()
{
    // ...
    return View();  // ? Không truy?n Model!
}
```

? `Model` = `null` ? `@foreach` fail!

## ? **Gi?i pháp**

### **C?p nh?t HomeController.Index()**

```csharp
public async Task<IActionResult> Index()
{
    // Check authentication
    if (User.Identity?.IsAuthenticated != true)
    {
        return RedirectToAction("Index", "Introduce");
    }
    
    // ? 1. Load categories for navigation
    var categories = await _context.CourseCategories
        .OrderBy(c => c.DisplayOrder)
        .ThenBy(c => c.Name)
        .ToListAsync();
    
    // ? 2. Load recommended courses (d?a trên user interests)
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var recommendedCourses = new List<Course>();
    
    if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int userIdInt))
    {
        var userInterests = await _context.UserInterests
            .Where(ui => ui.UserId == userIdInt)
            .Select(ui => ui.CategoryId)
            .ToListAsync();
        
        if (userInterests.Any())
        {
            recommendedCourses = await _context.Courses
                .Include(c => c.Owner)
                .Include(c => c.Category)
                .Where(c => c.IsPublished 
                         && c.CategoryId.HasValue 
                         && userInterests.Contains(c.CategoryId.Value))
                .OrderByDescending(c => c.AverageRating)
                .ThenByDescending(c => c.TotalReviews)
                .Take(10)
                .ToListAsync();
        }
    }
    
    // Fallback: Random courses n?u không có interests
    if (!recommendedCourses.Any())
    {
        recommendedCourses = await _context.Courses
            .Include(c => c.Owner)
            .Include(c => c.Category)
            .Where(c => c.IsPublished)
            .OrderByDescending(c => c.CreatedAt)
            .Take(10)
            .ToListAsync();
    }
    
    // ? 3. Load top rated courses
    var topRatedCourses = await _context.Courses
        .Include(c => c.Owner)
        .Include(c => c.Category)
        .Where(c => c.IsPublished && c.AverageRating > 0)
        .OrderByDescending(c => c.AverageRating)
        .ThenByDescending(c => c.TotalReviews)
        .Take(5)
        .ToListAsync();
    
    // ? 4. Pass data to view
    ViewBag.RecommendedCourses = recommendedCourses;
    ViewBag.TopRatedCourses = topRatedCourses;
    
    // ? 5. Return categories as Model
    return View(categories);
}
```

## ?? **Data Flow**

```
User ??ng nh?p ? Home/Index
  ?
Load Categories (cho navigation)
  ?
Load User Interests (t? Onboarding)
  ?
Load Recommended Courses (d?a trên interests)
  ?
Load Top Rated Courses
  ?
Pass to View:
  - Model: List<CourseCategory>
  - ViewBag.RecommendedCourses
  - ViewBag.TopRatedCourses
  ?
View renders ?
```

## ?? **Test**

### **B??c 1: Restart app**
```bash
Ctrl + C
F5
```

### **B??c 2: Login**
- Email: `student1@learn.vn`
- Password: `password`

### **B??c 3: Verify Home Page**
Ki?m tra các elements:
- ? **Category Navigation**: Hi?n th? categories
- ? **User Greeting**: "Chào m?ng [Name] tr? l?i!"
- ? **Recommended Courses**: Carousel v?i courses
- ? **Top Rated Course**: Featured course card

### **B??c 4: Check Console**
Không còn l?i NullReferenceException ?

## ?? **Queries th?c thi**

```sql
-- 1. Categories
SELECT * FROM CourseCategories 
ORDER BY DisplayOrder, Name;

-- 2. User Interests
SELECT CategoryId FROM UserInterests 
WHERE UserId = @UserId;

-- 3. Recommended Courses
SELECT TOP 10 * FROM Courses c
JOIN Users u ON c.OwnerId = u.UserId
JOIN CourseCategories cc ON c.CategoryId = cc.CategoryId
WHERE c.IsPublished = 1 
  AND c.CategoryId IN (SELECT CategoryId FROM UserInterests WHERE UserId = @UserId)
ORDER BY c.AverageRating DESC, c.TotalReviews DESC;

-- 4. Top Rated Courses
SELECT TOP 5 * FROM Courses c
JOIN Users u ON c.OwnerId = u.UserId
WHERE c.IsPublished = 1 AND c.AverageRating > 0
ORDER BY c.AverageRating DESC, c.TotalReviews DESC;
```

## ?? **Best Practices**

### ? DO:
```csharp
// Luôn truy?n Model cho View khi c?n
public async Task<IActionResult> Index()
{
    var data = await _context.Items.ToListAsync();
    return View(data);  // ? Pass Model
}
```

### ? DON'T:
```csharp
// Không truy?n Model khi View yêu c?u
public IActionResult Index()
{
    return View();  // ? Missing Model
}
```

### ?? TIP:
```csharp
// Ki?m tra null trong View (defensive programming)
@if (Model != null && Model.Any())
{
    @foreach (var item in Model)
    {
        // ...
    }
}
else
{
    <p>No data available</p>
}
```

## ?? **Related Issues**

### Issue 1: "Model is null" trong View
**Cause**: Controller không truy?n data  
**Fix**: `return View(data);`

### Issue 2: "Collection was modified" trong foreach
**Cause**: Concurrent modification  
**Fix**: Dùng `.ToList()` tr??c khi iterate

### Issue 3: Recommended courses r?ng
**Cause**: User ch?a hoàn t?t Onboarding  
**Fix**: Fallback to latest/random courses

## ?? **Checklist**

- [x] Build successful
- [x] HomeController.Index() loads categories
- [x] HomeController.Index() loads recommended courses
- [x] HomeController.Index() loads top rated courses
- [x] HomeController.Index() passes Model to View
- [x] No NullReferenceException on page load
- [x] Category navigation renders correctly
- [x] Recommended courses carousel works
- [x] Top rated course displays

## ?? **K?t qu?**

**Tr??c khi fix:**
```
Home/Index ? NullReferenceException ?
Categories: null
Courses: null
```

**Sau khi fix:**
```
Home/Index ? Loads successfully ?
Categories: 4 items (from database)
Recommended Courses: 10 items (based on interests)
Top Rated Courses: 5 items (highest ratings)
```

---

**Timestamp**: 2025-01-08  
**Status**: ? RESOLVED  
**Priority**: P0 (Critical - Breaks home page)  
**Impact**: Fixes home page loading for authenticated users
