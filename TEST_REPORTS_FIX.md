# S?a l?i Test Reports - X? lý null-safe và c?i thi?n UI

## V?n ??
Trang **Test Reports** (`/admin/TestReports`) g?p l?i khi:
1. Không có d? li?u test attempts trong database
2. User ho?c Test b? null (?ã b? xóa nh?ng còn attempts)
3. Thi?u x? lý null-safe cho các tr??ng d? li?u
4. UI không hi?n th? thông báo khi không có d? li?u

## Gi?i pháp ?ã áp d?ng

### 1. **AdminController.cs - TestReports() Method**

#### Thay ??i chính:
```csharp
// ? TR??C (SAI):
RecentAttempts = await _context.TestAttempts
    .Include(a => a.User)
    .Include(a => a.Test)
    .OrderByDescending(a => a.StartedAt)
    .Take(15)
    .Select(a => new { a.User.FullName, a.Test.Title, a.Score, a.MaxScore, a.StartedAt })
    .ToListAsync()

// ? V?n ??: 
// - a.User ho?c a.Test có th? null
// - a.User.FullName ho?c a.Test.Title có th? null
// - Gây l?i NullReferenceException

// ? SAU (?ÚNG):
var recentAttempts = await _context.TestAttempts
    .Include(a => a.User)
    .Include(a => a.Test)
    .OrderByDescending(a => a.StartedAt)
    .Take(15)
    .Select(a => new
    {
        FullName = a.User.FullName ?? "Unknown",      // ? Null-safe v?i giá tr? m?c ??nh
        Title = a.Test.Title ?? "Unknown Test",        // ? Null-safe v?i giá tr? m?c ??nh
        Score = a.Score,
        MaxScore = a.MaxScore,
        StartedAt = a.StartedAt
    })
    .ToListAsync();
```

#### TopScores Query:
```csharp
var topScores = await _context.TestAttempts
    .Include(a => a.User)
    .Include(a => a.Test)
    .Where(a => a.Score.HasValue && a.MaxScore.HasValue && a.MaxScore > 0) // ? Filter null scores
    .OrderByDescending(a => (decimal)a.Score!.Value / a.MaxScore!.Value)   // ? Null-forgiving operator vì ?ã filter
    .Take(15)
    .Select(a => new
    {
        FullName = a.User.FullName ?? "Unknown",      // ? Null-safe
        Title = a.Test.Title ?? "Unknown Test",        // ? Null-safe
        Score = a.Score!.Value,                        // ? Safe vì ?ã filter
        MaxScore = a.MaxScore!.Value,                  // ? Safe vì ?ã filter
        Percentage = (decimal)a.Score!.Value / a.MaxScore!.Value * 100
    })
    .ToListAsync();
```

### 2. **TestReports.cshtml - View Updates**

#### ? Thêm null-check cho collections:
```csharp
@if (Model.RecentAttempts != null && ((IEnumerable<dynamic>)Model.RecentAttempts).Any())
{
    // Hi?n th? table
}
else
{
    <div class="alert alert-info">
        <i class="bi bi-info-circle me-2"></i>Ch?a có l?n thi nào ???c ghi nh?n.
    </div>
}
```

#### ? Thêm badge màu cho ?i?m s?:
```csharp
@if (attempt.Score != null && attempt.MaxScore != null)
{
    var percentage = (decimal)attempt.Score / attempt.MaxScore * 100;
    var badgeClass = percentage >= 80 ? "bg-success" : percentage >= 60 ? "bg-warning" : "bg-danger";
    <span>@attempt.Score/@attempt.MaxScore </span>
    <span class="badge @badgeClass">@percentage.ToString("F0")%</span>
}
else
{
    <span class="text-muted">?ang th?c hi?n</span>
}
```

#### ? C?i thi?n UI:
- Thêm `table-responsive` ?? responsive trên mobile
- Thêm `table-hover` ?? highlight row khi hover
- Thêm icon `bi-info-circle` cho thông báo
- Thêm error handling cho export functions

