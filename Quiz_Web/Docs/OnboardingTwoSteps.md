# H??ng d?n Onboarding 2 b??c (Profile + Interests)

## T?ng quan

H? th?ng onboarding gi? ?ây bao g?m 2 b??c:
1. **B??c 1**: Thu th?p thông tin cá nhân (UserProfile)
2. **B??c 2**: Ch?n s? thích (UserInterests)

## C?u trúc Database

### B?ng UserProfiles
```sql
CREATE TABLE dbo.UserProfiles (
    UserId INT PRIMARY KEY,
    DoB DATE NULL,
    Gender NVARCHAR(20) NULL,
    Bio NVARCHAR(500) NULL,
    SchoolName NVARCHAR(200) NULL,
    GradeLevel NVARCHAR(50) NULL,
    Locale NVARCHAR(10) NULL,
    TimeZone NVARCHAR(64) NULL,
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
```

### B?ng UserInterests
```sql
CREATE TABLE dbo.UserInterests (
    UserInterestId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    CategoryId INT NOT NULL,
    CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    FOREIGN KEY (CategoryId) REFERENCES dbo.CourseCategories(CategoryId),
    UNIQUE (UserId, CategoryId)
);
```

## Lu?ng ho?t ??ng

### 1. ??ng nh?p l?n ??u
```
User ??ng nh?p
    ?
Check: HasUserProfile() AND HasUserInterests()
    ?
N?u FALSE ? Redirect ??n /Onboarding
N?u TRUE ? Redirect ??n Home
```

### 2. B??c 1 - Thu th?p Profile
- Hi?n th? form v?i các tr??ng:
  * Ngày sinh (DatePicker)
  * Gi?i tính (Dropdown: Nam/N?/Khác)
  * Tên tr??ng (TextBox)
  * C?p/Trình ?? h?c (TextBox)
  * Gi?i thi?u ng?n (Textarea - max 500 chars)

- User có th?:
  * Click "Ti?p theo" ? Chuy?n sang B??c 2
  * Click "B? qua" ? Chuy?n th?ng sang B??c 2

### 3. B??c 2 - Ch?n s? thích
- Hi?n th? grid categories
- User ph?i ch?n ít nh?t 1 category
- Có th? "Quay l?i" B??c 1
- Click "Hoàn t?t" ? Submit form

### 4. X? lý Submit
Backend th?c hi?n 2 actions:
1. **INSERT vào UserProfiles**:
```csharp
var userProfile = new UserProfile
{
    UserId = userId,
    DoB = model.DoB,
    Gender = model.Gender,
    Bio = model.Bio,
    SchoolName = model.SchoolName,
    GradeLevel = model.GradeLevel,
    Locale = "vi-VN",
    TimeZone = "SE Asia Standard Time"
};
_context.UserProfiles.Add(userProfile);
```

2. **INSERT vào UserInterests**:
```csharp
var userInterests = model.SelectedCategoryIds.Select(categoryId => new UserInterest
{
    UserId = userId,
    CategoryId = categoryId,
    CreatedAt = DateTime.UtcNow
}).ToList();
_context.UserInterests.AddRange(userInterests);
```

3. **SaveChanges** và redirect v? Home

## UI/UX Features

### Progress Indicator
- 2 steps v?i visual progress bar
- Active step có màu gradient purple
- Completed line khi ??n step 2

### Form Validation
- Client-side: Check ít nh?t 1 category ???c ch?n
- Server-side: Validate required fields
- Error messages hi?n th? d??i form

### Interactive Elements
- Category cards có hover effect
- Selected categories có checkmark
- Submit button hi?n th? s? l??ng categories ?ã ch?n
- Smooth transitions gi?a các steps

## Test Cases

### Test 1: User m?i ??ng nh?p l?n ??u
1. T?o tài kho?n m?i
2. ??ng nh?p
3. **K? v?ng**: Redirect ??n /Onboarding
4. ?i?n thông tin ? Ch?n categories ? Submit
5. **K? v?ng**: Redirect v? Home
6. Ki?m tra database:
```sql
SELECT * FROM UserProfiles WHERE UserId = [USER_ID];
SELECT * FROM UserInterests WHERE UserId = [USER_ID];
```

### Test 2: User ?ã có profile nh?ng ch?a có interests
```sql
-- T?o profile tr??c
INSERT INTO UserProfiles (UserId, Locale, TimeZone)
VALUES (1, 'vi-VN', 'SE Asia Standard Time');

-- Xóa interests (n?u có)
DELETE FROM UserInterests WHERE UserId = 1;
```
1. ??ng nh?p
2. **K? v?ng**: V?n redirect ??n /Onboarding
3. Có th? skip Step 1
4. Ch?n interests ? Step 2
5. Submit ? Redirect Home

### Test 3: User ?ã hoàn thành onboarding
```sql
-- ??m b?o có c? profile và interests
SELECT COUNT(*) FROM UserProfiles WHERE UserId = 1; -- = 1
SELECT COUNT(*) FROM UserInterests WHERE UserId = 1; -- >= 1
```
1. ??ng nh?p
2. **K? v?ng**: Redirect th?ng v? Home (không qua onboarding)

### Test 4: Skip toàn b? onboarding
1. ??ng nh?p l?n ??u
2. Click "B? qua" ? Step 1
3. Click "Quay l?i" (optional test navigation)
4. Click "B? qua" l?i
5. Không ch?n category nào ? Click Submit
6. **K? v?ng**: Error "Vui lòng ch?n ít nh?t m?t ch? ??"
7. Ch?n 1 category ? Submit
8. **K? v?ng**: Success

