# H??ng d?n Test H? th?ng Onboarding v� Notification

## 1. Chu?n b? Database

### B??c 1: Ch?y migration ?? t?o b?ng UserInterests (n?u ch?a c�)
```bash
# Trong Package Manager Console
Add-Migration AddUserInterests
Update-Database
```

Ho?c ch?y SQL script tr?c ti?p:
```sql
-- Xem file: Database/VerifyUserInterests.sql
```

### B??c 2: Ki?m tra b?ng CourseCategories c� d? li?u
```sql
SELECT * FROM CourseCategories ORDER BY DisplayOrder;
```

N?u ch?a c�, th�m d? li?u m?u:
```sql
INSERT INTO CourseCategories (Name, Slug, Description, DisplayOrder, CreatedAt)
VALUES 
('L?p tr�nh', 'lap-trinh', 'H?c c�c ng�n ng? l?p tr�nh', 1, GETUTCDATE()),
('Kinh doanh', 'kinh-doanh', 'K? n?ng qu?n l� v� kh?i nghi?p', 2, GETUTCDATE()),
('Thi?t k?', 'thiet-ke', 'Thi?t k? ?? h?a v� UI/UX', 3, GETUTCDATE()),
('Marketing', 'marketing', 'Digital Marketing v� SEO', 4, GETUTCDATE()),
('Ngo?i ng?', 'ngoai-ngu', 'H?c ti?ng Anh v� c�c ng�n ng? kh�c', 5, GETUTCDATE());
```

### B??c 3: Th�m m?t v�i kh�a h?c m?u
```sql
INSERT INTO Courses (OwnerId, Title, Slug, Summary, Price, IsPublished, CategoryId, CreatedAt)
VALUES 
(1, 'L?p tr�nh C# t? c? b?n ??n n�ng cao', 'csharp-basic-to-advanced', 
 'Kh�a h?c to�n di?n v? C# v� .NET', 500000, 1, 1, GETUTCDATE()),
(1, 'Marketing tr�n Facebook', 'facebook-marketing', 
 'H?c c�ch qu?ng c�o hi?u qu? tr�n Facebook', 300000, 1, 4, GETUTCDATE()),
(1, 'Thi?t k? UI/UX v?i Figma', 'figma-ui-ux', 
 'T?o giao di?n ??p v?i Figma', 400000, 1, 3, GETUTCDATE());
```

## 2. Test Flow Onboarding

### Test Case 1: User m?i ??ng nh?p l?n ??u
1. **??ng nh?p** v?i t�i kho?n test (ho?c t?o t�i kho?n m?i)
   - URL: `https://localhost:7158/login`
   - ?i?n username v� password
   - Click "??ng nh?p"

2. **K? v?ng**: T? ??ng redirect ??n `/Onboarding`

3. **Tr�n trang Onboarding**:
   - Ki?m tra hi?n th? c�c category
   - Ch?n 2-3 categories b?t k?
   - Click "Ti?p t?c"

4. **K? v?ng**: 
   - Redirect v? trang Home `/`
   - Kh�ng b? redirect l?i Onboarding n?a

5. **Ki?m tra Database**:
```sql
SELECT * FROM UserInterests WHERE UserId = [YOUR_USER_ID];
-- N�n th?y 2-3 records
```

### Test Case 2: User ?� ho�n th�nh onboarding
1. ??ng nh?p v?i t�i kho?n ?� ch?n interests
2. **K? v?ng**: Kh�ng b? redirect ??n Onboarding, v�o th?ng trang Home

### Test Case 3: Skip Onboarding
1. X�a records trong UserInterests cho user test:
```sql
DELETE FROM UserInterests WHERE UserId = [YOUR_USER_ID];
```
2. ??ng nh?p l?i
3. Tr�n trang Onboarding, click "B? qua"
4. **K? v?ng**: Redirect v? Home (nh?ng kh�ng c� interests)

## 3. Test Background Service (Course Recommendation)

### C�ch 1: ??i service ch?y t? ??ng (24h)
Service s? t? ch?y sau 24 gi?. Ki?m tra logs trong Output window c?a Visual Studio.