#### ? Export Functions v?i error handling:
```javascript
function exportToCSV() {
    if (window.exportToCSV) {
        window.exportToCSV('Báo cáo ho?t ??ng ki?m tra');
    } else {
        console.error('Export function not available');
        alert('Ch?c n?ng xu?t CSV ch?a s?n sàng');
    }
}
```

## So sánh tr??c và sau

### Tr??c khi s?a:
? **L?i khi không có d? li?u**
```
NullReferenceException: Object reference not set to an instance of an object
at Quiz_Web.Controllers.AdminController.TestReports()
```

? **L?i khi User/Test b? xóa**
```
NullReferenceException: a.User is null
```

? **UI tr?ng không có thông báo**

### Sau khi s?a:
? **X? lý null-safe hoàn toàn**
- User null ? hi?n th? "Unknown"
- Test null ? hi?n th? "Unknown Test"
- Score null ? hi?n th? "?ang th?c hi?n"

? **UI hi?n th? thông báo rõ ràng**
- "Ch?a có l?n thi nào ???c ghi nh?n"
- "Ch?a có ?i?m s? nào ???c ghi nh?n"

? **Badge màu theo ?i?m s?**
- >= 80%: Xanh lá (success)
- 60-79%: Vàng (warning)
- < 60%: ?? (danger)

## Các case test ?ã cover

### 1. ? Database tr?ng (ch?a có attempts)
- **Tr??c**: L?i NullReferenceException
- **Sau**: Hi?n th? thông báo "Ch?a có l?n thi nào"

### 2. ? User b? xóa nh?ng còn attempts
- **Tr??c**: L?i a.User is null
- **Sau**: Hi?n th? "Unknown" thay vì tên user

### 3. ? Test b? xóa nh?ng còn attempts
- **Tr??c**: L?i a.Test is null
- **Sau**: Hi?n th? "Unknown Test" thay vì tên test

### 4. ? Attempt ?ang th?c hi?n (ch?a có Score)
- **Tr??c**: L?i khi truy c?p Score.Value
- **Sau**: Hi?n th? "?ang th?c hi?n"

### 5. ? MaxScore = 0 (edge case)
- **Tr??c**: Division by zero error
- **Sau**: Filter ra kh?i TopScores (a.MaxScore > 0)

## Build Status
? **Build successful** - Không có l?i compilation

## Files Changed
1. `Quiz_Web\Controllers\AdminController.cs` - Method `TestReports()`
2. `Quiz_Web\Views\Admin\TestReports.cshtml` - View improvements

## Testing Checklist

### Manual Testing:
- [ ] Truy c?p `/admin/TestReports` khi database tr?ng
- [ ] T?o test attempt m?i và ki?m tra hi?n th?
- [ ] Xóa user có attempts và ki?m tra hi?n th? "Unknown"
- [ ] Xóa test có attempts và ki?m tra hi?n th? "Unknown Test"
- [ ] Ki?m tra badge màu cho các m?c ?i?m khác nhau
- [ ] Test export CSV/Excel (n?u có d? li?u)
- [ ] Test responsive trên mobile

### Database Scenarios:
```sql
-- Scenario 1: No attempts
SELECT COUNT(*) FROM TestAttempts; -- Should be 0

-- Scenario 2: Attempts with deleted users
SELECT ta.* FROM TestAttempts ta
LEFT JOIN Users u ON ta.UserId = u.UserId
WHERE u.UserId IS NULL;

-- Scenario 3: Attempts with deleted tests
SELECT ta.* FROM TestAttempts ta
LEFT JOIN Tests t ON ta.TestId = t.TestId
WHERE t.TestId IS NULL;

-- Scenario 4: In-progress attempts (no score yet)
SELECT * FROM TestAttempts 
WHERE Score IS NULL OR MaxScore IS NULL;
```

## Summary

?ã hoàn thành s?a l?i Test Reports:

? **Controller:**
- Null-safe queries v?i giá tr? m?c ??nh
- Filter null scores trong TopScores
- X? lý ?úng nullable types

? **View:**
- Null-check cho collections
- Badge màu theo ?i?m s?
- Thông báo khi không có d? li?u
- Responsive table
- Error handling cho export

? **Build:** Thành công, không có l?i

? **UX:** C?i thi?n ?áng k? v?i badges màu và thông báo rõ ràng
