# H??ng d?n Test H? th?ng Onboarding và Notification

## 1. Chu?n b? Database

### B??c 1: Ch?y migration ?? t?o b?ng UserInterests (n?u ch?a có)
```bash
# Trong Package Manager Console
Add-Migration AddUserInterests
Update-Database
```

Ho?c ch?y SQL script tr?c ti?p:
```sql
-- Xem file: Database/VerifyUserInterests.sql
```

### B??c 2: Ki?m tra b?ng CourseCategories có d? li?u
```sql
SELECT * FROM CourseCategories ORDER BY DisplayOrder;
```

N?u ch?a có, thêm d? li?u m?u:
```sql
INSERT INTO CourseCategories (Name, Slug, Description, DisplayOrder, CreatedAt)
VALUES 
('L?p trình', 'lap-trinh', 'H?c các ngôn ng? l?p trình', 1, GETUTCDATE()),
('Kinh doanh', 'kinh-doanh', 'K? n?ng qu?n lý và kh?i nghi?p', 2, GETUTCDATE()),
('Thi?t k?', 'thiet-ke', 'Thi?t k? ?? h?a và UI/UX', 3, GETUTCDATE()),
('Marketing', 'marketing', 'Digital Marketing và SEO', 4, GETUTCDATE()),
('Ngo?i ng?', 'ngoai-ngu', 'H?c ti?ng Anh và các ngôn ng? khác', 5, GETUTCDATE());
```

### B??c 3: Thêm m?t vài khóa h?c m?u
```sql
INSERT INTO Courses (OwnerId, Title, Slug, Summary, Price, IsPublished, CategoryId, CreatedAt)
VALUES 
(1, 'L?p trình C# t? c? b?n ??n nâng cao', 'csharp-basic-to-advanced', 
 'Khóa h?c toàn di?n v? C# và .NET', 500000, 1, 1, GETUTCDATE()),
(1, 'Marketing trên Facebook', 'facebook-marketing', 
 'H?c cách qu?ng cáo hi?u qu? trên Facebook', 300000, 1, 4, GETUTCDATE()),
(1, 'Thi?t k? UI/UX v?i Figma', 'figma-ui-ux', 
 'T?o giao di?n ??p v?i Figma', 400000, 1, 3, GETUTCDATE());
```

## 2. Test Flow Onboarding

### Test Case 1: User m?i ??ng nh?p l?n ??u
1. **??ng nh?p** v?i tài kho?n test (ho?c t?o tài kho?n m?i)
   - URL: `https://localhost:7158/login`
   - ?i?n username và password
   - Click "??ng nh?p"

2. **K? v?ng**: T? ??ng redirect ??n `/Onboarding`

3. **Trên trang Onboarding**:
   - Ki?m tra hi?n th? các category
   - Ch?n 2-3 categories b?t k?
   - Click "Ti?p t?c"

4. **K? v?ng**: 
   - Redirect v? trang Home `/`
   - Không b? redirect l?i Onboarding n?a

5. **Ki?m tra Database**:
```sql
SELECT * FROM UserInterests WHERE UserId = [YOUR_USER_ID];
-- Nên th?y 2-3 records
```

### Test Case 2: User ?ã hoàn thành onboarding
1. ??ng nh?p v?i tài kho?n ?ã ch?n interests
2. **K? v?ng**: Không b? redirect ??n Onboarding, vào th?ng trang Home

### Test Case 3: Skip Onboarding
1. Xóa records trong UserInterests cho user test:
```sql
DELETE FROM UserInterests WHERE UserId = [YOUR_USER_ID];
```
2. ??ng nh?p l?i
3. Trên trang Onboarding, click "B? qua"
4. **K? v?ng**: Redirect v? Home (nh?ng không có interests)

## 3. Test Background Service (Course Recommendation)

### Cách 1: ??i service ch?y t? ??ng (24h)
Service s? t? ch?y sau 24 gi?. Ki?m tra logs trong Output window c?a Visual Studio.

### Cách 2: Thay ??i chu k? ?? test ngay
M? file `CourseRecommendationService.cs`, dòng 27, ??i thành:
```csharp
_timer = new Timer(
    DoWork,
    null,
    TimeSpan.FromSeconds(30),  // Ch?y sau 30 giây
    TimeSpan.FromMinutes(5)    // L?p l?i m?i 5 phút
);
```

Sau ?ó:
1. Restart ?ng d?ng
2. ??i 30 giây
3. Ki?m tra Output window ? "Course Recommendation Service is working"
4. Ki?m tra b?ng Notifications:

```sql
SELECT TOP 10 * 
FROM Notifications 
WHERE UserId = [YOUR_USER_ID]
ORDER BY CreatedAt DESC;
```

**K? v?ng**: Th?y 1-3 notifications v?i Type = 'CourseRecommendation'