### C�ch 2: Thay ??i chu k? ?? test ngay
M? file `CourseRecommendationService.cs`, d�ng 27, ??i th�nh:
```csharp
_timer = new Timer(
    DoWork,
    null,
    TimeSpan.FromSeconds(30),  // Ch?y sau 30 gi�y
    TimeSpan.FromMinutes(5)    // L?p l?i m?i 5 ph�t
);
```

Sau ?�:
1. Restart ?ng d?ng
2. ??i 30 gi�y
3. Ki?m tra Output window ? "Course Recommendation Service is working"
4. Ki?m tra b?ng Notifications:

```sql
SELECT TOP 10 * 
FROM Notifications 
WHERE UserId = [YOUR_USER_ID]
ORDER BY CreatedAt DESC;
```

**K? v?ng**: Th?y 1-3 notifications v?i Type = 'CourseRecommendation'

### C�ch 3: Trigger th? c�ng qua code
Th�m m?t test endpoint (ch? ?? dev):

T?o file `Controllers/TestController.cs`:
```csharp
#if DEBUG
using Microsoft.AspNetCore.Mvc;
using Quiz_Web.Services;

namespace Quiz_Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly CourseRecommendationService _service;

        public TestController(IServiceProvider serviceProvider)
        {
            // Note: This is for testing only
        }

        [HttpPost("trigger-recommendations")]
        public IActionResult TriggerRecommendations()
        {
            // Manually trigger the recommendation service
            // Implementation needed
            return Ok(new { message = "Triggered" });
        }
    }
}
#endif
```

## 4. Test Notification API

S? d?ng browser ho?c Postman:

### Get all notifications
```
GET https://localhost:7158/api/notifications
Authorization: Cookie (??ng nh?p tr??c)
```

**Expected Response**:
```json
[
  {
    "notificationId": 1,
    "type": "CourseRecommendation",
    "title": "?? xu?t d�nh ri�ng cho b?n!",
    "body": "D?a tr�n s? th�ch \"L?p tr�nh\", ch�ng t�i ngh? b?n s? th�ch kh�a h?c \"C# c? b?n\".",
    "data": "{\"CourseId\":1}",
    "isRead": false,
    "createdAt": "2025-01-30T10:00:00Z",
    "timeAgo": "2 gi? tr??c"
  }
]
```

### Get unread count
```
GET https://localhost:7158/api/notifications/unread-count
```

**Expected Response**:
```json
{
  "count": 3
}
```

### Mark as read
```
POST https://localhost:7158/api/notifications/1/mark-read
```

**Expected Response**:
```json
{
  "success": true
}
```

### Mark all as read
```
POST https://localhost:7158/api/notifications/mark-all-read
```

**Expected Response**:
```json
{
  "success": true,
  "count": 3
}
```

## 5. Test Notification Dropdown UI

### Test trong browser
1. ??ng nh?p v�o ?ng d?ng
2. Ki?m tra thanh navigation bar
3. **K? v?ng**: Th?y icon chu�ng v?i badge s? th�ng b�o (n?u c�)

### Test c�c t�nh n?ng
1. **Click v�o icon chu�ng**
   - K? v?ng: Dropdown hi?n th? ra
   - K? v?ng: Loading indicator xu?t hi?n
   - K? v?ng: Danh s�ch notification load ???c

2. **Ki?m tra notification item**
   - Notification ch?a ??c c� background m�u xanh nh?t
   - C� ch?m tr�n m�u t�m b�n tr�i
   - Hi?n th? th?i gian t??ng ??i ("2 gi? tr??c")

3. **Click v�o m?t notification**
   - K? v?ng: Chuy?n ??n trang chi ti?t kh�a h?c
   - Badge s? gi?m ?i 1

4. **Click "?�nh d?u t?t c? ?� ??c"**
   - K? v?ng: T?t c? notification chuy?n sang tr?ng th�i ?� ??c
   - Badge bi?n m?t

5. **Click b�n ngo�i dropdown**
   - K? v?ng: Dropdown ?�ng l?i

## 6. Troubleshooting

### L?i: "This localhost page can't be found"
**Gi?i ph�p**:
- Ki?m tra ?ng d?ng ?ang ch?y
- Ki?m tra port number (7158 ho?c kh�c)
- Th? URL: `https://localhost:7158/Onboarding/Index`

