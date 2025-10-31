# H??ng d?n Onboarding 2 b??c (Profile + Interests)

## T?ng quan

H? th?ng onboarding gi? ?�y bao g?m 2 b??c:
1. **B??c 1**: Thu th?p th�ng tin c� nh�n (UserProfile)
2. **B??c 2**: Ch?n s? th�ch (UserInterests)

## C?u tr�c Database

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
- Hi?n th? form v?i c�c tr??ng:
  * Ng�y sinh (DatePicker)
  * Gi?i t�nh (Dropdown: Nam/N?/Kh�c)
  * T�n tr??ng (TextBox)
  * C?p/Tr�nh ?? h?c (TextBox)
  * Gi?i thi?u ng?n (Textarea - max 500 chars)

- User c� th?:
  * Click "Ti?p theo" ? Chuy?n sang B??c 2
  * Click "B? qua" ? Chuy?n th?ng sang B??c 2

### 3. B??c 2 - Ch?n s? th�ch
- Hi?n th? grid categories
- User ph?i ch?n �t nh?t 1 category
- C� th? "Quay l?i" B??c 1
- Click "Ho�n t?t" ? Submit form

### 4. X? l� Submit
Backend th?c hi?n 2 actions:
1. **INSERT v�o UserProfiles**:
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

2. **INSERT v�o UserInterests**:
```csharp
var userInterests = model.SelectedCategoryIds.Select(categoryId => new UserInterest
{
    UserId = userId,
    CategoryId = categoryId,
    CreatedAt = DateTime.UtcNow
}).ToList();
_context.UserInterests.AddRange(userInterests);
```

3. **SaveChanges** v� redirect v? Home

## UI/UX Features

### Progress Indicator
- 2 steps v?i visual progress bar
- Active step c� m�u gradient purple
- Completed line khi ??n step 2

### Form Validation
- Client-side: Check �t nh?t 1 category ???c ch?n
- Server-side: Validate required fields
- Error messages hi?n th? d??i form

### Interactive Elements
- Category cards c� hover effect
- Selected categories c� checkmark
- Submit button hi?n th? s? l??ng categories ?� ch?n
- Smooth transitions gi?a c�c steps

## Test Cases

### Test 1: User m?i ??ng nh?p l?n ??u
1. T?o t�i kho?n m?i
2. ??ng nh?p
3. **K? v?ng**: Redirect ??n /Onboarding
4. ?i?n th�ng tin ? Ch?n categories ? Submit
5. **K? v?ng**: Redirect v? Home
6. Ki?m tra database:
```sql
SELECT * FROM UserProfiles WHERE UserId = [USER_ID];
SELECT * FROM UserInterests WHERE UserId = [USER_ID];
```

### Test 2: User ?� c� profile nh?ng ch?a c� interests
```sql
-- T?o profile tr??c
INSERT INTO UserProfiles (UserId, Locale, TimeZone)
VALUES (1, 'vi-VN', 'SE Asia Standard Time');

-- X�a interests (n?u c�)
DELETE FROM UserInterests WHERE UserId = 1;
```
1. ??ng nh?p
2. **K? v?ng**: V?n redirect ??n /Onboarding
3. C� th? skip Step 1
4. Ch?n interests ? Step 2
5. Submit ? Redirect Home

### Test 3: User ?� ho�n th�nh onboarding
```sql
-- ??m b?o c� c? profile v� interests
SELECT COUNT(*) FROM UserProfiles WHERE UserId = 1; -- = 1
SELECT COUNT(*) FROM UserInterests WHERE UserId = 1; -- >= 1
```
1. ??ng nh?p
2. **K? v?ng**: Redirect th?ng v? Home (kh�ng qua onboarding)

### Test 4: Skip to�n b? onboarding
1. ??ng nh?p l?n ??u
2. Click "B? qua" ? Step 1
3. Click "Quay l?i" (optional test navigation)
4. Click "B? qua" l?i
5. Kh�ng ch?n category n�o ? Click Submit
6. **K? v?ng**: Error "Vui l�ng ch?n �t nh?t m?t ch? ??"
7. Ch?n 1 category ? Submit
8. **K? v?ng**: Success

### Test 5: Validate form fields
```javascript
// Step 1 - Kh�ng b?t bu?c
// User c� th? ?? tr?ng t?t c? v� v?n Next

// Step 2 - B?t bu?c ch?n �t nh?t 1 category
// Th? submit kh�ng ch?n g� ? L?i
```

## Customization

### Thay ??i required fields
Trong `OnboardingViewModel.cs`, th�m `[Required]`:
```csharp
[Required(ErrorMessage = "Vui l�ng nh?p ng�y sinh")]
[DataType(DataType.Date)]
public DateOnly? DoB { get; set; }
```

### Th�m validation ph�a client
Trong `Index.cshtml`, th�m v�o function `nextToStep2()`:
```javascript
$('#nextToStep2').on('click', function() {
    // Validate DoB
    const dob = $('input[name="DoB"]').val();
    if (!dob) {
        showError('Vui l�ng nh?p ng�y sinh');
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
        message = "Vui l�ng ch?n �t nh?t 3 ch? ??" 
    });
}
```

## SQL Queries h?u �ch

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

### Th?ng k� onboarding completion
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

-- X�a profile
DELETE FROM UserProfiles WHERE UserId = @TestUserId;

-- X�a interests
DELETE FROM UserInterests WHERE UserId = @TestUserId;

-- X�a notifications (optional)
DELETE FROM Notifications WHERE UserId = @TestUserId;
```

## Troubleshooting

### L?i: "This localhost page can't be found"
1. Restart ?ng d?ng
2. Test URL: `https://localhost:7158/test-onboarding-route`
3. N?u OK ? Test `https://localhost:7158/Onboarding`

### L?i: Categories kh�ng hi?n th?
```sql
-- Ki?m tra c� categories kh�ng
SELECT * FROM CourseCategories;

-- N?u r?ng, ch?y script:
INSERT INTO CourseCategories (Name, Slug, Description, DisplayOrder, CreatedAt)
VALUES 
('L?p tr�nh', 'lap-trinh', 'H?c l?p tr�nh v� ph�t tri?n ph?n m?m', 1, GETUTCDATE()),
('Thi?t k?', 'thiet-ke', 'Thi?t k? ?? h?a v� UI/UX', 2, GETUTCDATE()),
('Marketing', 'marketing', 'Digital Marketing v� SEO', 3, GETUTCDATE());
```

### L?i: Foreign key constraint
```
The INSERT statement conflicted with the FOREIGN KEY constraint "FK_UserProfiles_User"
```
**Nguy�n nh�n**: UserId kh�ng t?n t?i trong b?ng Users
**Gi?i ph�p**: Ki?m tra user ?� ??ng nh?p ?�ng ch?a

### L?i: Duplicate key
```
Cannot insert duplicate key in object 'dbo.UserProfiles'
```
**Nguy�n nh�n**: User ?� c� profile r?i
**Gi?i ph�p**: X�a profile c? ?? test l?i:
```sql
DELETE FROM UserProfiles WHERE UserId = [USER_ID];
```

## Best Practices

1. **Always validate on server-side**
   - Client validation c� th? bypass
   - Server validation l� b?t bu?c

2. **Use transactions** (?� implement)
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

**H? th?ng onboarding 2 b??c ?� s?n s�ng! ??**
