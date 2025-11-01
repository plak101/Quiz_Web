# H? th?ng Onboarding và Thông báo ?? xu?t Khóa h?c

## T?ng quan

H? th?ng này bao g?m 4 ph?n chính:
1. **Onboarding** - Thu th?p s? thích ng??i dùng l?n ??ng nh?p ??u tiên
2. **Background Service** - T? ??ng t?o thông báo ?? xu?t khóa h?c
3. **Notification API** - API ?? qu?n lý thông báo
4. **Notification Dropdown** - Giao di?n hi?n th? thông báo

## C?u trúc Database

### B?ng UserInterests
```sql
UserInterestId INT IDENTITY(1,1) PRIMARY KEY
UserId INT NOT NULL (FK ? Users)
CategoryId INT NOT NULL (FK ? CourseCategories)
CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
UNIQUE (UserId, CategoryId)
```

B?ng này l?u tr? các ch? ?? mà ng??i dùng quan tâm.

## Lu?ng ho?t ??ng

### 1. Onboarding Flow
1. Ng??i dùng ??ng nh?p l?n ??u
2. H? th?ng ki?m tra `UserInterests` ?? xem ng??i dùng ?ã ch?n s? thích ch?a
3. N?u ch?a ? Chuy?n h??ng ??n `/Onboarding`
4. Ng??i dùng ch?n các ch? ?? quan tâm
5. H? th?ng l?u vào b?ng `UserInterests`
6. Chuy?n h??ng v? trang ch?

### 2. Background Service Flow
Service `CourseRecommendationService` ch?y ??nh k? (m?c ??nh 24 gi?/l?n):

1. L?y danh sách ng??i dùng có s? thích (`UserInterests`)
2. V?i m?i ng??i dùng:
   - L?y top 3 khóa h?c phù h?p d?a trên:
     * CategoryId kh?p v?i s? thích
     * Khóa h?c ?ã publish (`IsPublished = 1`)
     * Lo?i tr? khóa h?c ?ã mua
     * Lo?i tr? khóa h?c ?ã ???c ?? xu?t tr??c ?ó
     * S?p x?p theo `AverageRating` gi?m d?n
   - T?o thông báo cho m?i khóa h?c ?? xu?t
3. L?u thông báo vào b?ng `Notifications`

### 3. Notification API
**Endpoints:**
- `GET /api/notifications` - L?y danh sách thông báo
- `GET /api/notifications/unread-count` - S? thông báo ch?a ??c
- `POST /api/notifications/{id}/mark-read` - ?ánh d?u ?ã ??c
- `POST /api/notifications/mark-all-read` - ?ánh d?u t?t c? ?ã ??c
- `DELETE /api/notifications/{id}` - Xóa thông báo

### 4. Notification Dropdown UI
- Hi?n th? icon chuông v?i badge s? thông báo ch?a ??c
- Click vào icon ? Hi?n th? dropdown v?i danh sách thông báo
- M?i thông báo có:
  * Icon phân lo?i theo type
  * N?i dung thông báo
  * Th?i gian (tính t??ng ??i: "2 gi? tr??c")
  * Tr?ng thái ?ã ??c/ch?a ??c
- Click vào thông báo ? Chuy?n ??n trang chi ti?t khóa h?c

## C?u hình

### Thay ??i t?n su?t ch?y Background Service
Trong `CourseRecommendationService.cs`:

```csharp
_timer = new Timer(
    DoWork,
    null,
    TimeSpan.Zero,  // Th?i gian b?t ??u
    TimeSpan.FromHours(24)  // Chu k? l?p l?i (24 gi?)
);
```

?? test, có th? ??i thành:
```csharp
TimeSpan.FromMinutes(5)  // Ch?y m?i 5 phút
```

### S? l??ng thông báo hi?n th?
Trong `_NotificationDropdown.cshtml`:

