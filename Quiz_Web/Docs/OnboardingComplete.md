# ? Ho�n th�nh: Onboarding 2 b??c v?i Profile + Interests

## ?? Nh?ng g� ?� implement

### 1. ViewModel c?p nh?t
- ? `OnboardingViewModel.cs` v?i ??y ?? fields:
  - DoB (DateOnly)
  - Gender (string)
  - Bio (string, max 500 chars)
  - SchoolName (string)
  - GradeLevel (string)
  - Categories (List<CourseCategory>)
  - SelectedCategoryIds (List<int>)

### 2. Controller logic
- ? Check c? `UserProfile` V� `UserInterests`
- ? `OnGet`: Load categories
- ? `OnPost`: 
  - Step A: INSERT UserProfile
  - Step B: INSERT UserInterests (multiple)
  - Step C: SaveChanges + Redirect
- ? Skip function (t?o minimal profile)

### 3. View 2-step flow
- ? Progress indicator (1 ? 2)
- ? **Step 1**: Form thu th?p profile
  - Ng�y sinh (date picker)
  - Gi?i t�nh (dropdown)
  - T�n tr??ng (text)
  - C?p/Tr�nh ?? (text)
  - Gi?i thi?u (textarea)
- ? **Step 2**: Grid categories v?i checkboxes
- ? Navigation: Next, Back, Skip buttons
- ? Form validation (JS + Server)

### 4. Services updated
- ? `IUserService`: Added `HasUserProfile()`
- ? `UserService`: Implemented both checks
- ? `AccountController`: Login logic ki?m tra c? 2

### 5. Styling
- ? Responsive design
- ? Progress bar animation
- ? Form field styling
- ? Category card hover effects
- ? Mobile-friendly

## ?? C�ch s? d?ng

### B??c 1: Chu?n b? Database
```sql
-- Ki?m tra tables
SELECT * FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN ('UserProfiles', 'UserInterests', 'CourseCategories');

-- Th�m categories n?u ch?a c�
INSERT INTO CourseCategories (Name, Slug, Description, DisplayOrder, CreatedAt)
VALUES 
('L?p tr�nh', 'lap-trinh', 'H?c l?p tr�nh v� ph�t tri?n ph?n m?m', 1, GETUTCDATE()),
('Thi?t k?', 'thiet-ke', 'Thi?t k? ?? h?a v� UI/UX', 2, GETUTCDATE()),
('Marketing', 'marketing', 'Digital Marketing v� SEO', 3, GETUTCDATE()),
('Kinh doanh', 'kinh-doanh', 'Qu?n l� v� kh?i nghi?p', 4, GETUTCDATE()),
('Ngo?i ng?', 'ngoai-ngu', 'H?c ti?ng Anh v� ng�n ng? kh�c', 5, GETUTCDATE());
```

### B??c 2: Test Flow
1. **T?o t�i kho?n m?i** ho?c reset onboarding cho user test:
```sql
DELETE FROM UserInterests WHERE UserId = 1;
DELETE FROM UserProfiles WHERE UserId = 1;
```

2. **??ng nh?p**:
   - URL: `https://localhost:7158/login`
   - ?i?n credentials
   - Click "??ng nh?p"

3. **K? v?ng**: T? ??ng redirect ??n `/Onboarding`

4. **Step 1 - Profile**:
   - ?i?n th�ng tin (optional, c� th? skip)
   - Click "Ti?p theo" ho?c "B? qua"

5. **Step 2 - Interests**:
   - Ch?n �t nh?t 1 category
   - Click "Ho�n t?t"

6. **K? v?ng**: 
   - Success message
   - Redirect v? Home
   - Database c� records m?i

7. **Verify Database**:
```sql
SELECT * FROM UserProfiles WHERE UserId = 1;
SELECT 
    ui.UserInterestId,
    c.Name AS CategoryName,
    ui.CreatedAt
FROM UserInterests ui
JOIN CourseCategories c ON ui.CategoryId = c.CategoryId
WHERE ui.UserId = 1;
```

### B??c 3: Test Navigation
- **Next ? Back**: Ki?m tra d? li?u c� gi? kh�ng
- **Skip Step 1**: ?i th?ng Step 2
- **Try submit without categories**: Xem error message
- **Select categories**: Counter tr�n button c?p nh?t

## ?? Files ?� t?o/s?a

### T?o m?i:
- `Docs/OnboardingTwoSteps.md` - H??ng d?n chi ti?t
- `Docs/OnboardingComplete.md` - Summary n�y

### ?� s?a:
1. `Models/ViewModels/OnboardingViewModel.cs` - Added profile fields
2. `Controllers/OnboardingController.cs` - 2-step logic
3. `Views/Onboarding/Index.cshtml` - 2-step UI
4. `wwwroot/css/onboarding.css` - Progress bar + form styles
5. `Services/IServices/IUserService.cs` - Added HasUserProfile
6. `Services/UserService.cs` - Implemented HasUserProfile
7. `Controllers/AccountController.cs` - Check both profile & interests

## ?? Test URLs

| URL | M?c ?�ch |
|-----|----------|
| `/test-onboarding-route` | Verify controller accessible |
| `/debug-onboarding` | Test view rendering (from HomeController) |
| `/Onboarding` | Main onboarding page |
| `/Onboarding/Index` | Alternative URL |

## ?? Database Schema