### L?i: Kh�ng th?y categories
**Gi?i ph�p**:
```sql
-- Ki?m tra d? li?u
SELECT * FROM CourseCategories;

-- N?u r?ng, ch?y script insert ? tr�n
```

### L?i: Background service kh�ng ch?y
**Gi?i ph�p**:
1. Ki?m tra Output window trong Visual Studio
2. T�m log: "Course Recommendation Service is starting"
3. N?u kh�ng th?y, ki?m tra `Program.cs` ?� register service ch?a:
```csharp
builder.Services.AddHostedService<CourseRecommendationService>();
```

### L?i: Notification kh�ng hi?n th?
**Gi?i ph�p**:
1. M? Developer Tools (F12)
2. V�o tab Console, xem c� l?i JavaScript kh�ng
3. V�o tab Network, ki?m tra request ??n `/api/notifications`
4. Ki?m tra response c� d? li?u kh�ng

### L?i: Badge kh�ng c?p nh?t
**Gi?i ph�p**:
1. Hard refresh (Ctrl + Shift + R)
2. Ki?m tra API `/api/notifications/unread-count` c� tr? v? ?�ng kh�ng
3. Xem Console log c� l?i kh�ng

## 7. SQL Queries h?u �ch

### Xem t?t c? interests c?a m?t user
```sql
SELECT 
    u.Username,
    c.Name AS CategoryName
FROM UserInterests ui
JOIN Users u ON ui.UserId = u.UserId
JOIN CourseCategories c ON ui.CategoryId = c.CategoryId
WHERE u.UserId = 1;
```

### Xem t?t c? notifications c?a m?t user
```sql
SELECT 
    n.NotificationId,
    n.Type,
    n.Title,
    n.Body,
    n.IsRead,
    n.CreatedAt
FROM Notifications n
WHERE n.UserId = 1
ORDER BY n.CreatedAt DESC;
```

### ??m s? kh�a h?c theo category
```sql
SELECT 
    cc.Name AS CategoryName,
    COUNT(c.CourseId) AS CourseCount
FROM CourseCategories cc
LEFT JOIN Courses c ON cc.CategoryId = c.CategoryId AND c.IsPublished = 1
GROUP BY cc.CategoryId, cc.Name
ORDER BY CourseCount DESC;
```

### Xem kh�a h?c ???c ?? xu?t nhi?u nh?t
```sql
SELECT 
    c.Title,
    COUNT(n.NotificationId) AS RecommendationCount
FROM Notifications n
JOIN Courses c ON n.Data LIKE '%' + CAST(c.CourseId AS VARCHAR) + '%'
WHERE n.Type = 'CourseRecommendation'
GROUP BY c.CourseId, c.Title
ORDER BY RecommendationCount DESC;
```

## 8. Checklist ho�n th�nh

- [ ] Database c� b?ng UserInterests
- [ ] Database c� d? li?u trong CourseCategories
- [ ] Database c� �t nh?t 3 kh�a h?c
- [ ] Trang /Onboarding hi?n th? ?�ng
- [ ] C� th? ch?n categories v� submit
- [ ] Sau onboarding kh�ng b? redirect l?i
- [ ] Background service ?� ch?y (xem log)
- [ ] C� notifications trong database
- [ ] API /api/notifications tr? v? d? li?u
- [ ] Notification dropdown hi?n th? trong navbar
- [ ] Badge hi?n th? s? th�ng b�o ?�ng
- [ ] Click notification chuy?n ??n trang course
- [ ] ?�nh d?u ?� ??c ho?t ??ng
- [ ] Build kh�ng c� l?i

## 9. Demo Flow ho�n ch?nh

1. **T?o t�i kho?n m?i** ? ??ng nh?p
2. **Onboarding**: Ch?n "L?p tr�nh" v� "Thi?t k?"
3. **??i background service ch?y** (ho?c trigger th? c�ng)
4. **Ki?m tra notification dropdown**: Th?y ?? xu?t kh�a h?c C# v� Figma
5. **Click v�o notification kh�a h?c C#**: Chuy?n ??n trang chi ti?t
6. **Quay l?i trang ch?**: Badge gi?m ?i 1
7. **Click "?�nh d?u t?t c? ?� ??c"**: Badge = 0

---

**Ch�c b?n test th�nh c�ng! ??**