### Test 5: Validate form fields
```javascript
// Step 1 - Không b?t bu?c
// User có th? ?? tr?ng t?t c? và v?n Next

// Step 2 - B?t bu?c ch?n ít nh?t 1 category
// Th? submit không ch?n gì ? L?i
```

## Customization

### Thay ??i required fields
Trong `OnboardingViewModel.cs`, thêm `[Required]`:
```csharp
[Required(ErrorMessage = "Vui lòng nh?p ngày sinh")]
[DataType(DataType.Date)]
public DateOnly? DoB { get; set; }
```

### Thêm validation phía client
Trong `Index.cshtml`, thêm vào function `nextToStep2()`:
```javascript
$('#nextToStep2').on('click', function() {
    // Validate DoB
    const dob = $('input[name="DoB"]').val();
    if (!dob) {
        showError('Vui lòng nh?p ngày sinh');
        return;
    }
    
    currentStep = 2;
    showStep(2);
});
```

### Thay ??i s? l??ng minimum categories
Trong Controller:
```csharp
if (model.SelectedCategoryIds == null || model.SelectedCategoryIds.Count < 3) {
    return Json(new { 
        success = false, 
        message = "Vui lòng ch?n ít nh?t 3 ch? ??" 
    });
}
```

## SQL Queries h?u ích

### Xem user profiles
```sql
SELECT 
    u.Username,
    u.FullName,
    up.DoB,
    up.Gender,
    up.SchoolName,
    up.GradeLevel,
    up.Bio
FROM Users u
LEFT JOIN UserProfiles up ON u.UserId = up.UserId
WHERE u.UserId = 1;
```

### Xem interests c?a user
```sql
SELECT 
    u.Username,
    c.Name AS CategoryName,
    ui.CreatedAt
FROM UserInterests ui
JOIN Users u ON ui.UserId = u.UserId
JOIN CourseCategories c ON ui.CategoryId = c.CategoryId
WHERE u.UserId = 1
ORDER BY ui.CreatedAt;
```

### Th?ng kê onboarding completion
```sql
SELECT 
    (SELECT COUNT(*) FROM Users) AS TotalUsers,
    (SELECT COUNT(*) FROM UserProfiles) AS UsersWithProfile,
    (SELECT COUNT(DISTINCT UserId) FROM UserInterests) AS UsersWithInterests,
    (SELECT COUNT(*) 
     FROM UserProfiles up
     WHERE EXISTS (
         SELECT 1 FROM UserInterests ui 
         WHERE ui.UserId = up.UserId
     )) AS CompletedOnboarding;
```

### Reset onboarding cho test user
```sql
DECLARE @TestUserId INT = 1;

-- Xóa profile
DELETE FROM UserProfiles WHERE UserId = @TestUserId;

-- Xóa interests
DELETE FROM UserInterests WHERE UserId = @TestUserId;

-- Xóa notifications (optional)
DELETE FROM Notifications WHERE UserId = @TestUserId;
```

## Troubleshooting

### L?i: "This localhost page can't be found"
1. Restart ?ng d?ng
2. Test URL: `https://localhost:7158/test-onboarding-route`
3. N?u OK ? Test `https://localhost:7158/Onboarding`

### L?i: Categories không hi?n th?
```sql
-- Ki?m tra có categories không
SELECT * FROM CourseCategories;

-- N?u r?ng, ch?y script:
INSERT INTO CourseCategories (Name, Slug, Description, DisplayOrder, CreatedAt)
VALUES 
('L?p trình', 'lap-trinh', 'H?c l?p trình và phát tri?n ph?n m?m', 1, GETUTCDATE()),
('Thi?t k?', 'thiet-ke', 'Thi?t k? ?? h?a và UI/UX', 2, GETUTCDATE()),
('Marketing', 'marketing', 'Digital Marketing và SEO', 3, GETUTCDATE());
```

### L?i: Foreign key constraint
```
The INSERT statement conflicted with the FOREIGN KEY constraint "FK_UserProfiles_User"
```
**Nguyên nhân**: UserId không t?n t?i trong b?ng Users
**Gi?i pháp**: Ki?m tra user ?ã ??ng nh?p ?úng ch?a

### L?i: Duplicate key
```
Cannot insert duplicate key in object 'dbo.UserProfiles'
```
**Nguyên nhân**: User ?ã có profile r?i
**Gi?i pháp**: Xóa profile c? ?? test l?i:
```sql
DELETE FROM UserProfiles WHERE UserId = [USER_ID];
```

## Best Practices

1. **Always validate on server-side**
   - Client validation có th? bypass
   - Server validation là b?t bu?c

2. **Use transactions** (?ã implement)
   ```csharp
   using var transaction = await _context.Database.BeginTransactionAsync();
   // ... operations ...
   await transaction.CommitAsync();
   ```

3. **Log important actions**
   ```csharp
   _logger.LogInformation("User {UserId} completed onboarding", userId);
   ```

4. **Handle errors gracefully**
   - Show user-friendly messages
   - Log detailed errors for debugging

5. **Optimize database queries**
   - Use `AnyAsync()` instead of `CountAsync() > 0`
   - Avoid N+1 queries

---

**H? th?ng onboarding 2 b??c ?ã s?n sàng! ??**