```javascript
loadNotifications() {
    $.ajax({
        url: '/api/notifications',
        data: { take: 10 },  // L?y 10 thông báo m?i nh?t
        // ...
    });
}
```

## Files t?o m?i

### 1. Database
- `Quiz_Web\Database\VerifyUserInterests.sql`

### 2. Models
- `Quiz_Web\Models\ViewModels\OnboardingViewModel.cs`

### 3. Controllers
- `Quiz_Web\Controllers\OnboardingController.cs`
- `Quiz_Web\Controllers\API\NotificationsController.cs`

### 4. Services
- `Quiz_Web\Services\CourseRecommendationService.cs`

### 5. Views
- `Quiz_Web\Views\Onboarding\Index.cshtml`
- `Quiz_Web\Views\Shared\_NotificationDropdown.cshtml`

### 6. Styles
- `Quiz_Web\wwwroot\css\onboarding.css`

## Files ?ã s?a ??i

1. `Quiz_Web\Program.cs`
   - ??ng ký `CourseRecommendationService` nh? HostedService

2. `Quiz_Web\Controllers\AccountController.cs`
   - Thêm logic check onboarding sau khi ??ng nh?p

3. `Quiz_Web\Services\IServices\IUserService.cs`
   - Thêm method `HasUserInterests(int userId)`

4. `Quiz_Web\Services\UserService.cs`
   - Implement method `HasUserInterests(int userId)`

5. `Quiz_Web\Views\Shared\_Layout.cshtml`
   - Thêm partial view `_NotificationDropdown`

## Ki?m tra h? th?ng

### 1. Test Onboarding
1. T?o tài kho?n m?i ho?c xóa records trong `UserInterests` cho user test
2. ??ng nh?p
3. Ki?m tra xem có redirect ??n `/Onboarding` không
4. Ch?n m?t s? category và submit
5. Ki?m tra database xem ?ã có records trong `UserInterests`

### 2. Test Background Service
1. Set chu k? ng?n (5 phút) ?? test
2. Ch?y ?ng d?ng
3. Ki?m tra logs trong Output window c?a Visual Studio
4. Sau 5 phút, check b?ng `Notifications` xem có thông báo m?i không

### 3. Test Notification API
Dùng Postman ho?c browser:
```
GET /api/notifications
GET /api/notifications/unread-count
POST /api/notifications/{id}/mark-read
```

### 4. Test Notification Dropdown
1. ??m b?o có thông báo trong database
2. Reload trang
3. Ki?m tra badge s? thông báo trên icon chuông
4. Click vào icon chuông
5. Ki?m tra dropdown hi?n th? ?úng
6. Click vào m?t thông báo
7. Ki?m tra redirect ??n trang khóa h?c
8. Click "?ánh d?u t?t c? ?ã ??c"

## Troubleshooting

### Onboarding không xu?t hi?n
- Ki?m tra `UserInterests` table ?ã t?n t?i ch?a
- Xóa records trong `UserInterests` cho user test
- Check console logs trong browser

### Background Service không ch?y
- Ki?m tra `Program.cs` ?ã register HostedService ch?a
- Check Output window trong Visual Studio
- ??m b?o connection string ?úng

### Notification không hi?n th?
- Ki?m tra API endpoint `/api/notifications` có tr? v? data không
- Check browser console có l?i JavaScript không
- Xác nh?n user ?ã ??ng nh?p

### Badge không c?p nh?t
- Check API `/api/notifications/unread-count`
- Xem có l?i CORS không
- Refresh l?i trang

## M? r?ng t??ng lai

1. **Real-time notifications** v?i SignalR
2. **Email notifications** khi có khóa h?c m?i phù h?p
3. **Push notifications** cho mobile app
4. **AI-based recommendations** thay vì rule-based
5. **A/B testing** cho notification content
6. **Analytics** ?? track click-through rate

## Support

N?u g?p v?n ??, hãy ki?m tra:
1. Database connection
2. Entity Framework migrations
3. Browser console logs
4. Visual Studio Output logs
