# ?? T�m t?t: H? th?ng Onboarding & Notification ?� ho�n th�nh

## ? C�c t�nh n?ng ?� implement

### 1. Onboarding System (Thu th?p s? th�ch ng??i d�ng)
- ? Trang onboarding v?i UI ??p (gradient background)
- ? Hi?n th? danh s�ch categories d?ng grid cards
- ? Cho ph�p ch?n nhi?u categories
- ? Validation: ph?i ch?n �t nh?t 1 category
- ? L?u v�o database (b?ng UserInterests)
- ? Redirect v? Home sau khi ho�n th�nh
- ? Ch?n onboarding l?p l?i (check ?� c� interests ch?a)

### 2. Background Service (T? ??ng ?? xu?t kh�a h?c)
- ? Service ch?y ??nh k? 24h (c� th? customize)
- ? Logic ?? xu?t th�ng minh:
  - D?a tr�n interests c?a user
  - Lo?i tr? kh�a h?c ?� mua
  - Lo?i tr? kh�a h?c ?� ?? xu?t
  - ?u ti�n kh�a h?c c� rating cao
- ? T?o t?i ?a 3 notifications/user/ng�y
- ? Logging ??y ??

### 3. Notification API (RESTful API)
- ? GET `/api/notifications` - L?y danh s�ch th�ng b�o
- ? GET `/api/notifications/unread-count` - S? th�ng b�o ch?a ??c
- ? POST `/api/notifications/{id}/mark-read` - ?�nh d?u ?� ??c
- ? POST `/api/notifications/mark-all-read` - ?�nh d?u t?t c? ?� ??c
- ? DELETE `/api/notifications/{id}` - X�a th�ng b�o
- ? Time-relative formatting ("2 gi? tr??c")

### 4. Notification Dropdown UI
- ? Icon chu�ng v?i badge ??
- ? Dropdown panel ??p (inspired by Udemy)
- ? Loading state
- ? Empty state
- ? Notification items v?i icon ph�n lo?i
- ? Unread indicator (ch?m tr�n t�m)
- ? Hover effects
- ? Click ?? navigate ??n course detail
- ? Auto-refresh m?i 1 ph�t
- ? Responsive design

## ?? Files ?� t?o m?i

### Database
- `Database/VerifyUserInterests.sql` - Script t?o/verify b?ng UserInterests

### Models
- `Models/ViewModels/OnboardingViewModel.cs` - ViewModel cho onboarding

### Controllers
- `Controllers/OnboardingController.cs` - Controller x? l� onboarding flow
- `Controllers/API/NotificationsController.cs` - REST API cho notifications

### Views
- `Views/Onboarding/Index.cshtml` - Trang onboarding UI
- `Views/Shared/_NotificationDropdown.cshtml` - Component dropdown

### Styles
- `wwwroot/css/onboarding.css` - Styles cho onboarding page
- `wwwroot/css/notification-dropdown.css` - Styles cho notification dropdown

### Services
- `Services/CourseRecommendationService.cs` - Background service

### Documentation
- `Docs/OnboardingAndNotificationSystem.md` - T�i li?u h? th?ng ??y ??
- `Docs/TestingGuide.md` - H??ng d?n test chi ti?t
- `Docs/Troubleshoot404.md` - Gi?i quy?t l?i 404

## ?? Files ?� ch?nh s?a

1. **Program.cs**
   - ??ng k� `CourseRecommendationService` nh? HostedService

2. **AccountController.cs**
   - Th�m check onboarding sau login
   - Redirect ??n /Onboarding n?u ch?a c� interests

3. **IUserService.cs + UserService.cs**
   - Th�m method `HasUserInterests(int userId)`

4. **_Layout.cshtml**
   - Replace static notification icon v?i dynamic dropdown

## ?? C�ch s? d?ng

### B??c 1: ??m b?o Database ?� s?n s�ng
```sql
-- Ch?y script n�y
USE LearningPlatform;

-- Ki?m tra b?ng UserInterests
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserInterests';

-- Ki?m tra c� categories
SELECT * FROM CourseCategories;

-- Ki?m tra c� courses
SELECT * FROM Courses WHERE IsPublished = 1;
```

### B??c 2: Ch?y ?ng d?ng
```bash
# Trong Visual Studio
1. Clean Solution (Build ? Clean Solution)
2. Rebuild (Ctrl + Shift + B)
3. Run (F5)
```

### B??c 3: Test routing
Truy c?p URL test:
```
https://localhost:7158/test-onboarding-route
```
K? v?ng th?y: "Onboarding Controller is accessible! ?"

### B??c 4: Test onboarding flow
```
1. ??ng nh?p v?i t�i kho?n m?i (ho?c x�a UserInterests cho user test)
2. T? ??ng redirect ??n /Onboarding
3. Ch?n categories ? Click "Ti?p t?c"
4. Redirect v? Home
```

### B??c 5: Test notification system
```
1. ??i 24h ho?c thay ??i timer trong CourseRecommendationService.cs
2. Ki?m tra b?ng Notifications c� data kh�ng
3. Reload trang ? Xem notification dropdown
4. Click v�o notification ? Navigate ??n course detail
```

## ?? Customize

