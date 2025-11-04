# Tính n?ng ?ánh giá (Review) Khóa h?c - H??ng d?n

## ?? T?ng quan

Tính n?ng ?ánh giá cho phép h?c viên ?ã mua khóa h?c ??a ra nh?n xét và ?ánh giá v? khóa h?c. H? th?ng t? ??ng tính toán ?i?m trung bình và t?ng s? ?ánh giá cho m?i khóa h?c.

## ? Tính n?ng chính

### 1. ?ánh giá khóa h?c
- ? Cho ?i?m t? 1-5 sao v?i giao di?n t??ng tác
- ?? Vi?t nh?n xét chi ti?t (tùy ch?n, t?i ?a 1000 ký t?)
- ? Ch? h?c viên ?ã mua khóa h?c m?i ???c ?ánh giá
- ?? M?i h?c viên ch? ???c ?ánh giá 1 l?n cho m?i khóa h?c

### 2. Qu?n lý ?ánh giá
- ?? Ch?nh s?a ?ánh giá c?a mình
- ??? Xóa ?ánh giá c?a mình
- ??? Xem t?t c? ?ánh giá c?a khóa h?c

### 3. Th?ng kê t? ??ng
- ?? T? ??ng c?p nh?t ?i?m trung bình
- ?? Hi?n th? phân b? ?ánh giá (1-5 sao)
- ?? ??m t?ng s? ?ánh giá

## ??? Ki?n trúc

### Database Schema

```sql
CREATE TABLE dbo.CourseReviews (
    ReviewId     INT IDENTITY(1,1) PRIMARY KEY,
    CourseId     INT NOT NULL,
    UserId       INT NOT NULL,
    Rating       DECIMAL(2,1) NOT NULL CHECK (Rating BETWEEN 0 AND 5),
    Comment      NVARCHAR(1000) NULL,
    CreatedAt    DATETIME2(7) NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt    DATETIME2(7) NULL,
    IsApproved   BIT NOT NULL DEFAULT (1),
    FOREIGN KEY (CourseId) REFERENCES dbo.Courses(CourseId),
    FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
);
```

**Trigger t? ??ng c?p nh?t rating:**
```sql
CREATE TRIGGER trg_UpdateCourseRating
ON dbo.CourseReviews
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    UPDATE c
    SET 
        AverageRating = AVG(Rating),
        TotalReviews = COUNT(*)
    FROM dbo.Courses c
    WHERE c.CourseId IN (SELECT CourseId FROM inserted UNION SELECT CourseId FROM deleted);
END;
```

### Backend Structure

```
Services/
??? IServices/
?   ??? IReviewService.cs          # Interface ??nh ngh?a các method
??? ReviewService.cs               # Implementation c?a service

Controllers/
??? ReviewController.cs            # Controller x? lý HTTP requests

Models/
??? Entities/
?   ??? CourseReview.cs           # Entity model
??? ViewModels/
    ??? ReviewViewModels.cs        # View models (Create/Edit)
```

### Frontend Structure

```
Views/
??? Review/
?   ??? Edit.cshtml               # View ch?nh s?a ?ánh giá
??? Course/
    ??? Detail.cshtml             # Hi?n th? và form ?ánh giá trong trang chi ti?t

wwwroot/css/course/
??? course-detail.css             # CSS cho UI ?ánh giá
```

## ?? API Endpoints

### 1. T?o ?ánh giá m?i
```
POST /reviews/create
Content-Type: application/x-www-form-urlencoded

Parameters:
- CourseId (int, required): ID khóa h?c
- Rating (decimal, required): ?i?m ?ánh giá (1-5)
- Comment (string, optional): Nh?n xét (max 1000 chars)
```

### 2. Ch?nh s?a ?ánh giá
```
GET  /reviews/edit/{id}           # Hi?n th? form edit
POST /reviews/edit/{id}           # Submit form edit

Parameters:
- ReviewId (int, required)
- Rating (decimal, required)
- Comment (string, optional)
```

### 3. Xóa ?ánh giá
```
POST /reviews/delete/{id}
```

### 4. L?y th?ng kê rating
```
GET /reviews/rating-stats/{courseId}

Response:
{
  "distribution": { "5": 10, "4": 5, "3": 2, "2": 1, "1": 0 },
  "totalReviews": 18,
  "percentages": { "5": 55.6, "4": 27.8, ... }
}
```

## ?? UI/UX Features

### Star Rating Component
- **Interactive**: Click ?? ch?n rating
- **Visual feedback**: Hover effect
- **Labels**: Hi?n th? text t??ng ?ng (R?t t?, T?, Trung bình, T?t, Xu?t s?c)

### Review Form
- **Collapsible**: ?n/hi?n form ?ánh giá
- **Validation**: Client-side & server-side validation
- **Character counter**: ??m ký t? cho textarea (max 1000)
- **Responsive**: T?i ?u cho mobile

### Review Display
- **User avatar**: Hi?n th? initials trong avatar tròn
- **Timestamp**: Ngày t?o ?ánh giá
- **Edit/Delete buttons**: Ch? hi?n v?i ch? s? h?u
- **Rating distribution**: Bi?u ?? thanh phân b? ?i?m

