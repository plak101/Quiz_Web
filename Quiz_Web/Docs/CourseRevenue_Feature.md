# Tính n?ng Th?ng kê Doanh thu - Revenue Analytics

## T?ng quan
Tính n?ng th?ng kê doanh thu cho phép ng??i dùng (gi?ng viên/instructor) xem chi ti?t doanh thu t? các khóa h?c mà h? ?ã t?o.

## Công th?c tính toán
- **T?ng doanh thu (Gross Revenue)**: `Giá khóa h?c × S? l??t mua`
- **Thu nh?p c?a gi?ng viên (Instructor Revenue)**: `T?ng doanh thu × 60%`
- **Phí n?n t?ng (Platform Fee)**: `T?ng doanh thu × 40%`

## Tính n?ng

### 1. Nút truy c?p
- Thêm nút **"Doanh thu"** màu xanh lá cây bên c?nh nút "T?o khóa h?c" trong trang `/courses/mine`
- Icon: `fa-chart-line`

### 2. Trang Th?ng kê (`/courses/revenue`)
Hi?n th?:

#### A. Cards T?ng quan (4 cards)
1. **T?ng s? l??t mua** - T?ng s? h?c viên ?ã mua khóa h?c
2. **T?ng doanh thu** - T?ng ti?n thu ???c t? t?t c? khóa h?c
3. **Thu nh?p c?a b?n (60%)** - S? ti?n gi?ng viên nh?n ???c
4. **Phí n?n t?ng (40%)** - S? ti?n n?n t?ng gi? l?i

#### B. Bi?u ?? C?t (Bar Chart)
- Hi?n th? **top 10** khóa h?c có doanh thu cao nh?t
- 3 lo?i c?t:
  - **Xanh d??ng**: T?ng doanh thu
  - **Xanh lá**: Thu nh?p gi?ng viên (60%)
  - **Vàng**: Phí n?n t?ng (40%)
- S? d?ng Chart.js ?? v? bi?u ??
- Tooltip hi?n th? s? ti?n ??nh d?ng VN?
- Tr?c Y: ??nh d?ng ti?n t? rút g?n (K, M)
- Tr?c X: Tên khóa h?c (t?i ?a 30 ký t?)

#### C. B?ng Chi ti?t (DataTable)
Hi?n th? t?t c? khóa h?c v?i thông tin:
- Tên khóa h?c & ID
- Giá
- S? l??t mua
- T?ng doanh thu
- Thu nh?p (60%)
- Phí n?n t?ng (40%)

Tính n?ng b?ng:
- Tìm ki?m
- S?p x?p
- Phân trang
- Ngôn ng? ti?ng Vi?t
- M?c ??nh s?p x?p theo thu nh?p gi?m d?n

## Files ?ã thay ??i

### 1. Controller
**File**: `Quiz_Web/Controllers/CourseController.cs`
- Thêm action method `Revenue()`
- Inject `LearningPlatformContext` ?? truy v?n database
- L?y danh sách khóa h?c c?a user v?i tr?ng thái `IsPublished = true`
- Join v?i `CoursePurchases` (status = "Paid")
- Tính toán doanh thu và truy?n vào ViewBag

### 2. ViewModel
**File**: `Quiz_Web/Models/ViewModels/CourseViewModels.cs`
- Thêm class `CourseRevenueViewModel`:
  ```csharp
  public class CourseRevenueViewModel
  {
      public int CourseId { get; set; }
      public string CourseTitle { get; set; }
      public decimal CoursePrice { get; set; }
      public int TotalPurchases { get; set; }
      public decimal GrossRevenue { get; set; }
      public decimal InstructorRevenue { get; set; }
      public decimal PlatformFee { get; set; }
  }
  ```

### 3. View - Danh sách khóa h?c
**File**: `Quiz_Web/Views/Course/My.cshtml`
- Thay ??i layout header ?? có 2 nút:
  ```razor
  <div class="d-flex gap-2">
      <a asp-action="Revenue" class="btn btn-success">
          <i class="fa-solid fa-chart-line me-1"></i> Doanh thu
      </a>
      <a asp-action="Builder" class="btn btn-primary">
          <i class="fa-solid fa-plus me-1"></i> T?o khóa h?c
      </a>
  </div>
  ```

### 4. View - Trang doanh thu
**File**: `Quiz_Web/Views/Course/Revenue.cshtml`
- Hi?n th? 4 cards th?ng kê
- Bi?u ?? c?t v?i Chart.js (d? li?u truy?n qua `data-revenue` attribute)
- B?ng chi ti?t v?i DataTables
- Script ?ã ???c tách ra file riêng

### 5. JavaScript (M?I)
**File**: `Quiz_Web/wwwroot/js/course/course-revenue.js`
- Kh?i t?o DataTable v?i c?u hình ti?ng Vi?t
- L?y d? li?u t? `data-revenue` attribute c?a canvas
- X? lý và s?p x?p d? li?u (top 10)
- T?o bi?u ?? Chart.js v?i c?u hình ??y ??
- Format s? ti?n VN? cho tooltip và tr?c Y

