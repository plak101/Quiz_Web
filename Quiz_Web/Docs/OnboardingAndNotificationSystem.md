# H? th?ng Onboarding v� Th�ng b�o ?? xu?t Kh�a h?c

## T?ng quan

H? th?ng n�y bao g?m 4 ph?n ch�nh:
1. **Onboarding** - Thu th?p s? th�ch ng??i d�ng l?n ??ng nh?p ??u ti�n
2. **Background Service** - T? ??ng t?o th�ng b�o ?? xu?t kh�a h?c
3. **Notification API** - API ?? qu?n l� th�ng b�o
4. **Notification Dropdown** - Giao di?n hi?n th? th�ng b�o

## C?u tr�c Database

### B?ng UserInterests
```sql
UserInterestId INT IDENTITY(1,1) PRIMARY KEY
UserId INT NOT NULL (FK ? Users)
CategoryId INT NOT NULL (FK ? CourseCategories)
CreatedAt DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME()
UNIQUE (UserId, CategoryId)
```

B?ng n�y l?u tr? c�c ch? ?? m� ng??i d�ng quan t�m.

## Lu?ng ho?t ??ng

### 1. Onboarding Flow
1. Ng??i d�ng ??ng nh?p l?n ??u
2. H? th?ng ki?m tra `UserInterests` ?? xem ng??i d�ng ?� ch?n s? th�ch ch?a
3. N?u ch?a ? Chuy?n h??ng ??n `/Onboarding`
4. Ng??i d�ng ch?n c�c ch? ?? quan t�m
5. H? th?ng l?u v�o b?ng `UserInterests`
6. Chuy?n h??ng v? trang ch?

### 2. Background Service Flow
Service `CourseRecommendationService` ch?y ??nh k? (m?c ??nh 24 gi?/l?n):

1. L?y danh s�ch ng??i d�ng c� s? th�ch (`UserInterests`)
2. V?i m?i ng??i d�ng:
   - L?y top 3 kh�a h?c ph� h?p d?a tr�n:
     * CategoryId kh?p v?i s? th�ch
     * Kh�a h?c ?� publish (`IsPublished = 1`)
     * Lo?i tr? kh�a h?c ?� mua
     * Lo?i tr? kh�a h?c ?� ???c ?? xu?t tr??c ?�
     * S?p x?p theo `AverageRating` gi?m d?n
   - T?o th�ng b�o cho m?i kh�a h?c ?? xu?t
3. L?u th�ng b�o v�o b?ng `Notifications`

### 3. Notification API
**Endpoints:**
- `GET /api/notifications` - L?y danh s�ch th�ng b�o
- `GET /api/notifications/unread-count` - S? th�ng b�o ch?a ??c
- `POST /api/notifications/{id}/mark-read` - ?�nh d?u ?� ??c
- `POST /api/notifications/mark-all-read` - ?�nh d?u t?t c? ?� ??c
- `DELETE /api/notifications/{id}` - X�a th�ng b�o

### 4. Notification Dropdown UI
- Hi?n th? icon chu�ng v?i badge s? th�ng b�o ch?a ??c
- Click v�o icon ? Hi?n th? dropdown v?i danh s�ch th�ng b�o
- M?i th�ng b�o c�:
  * Icon ph�n lo?i theo type
  * N?i dung th�ng b�o
  * Th?i gian (t�nh t??ng ??i: "2 gi? tr??c")
  * Tr?ng th�i ?� ??c/ch?a ??c
- Click v�o th�ng b�o ? Chuy?n ??n trang chi ti?t kh�a h?c

## C?u h�nh

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

?? test, c� th? ??i th�nh:
```csharp
TimeSpan.FromMinutes(5)  // Ch?y m?i 5 ph�t
```

### S? l??ng th�ng b�o hi?n th?
Trong `_NotificationDropdown.cshtml`:

```javascript
loadNotifications() {
    $.ajax({
        url: '/api/notifications',
        data: { take: 10 },  // L?y 10 th�ng b�o m?i nh?t
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

## Files ?� s?a ??i

1. `Quiz_Web\Program.cs`
   - ??ng k� `CourseRecommendationService` nh? HostedService

2. `Quiz_Web\Controllers\AccountController.cs`
   - Th�m logic check onboarding sau khi ??ng nh?p

3. `Quiz_Web\Services\IServices\IUserService.cs`
   - Th�m method `HasUserInterests(int userId)`

4. `Quiz_Web\Services\UserService.cs`
   - Implement method `HasUserInterests(int userId)`

5. `Quiz_Web\Views\Shared\_Layout.cshtml`
   - Th�m partial view `_NotificationDropdown`

## Ki?m tra h? th?ng

### 1. Test Onboarding
1. T?o t�i kho?n m?i ho?c x�a records trong `UserInterests` cho user test
2. ??ng nh?p
3. Ki?m tra xem c� redirect ??n `/Onboarding` kh�ng
4. Ch?n m?t s? category v� submit
5. Ki?m tra database xem ?� c� records trong `UserInterests`

### 2. Test Background Service
1. Set chu k? ng?n (5 ph�t) ?? test
2. Ch?y ?ng d?ng
3. Ki?m tra logs trong Output window c?a Visual Studio
4. Sau 5 ph�t, check b?ng `Notifications` xem c� th�ng b�o m?i kh�ng

### 3. Test Notification API
D�ng Postman ho?c browser:
```
GET /api/notifications
GET /api/notifications/unread-count
POST /api/notifications/{id}/mark-read
```

### 4. Test Notification Dropdown
1. ??m b?o c� th�ng b�o trong database
2. Reload trang
3. Ki?m tra badge s? th�ng b�o tr�n icon chu�ng
4. Click v�o icon chu�ng
5. Ki?m tra dropdown hi?n th? ?�ng
6. Click v�o m?t th�ng b�o
7. Ki?m tra redirect ??n trang kh�a h?c
8. Click "?�nh d?u t?t c? ?� ??c"

## Troubleshooting

### Onboarding kh�ng xu?t hi?n
- Ki?m tra `UserInterests` table ?� t?n t?i ch?a
- X�a records trong `UserInterests` cho user test
- Check console logs trong browser

### Background Service kh�ng ch?y
- Ki?m tra `Program.cs` ?� register HostedService ch?a
- Check Output window trong Visual Studio
- ??m b?o connection string ?�ng

### Notification kh�ng hi?n th?
- Ki?m tra API endpoint `/api/notifications` c� tr? v? data kh�ng
- Check browser console c� l?i JavaScript kh�ng
- X�c nh?n user ?� ??ng nh?p

### Badge kh�ng c?p nh?t
- Check API `/api/notifications/unread-count`
- Xem c� l?i CORS kh�ng
- Refresh l?i trang

## M? r?ng t??ng lai

1. **Real-time notifications** v?i SignalR
2. **Email notifications** khi c� kh�a h?c m?i ph� h?p
3. **Push notifications** cho mobile app
4. **AI-based recommendations** thay v� rule-based
5. **A/B testing** cho notification content
6. **Analytics** ?? track click-through rate

## Support

N?u g?p v?n ??, h�y ki?m tra:
1. Database connection
2. Entity Framework migrations
3. Browser console logs
4. Visual Studio Output logs