## ?? Permissions & Security

### Ki?m tra quy?n
1. **T?o ?ánh giá**:
   - Ph?i ??ng nh?p
   - Ph?i mua khóa h?c (Status = "Paid")
   - Ch?a ?ánh giá khóa h?c này tr??c ?ó
   - Không ph?i là ch? khóa h?c

2. **Ch?nh s?a/Xóa**:
   - Ph?i ??ng nh?p
   - Ph?i là ch? s? h?u c?a ?ánh giá

### Validation Rules
- **Rating**: 1-5 sao (decimal), required
- **Comment**: T?i ?a 1000 ký t?, optional
- **Anti-forgery token**: B?t bu?c cho m?i POST request
- **HTML Sanitization**: T? ??ng x? lý n?u c?n

## ?? Cách s? d?ng

### 1. ??ng ký Service (?ã th?c hi?n)
```csharp
// Program.cs
builder.Services.AddScoped<IReviewService, ReviewService>();
```

### 2. Tích h?p vào trang chi ti?t khóa h?c

**Trong Course/Detail.cshtml:**
```razor
@using System.Security.Claims

@{
    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var userId = !string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var uid) ? uid : 0;
    var userReview = userId > 0 ? Model.CourseReviews?.FirstOrDefault(r => r.UserId == userId) : null;
    var hasPurchased = userId > 0 && Model.CoursePurchases?.Any(p => p.BuyerId == userId && p.Status == "Paid") == true;
    var canReview = hasPurchased && userReview == null && !isOwner;
}

<!-- Form hi?n th? t? ??ng d?a trên ?i?u ki?n -->
```

### 3. Inject service vào Controller c?n thi?t
```csharp
public class CourseController : Controller
{
    private readonly IReviewService _reviewService;
    
    public CourseController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }
}
```

## ?? Ví d? s? d?ng trong code

### Ki?m tra quy?n ?ánh giá
```csharp
var canReview = _reviewService.CanUserReview(courseId, userId);
if (canReview)
{
    // Hi?n th? form ?ánh giá
}
```

### T?o ?ánh giá m?i
```csharp
var model = new CreateReviewViewModel
{
    CourseId = 1,
    Rating = 4.5m,
    Comment = "Khóa h?c r?t hay!"
};

var review = _reviewService.CreateReview(model, userId);
```

### L?y th?ng kê
```csharp
var distribution = _reviewService.GetRatingDistribution(courseId);
// Result: { 5: 10, 4: 5, 3: 2, 2: 1, 1: 0 }
```

## ?? Testing Checklist

- [ ] Ch? h?c viên ?ã mua m?i ?ánh giá ???c
- [ ] M?t h?c viên ch? ?ánh giá ???c 1 l?n
- [ ] Ch? khóa h?c không th? ?ánh giá khóa h?c c?a mình
- [ ] Star rating ho?t ??ng m??t mà
- [ ] Character counter hi?n th? chính xác
- [ ] Validation ho?t ??ng ?úng
- [ ] Trigger t? ??ng c?p nh?t AverageRating và TotalReviews
- [ ] Edit/Delete ch? cho phép ch? s? h?u
- [ ] Responsive trên mobile
- [ ] Anti-forgery token ???c validate

## ?? Tính n?ng m? r?ng (Future)

1. **Review moderation**: Admin duy?t ?ánh giá tr??c khi hi?n th? (IsApproved)
2. **Helpful votes**: Ng??i dùng vote ?ánh giá h?u ích
3. **Filter reviews**: L?c theo s? sao, ngày t?o
4. **Sort reviews**: S?p x?p theo m?i nh?t, h?u ích nh?t
5. **Reply to reviews**: Gi?ng viên tr? l?i ?ánh giá
6. **Report reviews**: Báo cáo ?ánh giá spam/không phù h?p
7. **Rich text**: Cho phép format text trong comment
8. **Images**: Upload ?nh kèm theo ?ánh giá

## ?? Related Documentation

- [Course Entity Documentation](./COURSE_ENTITY.md)
- [Purchase Flow Documentation](./PURCHASE_FLOW.md)
- [API Reference](./API_REFERENCE.md)

## ?? Troubleshooting

### Không th?y form ?ánh giá
- Ki?m tra ?ã ??ng nh?p ch?a
- Ki?m tra ?ã mua khóa h?c ch?a (CoursePurchases.Status = "Paid")
- Ki?m tra ?ã ?ánh giá ch?a

### Rating không c?p nh?t
- Ki?m tra trigger `trg_UpdateCourseRating` ?ã t?o ch?a
- Ki?m tra `IsApproved = 1` trong CourseReviews

### L?i 403 Forbidden khi edit/delete
- Ki?m tra anti-forgery token
- Ki?m tra ownership (review.UserId == currentUserId)

## ?? Contributors

- **Backend**: ReviewService, ReviewController, Database Schema
- **Frontend**: Star rating component, Review form UI, Responsive design
- **Testing**: Unit tests, Integration tests

---

**Last updated**: 2024
**Version**: 1.0.0