### Cách 3: Trigger th? công qua code
Thêm m?t test endpoint (ch? ?? dev):

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
    "title": "?? xu?t dành riêng cho b?n!",
    "body": "D?a trên s? thích \"L?p trình\", chúng tôi ngh? b?n s? thích khóa h?c \"C# c? b?n\".",
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
1. ??ng nh?p vào ?ng d?ng
2. Ki?m tra thanh navigation bar
3. **K? v?ng**: Th?y icon chuông v?i badge s? thông báo (n?u có)

### Test các tính n?ng
1. **Click vào icon chuông**
   - K? v?ng: Dropdown hi?n th? ra
   - K? v?ng: Loading indicator xu?t hi?n
   - K? v?ng: Danh sách notification load ???c

2. **Ki?m tra notification item**
   - Notification ch?a ??c có background màu xanh nh?t
   - Có ch?m tròn màu tím bên trái
   - Hi?n th? th?i gian t??ng ??i ("2 gi? tr??c")

3. **Click vào m?t notification**
   - K? v?ng: Chuy?n ??n trang chi ti?t khóa h?c
   - Badge s? gi?m ?i 1

4. **Click "?ánh d?u t?t c? ?ã ??c"**
   - K? v?ng: T?t c? notification chuy?n sang tr?ng thái ?ã ??c
   - Badge bi?n m?t

5. **Click bên ngoài dropdown**
   - K? v?ng: Dropdown ?óng l?i

## 6. Troubleshooting

### L?i: "This localhost page can't be found"
**Gi?i pháp**:
- Ki?m tra ?ng d?ng ?ang ch?y
- Ki?m tra port number (7158 ho?c khác)
- Th? URL: `https://localhost:7158/Onboarding/Index`

### L?i: Không th?y categories
**Gi?i pháp**:
```sql
-- Ki?m tra d? li?u
SELECT * FROM CourseCategories;

-- N?u r?ng, ch?y script insert ? trên
```

### L?i: Background service không ch?y
**Gi?i pháp**:
1. Ki?m tra Output window trong Visual Studio
2. Tìm log: "Course Recommendation Service is starting"
3. N?u không th?y, ki?m tra `Program.cs` ?ã register service ch?a:
```csharp
builder.Services.AddHostedService<CourseRecommendationService>();
```

### L?i: Notification không hi?n th?
**Gi?i pháp**:
1. M? Developer Tools (F12)
2. Vào tab Console, xem có l?i JavaScript không
3. Vào tab Network, ki?m tra request ??n `/api/notifications`
4. Ki?m tra response có d? li?u không

### L?i: Badge không c?p nh?t
**Gi?i pháp**:
1. Hard refresh (Ctrl + Shift + R)
2. Ki?m tra API `/api/notifications/unread-count` có tr? v? ?úng không
3. Xem Console log có l?i không

## 7. SQL Queries h?u ích

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

### ??m s? khóa h?c theo category
```sql
SELECT 
    cc.Name AS CategoryName,
    COUNT(c.CourseId) AS CourseCount
FROM CourseCategories cc
LEFT JOIN Courses c ON cc.CategoryId = c.CategoryId AND c.IsPublished = 1
GROUP BY cc.CategoryId, cc.Name
ORDER BY CourseCount DESC;
```

### Xem khóa h?c ???c ?? xu?t nhi?u nh?t
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

## 8. Checklist hoàn thành

- [ ] Database có b?ng UserInterests
- [ ] Database có d? li?u trong CourseCategories
- [ ] Database có ít nh?t 3 khóa h?c
- [ ] Trang /Onboarding hi?n th? ?úng
- [ ] Có th? ch?n categories và submit
- [ ] Sau onboarding không b? redirect l?i
- [ ] Background service ?ã ch?y (xem log)
- [ ] Có notifications trong database
- [ ] API /api/notifications tr? v? d? li?u
- [ ] Notification dropdown hi?n th? trong navbar
- [ ] Badge hi?n th? s? thông báo ?úng
- [ ] Click notification chuy?n ??n trang course
- [ ] ?ánh d?u ?ã ??c ho?t ??ng
- [ ] Build không có l?i

## 9. Demo Flow hoàn ch?nh

1. **T?o tài kho?n m?i** ? ??ng nh?p
2. **Onboarding**: Ch?n "L?p trình" và "Thi?t k?"
3. **??i background service ch?y** (ho?c trigger th? công)
4. **Ki?m tra notification dropdown**: Th?y ?? xu?t khóa h?c C# và Figma
5. **Click vào notification khóa h?c C#**: Chuy?n ??n trang chi ti?t
6. **Quay l?i trang ch?**: Badge gi?m ?i 1
7. **Click "?ánh d?u t?t c? ?ã ??c"**: Badge = 0

---

**Chúc b?n test thành công! ??**