### Thay ??i chu k? ch?y Background Service
File: `Services/CourseRecommendationService.cs`, d�ng ~27:
```csharp
_timer = new Timer(
    DoWork,
    null,
    TimeSpan.Zero,           // B?t ??u ngay
    TimeSpan.FromHours(24)   // L?p l?i m?i 24h
);
```

?? test, ??i th�nh:
```csharp
TimeSpan.FromMinutes(5)  // L?p l?i m?i 5 ph�t
```

### Thay ??i s? l??ng courses ???c ?? xu?t
File: `Services/CourseRecommendationService.cs`, d�ng ~70:
```csharp
.Take(3)  // Thay s? n�y
```

### Thay ??i s? notifications hi?n th? trong dropdown
File: `Views/Shared/_NotificationDropdown.cshtml`, d�ng ~115:
```javascript
data: { take: 10 }  // Thay s? n�y
```

## ?? Database Schema ?� s? d?ng

### B?ng m?i
- **UserInterests** (UserInterestId, UserId, CategoryId, CreatedAt)

### B?ng hi?n c� ???c s? d?ng
- **Users**
- **CourseCategories**
- **Courses**
- **CoursePurchases**
- **Notifications**

## ?? Known Issues & Solutions

### Issue 1: L?i 404 khi truy c?p /Onboarding
**Solution**: 
- Restart ?ng d?ng
- Th? URL: `/test-onboarding-route` ?? verify controller accessible
- Ki?m tra file `Index.cshtml` t?n t?i trong `Views/Onboarding/`

### Issue 2: Notification kh�ng load
**Solution**:
- Ki?m tra browser console c� l?i JavaScript kh�ng
- Verify API `/api/notifications` tr? v? data
- Ki?m tra user ?� ??ng nh?p ch?a

### Issue 3: Background Service kh�ng ch?y
**Solution**:
- Ki?m tra Output window (View ? Output ? ASP.NET Core Web Server)
- T�m log: "Course Recommendation Service is starting"
- Verify `Program.cs` ?� register service

### Issue 4: Badge kh�ng c?p nh?t
**Solution**:
- Hard refresh (Ctrl + Shift + R)
- Clear browser cache
- Ki?m tra API `/api/notifications/unread-count`

## ?? Metrics & Monitoring

### Logs quan tr?ng
```
// Kh?i ??ng service
[Information] Course Recommendation Service is starting

// Service ?ang ch?y
[Information] Course Recommendation Service is working
[Information] Processing recommendations for X users

// T?o ?? xu?t
[Information] Generated Y recommendations for user Z

// User ho�n th�nh onboarding
[Information] User X completed onboarding with Y interests
```

### SQL Queries ?? monitor
```sql
-- S? user ?� onboarding
SELECT COUNT(DISTINCT UserId) FROM UserInterests;

-- S? notification ch?a ??c
SELECT UserId, COUNT(*) 
FROM Notifications 
WHERE IsRead = 0 
GROUP BY UserId;

-- Top categories ???c quan t�m
SELECT c.Name, COUNT(ui.UserInterestId) AS InterestCount
FROM CourseCategories c
JOIN UserInterests ui ON c.CategoryId = ui.CategoryId
GROUP BY c.CategoryId, c.Name
ORDER BY InterestCount DESC;
```

## ?? Next Steps (T�nh n?ng t??ng lai)

1. **Real-time notifications** v?i SignalR
2. **Email notifications** khi c� kh�a h?c m?i
3. **Push notifications** cho mobile app
4. **A/B testing** cho notification content
5. **ML-based recommendations** thay v� rule-based
6. **Notification preferences** (cho ph�p user t?t/b?t lo?i notification)
7. **Rich notifications** v?i h�nh ?nh v� action buttons
8. **Notification history page** (/notifications/all)

## ?? T�i li?u tham kh?o

- [OnboardingAndNotificationSystem.md](./OnboardingAndNotificationSystem.md) - T�i li?u chi ti?t
- [TestingGuide.md](./TestingGuide.md) - H??ng d?n test t?ng b??c
- [Troubleshoot404.md](./Troubleshoot404.md) - Gi?i quy?t l?i 404

## ? Demo Screenshots (Expected)

### 1. Onboarding Page
- Gradient background (purple ? violet)
- Grid layout v?i category cards
- Each card c� checkbox + icon + name + description
- Selected cards c� checkmark v� highlight
- Buttons: "B? qua" v� "Ti?p t?c"

### 2. Notification Dropdown
- Bell icon v?i red badge
- White dropdown panel
- Header: "Th�ng b�o" + Mark all read button
- Notification items v?i:
  * Purple gradient icon
  * Course recommendation text
  * Time ago ("2 gi? tr??c")
  * Unread indicator (purple dot)
- Footer: "Xem t?t c? th�ng b�o"

## ?? Status

### ? Completed
- Database schema
- Backend logic (Controllers, Services)
- Frontend UI (Views, Components)
- API endpoints
- Background service
- Documentation
- Build successful

### ?? Needs Testing
- Full onboarding flow
- Notification generation
- API endpoints
- UI interactions
- Edge cases

### ?? Ready for Production
After thorough testing, the system is ready for deployment with:
- Proper error handling
- Logging
- Security (Authorization)
- Responsive design
- Performance optimization (async/await)

---

**H? th?ng ?� s?n s�ng! H�y test v� enjoy! ??**