### UserProfiles
```
UserId (PK, FK to Users)
DoB (date, nullable)
Gender (nvarchar(20), nullable)
Bio (nvarchar(500), nullable)
SchoolName (nvarchar(200), nullable)
GradeLevel (nvarchar(50), nullable)
Locale (nvarchar(10), nullable)
TimeZone (nvarchar(64), nullable)
```

### UserInterests
```
UserInterestId (PK, IDENTITY)
UserId (FK to Users)
CategoryId (FK to CourseCategories)
CreatedAt (datetime2)
UNIQUE (UserId, CategoryId)
```

## ?? UI Features

### Progress Indicator
- Visual step 1/2
- Active state highlighting
- Animated progress line

### Form Fields (Step 1)
- Date picker cho DoB
- Dropdown cho Gender
- Text inputs cho School & Grade
- Textarea cho Bio (max 500 chars)
- Optional fields (user c� th? skip)

### Category Grid (Step 2)
- Responsive grid layout
- Icon + Name + Description
- Checkbox v?i visual feedback
- Checkmark khi selected
- Hover animations
- Counter tr�n submit button

### Buttons
- "Ti?p theo" (Next to Step 2)
- "B? qua" (Skip Step 1)
- "Quay l?i" (Back to Step 1)
- "Ho�n t?t" (Submit form)

## ?? Configuration

### Minimum categories required
Default: 1. ?? thay ??i, edit `OnboardingController.cs`:
```csharp
if (model.SelectedCategoryIds == null || model.SelectedCategoryIds.Count < 3) {
    // Require 3 instead of 1
}
```

### Make profile fields required
Edit `OnboardingViewModel.cs`:
```csharp
[Required(ErrorMessage = "Vui l�ng ch?n ng�y sinh")]
public DateOnly? DoB { get; set; }
```

### Change default locale/timezone
Edit `OnboardingController.cs`:
```csharp
Locale = "en-US",
TimeZone = "Pacific Standard Time"
```

## ?? Known Issues & Solutions

### Issue 1: "Page can't be found"
**Solution**: 
1. Rebuild solution (Ctrl + Shift + B)
2. Restart app (Shift + F5 ? F5)
3. Clear browser cache (Ctrl + Shift + R)

### Issue 2: Categories kh�ng hi?n th?
**Solution**: 
```sql
SELECT COUNT(*) FROM CourseCategories;
-- N?u = 0, ch?y INSERT script ? tr�n
```

### Issue 3: L?i Foreign Key
**Solution**: Ki?m tra UserId h?p l?:
```sql
SELECT UserId FROM Users WHERE UserId = [YOUR_ID];
```

### Issue 4: Duplicate onboarding
**Solution**: Clear existing data:
```sql
DELETE FROM UserProfiles WHERE UserId = [USER_ID];
DELETE FROM UserInterests WHERE UserId = [USER_ID];
```

## ?? Success Metrics

?? ?�nh gi� hi?u qu? onboarding:

```sql
-- Completion rate
SELECT 
    CAST(COUNT(DISTINCT up.UserId) AS FLOAT) / 
    CAST(COUNT(DISTINCT u.UserId) AS FLOAT) * 100 AS CompletionRate
FROM Users u
LEFT JOIN UserProfiles up ON u.UserId = up.UserId;

-- Average interests per user
SELECT AVG(InterestCount) AS AvgInterests
FROM (
    SELECT UserId, COUNT(*) AS InterestCount
    FROM UserInterests
    GROUP BY UserId
) AS Counts;

-- Most popular categories
SELECT 
    c.Name,
    COUNT(ui.UserInterestId) AS SelectionCount
FROM CourseCategories c
LEFT JOIN UserInterests ui ON c.CategoryId = ui.CategoryId
GROUP BY c.CategoryId, c.Name
ORDER BY SelectionCount DESC;
```

## ?? Next Steps

1. **Analytics tracking**
   - Track step completion
   - Measure drop-off rate
   - A/B test different flows

2. **Enhanced validation**
   - Age validation (DoB)
   - School name autocomplete
   - Bio character counter

3. **Progressive disclosure**
   - Show recommended categories based on profile
   - Smart defaults

4. **Social proof**
   - "X ng??i ?� ch?n ch? ?? n�y"
   - Popular categories highlight

5. **Gamification**
   - Progress rewards
   - Achievement badges
   - Welcome bonus

## ? Checklist ho�n th�nh

- [x] ViewModel v?i profile fields
- [x] Controller x? l� 2 steps
- [x] View v?i progress indicator
- [x] Step 1: Profile form
- [x] Step 2: Categories grid
- [x] Navigation (Next, Back, Skip)
- [x] Form validation (client + server)
- [x] Database INSERT logic
- [x] Service methods (HasUserProfile, HasUserInterests)
- [x] Login logic ki?m tra onboarding
- [x] CSS styling ho�n ch?nh
- [x] Responsive design
- [x] Error handling
- [x] Success messaging
- [x] Documentation ??y ??
- [x] Build successful

---

## ?? K?t lu?n

H? th?ng onboarding 2 b??c ?� ho�n thi?n v?i:
- ? Thu th?p th�ng tin profile ??y ??
- ? Ch?n s? th�ch ?? ?? xu?t kh�a h?c
- ? UI/UX hi?n ??i, responsive
- ? Validation ??y ??
- ? Error handling t?t
- ? Documentation chi ti?t

**Restart ?ng d?ng v� test ngay! ??**

C�c b??c test nhanh:
1. Stop app (Shift + F5)
2. Clean + Rebuild (Ctrl + Shift + B)
3. Run (F5)
4. ??ng nh?p v?i user m?i
5. Enjoy onboarding flow! ??