**L?i ích c?a vi?c tách file JS:**
- ? Code g?n gàng, d? b?o trì
- ? Có th? cache file JS riêng
- ? D? debug và test
- ? Tách bi?t logic kh?i view
- ? Có th? reuse code

## Th? vi?n s? d?ng
- **Chart.js 4.4.0**: V? bi?u ?? c?t
- **DataTables 2.1.5**: B?ng d? li?u có tính n?ng tìm ki?m/s?p x?p/phân trang
- **Bootstrap 5**: UI components
- **Font Awesome**: Icons
- **jQuery 3.7.1**: DOM manipulation và DataTables

## ?i?u ki?n hi?n th?
- Ch? tính khóa h?c ?ã xu?t b?n (`IsPublished = true`)
- Ch? tính giao d?ch ?ã thanh toán (`CoursePurchase.Status = "Paid"`)
- Ng??i dùng ph?i ??ng nh?p và là ch? s? h?u khóa h?c (`OwnerId = userId`)

## Routing
- **URL**: `/courses/revenue`
- **HTTP Method**: GET
- **Authorization**: Required (ph?i ??ng nh?p)

## Giao di?n
- Responsive: Ho?t ??ng t?t trên mobile, tablet, desktop
- Cards có hi?u ?ng hover (nâng nh? lên)
- Bi?u ?? responsive, t? ??ng ?i?u ch?nh theo kích th??c màn hình
- B?ng responsive v?i scroll ngang trên mobile

## Ki?m tra tính n?ng

### Test Case 1: Ng??i dùng ch?a có khóa h?c
- **K?t qu? mong ??i**: Hi?n th? thông báo "B?n ch?a có doanh thu t? khóa h?c nào."

### Test Case 2: Có khóa h?c nh?ng ch?a có ai mua
- **K?t qu? mong ??i**: 
  - T?t c? cards hi?n th? s? 0
  - Hi?n th? b?ng v?i doanh thu = 0

### Test Case 3: Có khóa h?c và có ng??i mua
- **K?t qu? mong ??i**:
  - Cards hi?n th? s? li?u chính xác
  - Bi?u ?? hi?n th? top 10 khóa h?c
  - B?ng hi?n th? ??y ?? thông tin
  - Tính toán chính xác: 60% cho gi?ng viên, 40% phí n?n t?ng

### Test Case 4: Có >10 khóa h?c
- **K?t qu? mong ??i**:
  - Bi?u ?? ch? hi?n th? top 10
  - B?ng hi?n th? t?t c? v?i phân trang

## L?u ý k? thu?t

### 1. Truy?n d? li?u t? View sang JavaScript
S? d?ng `data-*` attribute ?? truy?n d? li?u JSON:
```html
<canvas id="revenueChart" 
        data-revenue='@Html.Raw(System.Text.Json.JsonSerializer.Serialize(Model))'></canvas>
```

??c d? li?u trong JavaScript:
```javascript
const revenueData = JSON.parse(revenueChartElement.dataset.revenue || '[]');
```

### 2. Performance
- S? d?ng `Include()` ?? eager loading, tránh N+1 query
- Cache file JavaScript v?i `asp-append-version="true"`

### 3. B?o m?t
- Ki?m tra `OwnerId` ?? ??m b?o user ch? xem ???c doanh thu c?a mình

### 4. Formatting
- S? ti?n ??nh d?ng theo chu?n Vi?t Nam (1.000.000 ?)

### 5. Chart.js Configuration
- C?u hình tooltip và scale ?? hi?n th? ti?n t? VN?
- Responsive và maintainAspectRatio
- Custom colors và border radius

## M? r?ng trong t??ng lai
- [ ] L?c theo kho?ng th?i gian (ngày/tháng/n?m)
- [ ] Export báo cáo doanh thu (PDF/Excel)
- [ ] Bi?u ?? ???ng (Line Chart) ?? xem xu h??ng theo th?i gian
- [ ] So sánh doanh thu gi?a các khóa h?c
- [ ] Thông báo khi có ??n hàng m?i
- [ ] Dashboard t?ng h?p cho admin

## Demo URL
```
http://localhost:7158/courses/mine
? Click nút "Doanh thu"
? http://localhost:7158/courses/revenue
```

## C?u trúc File
```
Quiz_Web/
??? Controllers/
?   ??? CourseController.cs         # Thêm action Revenue()
??? Models/
?   ??? ViewModels/
?       ??? CourseViewModels.cs     # Thêm CourseRevenueViewModel
??? Views/
?   ??? Course/
?       ??? My.cshtml               # Thêm nút "Doanh thu"
?       ??? Revenue.cshtml          # Trang th?ng kê (NEW)
??? wwwroot/
    ??? js/
        ??? course/
            ??? course-revenue.js   # Logic bi?u ?? và DataTable (NEW)
