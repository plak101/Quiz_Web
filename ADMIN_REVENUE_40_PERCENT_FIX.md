# S?a l?i T?ng Doanh Thu Admin - Ch? hi?n th? 40%

## V?n ??
Tr??c ?ây, ph?n t?ng doanh thu trong admin dashboard ?ang hi?n th? 100% t?ng s? ti?n t? các khóa h?c ?ã bán. Theo yêu c?u nghi?p v?:
- **Ng??i s? h?u khóa h?c (Instructor)** nh?n **60%** doanh thu
- **Admin (N?n t?ng)** nh?n **40%** doanh thu (phí n?n t?ng)

## Gi?i pháp
?ã c?p nh?t `DashboardService.cs`, `AdminController.cs` và các views liên quan ?? tính và hi?n th? ?úng 40% doanh thu cho admin.

## Các file ?ã s?a

### 1. **Quiz_Web\Services\DashboardService.cs**
#### Thay ??i trong `GetOverviewData()`:
```csharp
// ? TR??C (SAI):
var totalRevenue = _context.Payments.Where(p => p.Status == "completed").Sum(p => (decimal?)p.Amount) ?? 0;

// ? SAU (?ÚNG):
var totalPayments = _context.Payments.Where(p => p.Status == "Paid").Sum(p => (decimal?)p.Amount) ?? 0;
var totalRevenue = totalPayments * 0.40m; // Admin nh?n 40%, instructor nh?n 60%
```

#### Thay ??i trong `GetRevenuePayments()`:
- **MonthlyRevenue**: Tính 40% cho t?ng tháng
- **RevenueBySource**: Tính 40% cho t?ng ph??ng th?c thanh toán
- **TopSellingCourses**: Tính 40% doanh thu cho t?ng khóa h?c
- **TotalRevenue**: Tính 40% t?ng doanh thu

### 2. **Quiz_Web\Controllers\AdminController.cs**
#### Thay ??i trong `RevenueReports()`:
```csharp
// ? Tính t?ng doanh thu t? CoursePurchases (100%)
var totalGrossRevenue = await _context.CoursePurchases
    .Where(p => p.Status == "Paid")
    .SumAsync(p => p.PricePaid);

var monthlyGrossRevenue = await _context.CoursePurchases
    .Where(p => p.Status == "Paid" && p.PurchasedAt >= DateTime.UtcNow.AddDays(-30))
    .SumAsync(p => p.PricePaid);

// ? Tính 40% cho admin
var totalRevenue = totalGrossRevenue * 0.40m;
var monthlyRevenue = monthlyGrossRevenue * 0.40m;
```

**L?u ý**: ?ã s?a status t? `"completed"` ? `"Paid"` trong TopPayments query

### 3. **Quiz_Web\Views\Admin\Index.cshtml**
- C?p nh?t tiêu ?? card: "Doanh thu Admin (40%)"
- Thêm mô t?: "Phí n?n t?ng"
- C?p nh?t tiêu ?? bi?u ??: "Doanh thu Admin theo tháng (40%)"
- Thêm ghi chú: "Phí n?n t?ng - Instructor nh?n 60%"

### 4. **Quiz_Web\Views\Admin\RevenuePayments.cshtml**
- C?p nh?t t?t c? tiêu ?? và tooltip ?? rõ ràng hi?n th? "Doanh thu Admin (40%)"
- Thêm ghi chú v? instructor nh?n 60%

### 5. **Quiz_Web\Views\Admin\RevenueReports.cshtml**
- C?p nh?t card hi?n th? "Doanh thu Admin (40%)"
- Thêm mô t? "Phí n?n t?ng - Instructor nh?n 60%"

## Công th?c tính
```
Admin Revenue = Total Payment Amount × 0.40
Instructor Revenue = Total Payment Amount × 0.60
```

## Status Payment ?ã ???c chu?n hóa
- **Tr??c**: M?t s? ch? dùng `"completed"`, m?t s? ch? dùng `"Paid"`
- **Sau**: T?t c? ??u s? d?ng `"Paid"` ?? th?ng nh?t v?i database schema

## Ki?m tra
1. ? **T?ng quan (Dashboard)**: Card "Doanh thu Admin (40%)" hi?n th? ?úng 40% t?ng thanh toán
2. ? **Doanh thu & Thanh toán**: T?t c? bi?u ?? và s? li?u ??u tính theo 40%
3. ? **Báo cáo doanh thu (RevenueReports)**: Hi?n th? rõ ràng ?ây là ph?n doanh thu c?a admin (40%)

## L?u ý quan tr?ng
- Ph?n doanh thu c?a **Instructor (60%)** ???c hi?n th? trong trang `/courses/revenue` c?a t?ng instructor
- Trong view `RevenueReports`, b?ng **"Giao d?ch g?n ?ây"** hi?n th? **s? ti?n g?c 100%** (không tính 40%) ?? admin có th? xem chi ti?t giao d?ch
- Ch? các card t?ng h?p m?i hi?n th? 40%

## Test Cases
1. ? Khi có m?t khóa h?c giá 100,000 VN? ???c mua:
   - Admin nh?n: 40,000 VN? (40%)
   - Instructor nh?n: 60,000 VN? (60%)

2. ? T?ng doanh thu t? nhi?u khóa h?c:
   - N?u t?ng thanh toán = 1,000,000 VN?
   - Admin Dashboard hi?n th?: 400,000 VN? (40%)
   - T?ng instructor revenue: 600,000 VN? (60%)

3. ? Báo cáo doanh thu:
   - **TotalRevenue**: Hi?n th? 40% t?ng CoursePurchases
   - **MonthlyRevenue**: Hi?n th? 40% doanh thu tháng này
   - **TopPayments**: L?y t? b?ng Payments v?i status "Paid"

## Build Status
? **Build successful** - Không có l?i compilation

## Migration Notes
N?u database có d? li?u c? v?i status `"completed"`, c?n ch?y migration ?? c?p nh?t:

```sql
UPDATE Payments SET Status = 'Paid' WHERE Status = 'completed';
UPDATE CoursePurchases SET Status = 'Paid' WHERE Status = 'completed';
```

## Summary
?ã hoàn thành vi?c s?a l?i tính toán doanh thu:
- ? DashboardService: Tính 40% cho t?t c? metrics
- ? AdminController.RevenueReports(): Tính 40% cho TotalRevenue và MonthlyRevenue
- ? Views: Hi?n th? rõ ràng "Doanh thu Admin (40%)" và ghi chú v? instructor
- ? Status chu?n hóa: T?t c? s? d?ng "Paid"
- ? Build thành công, không có l?i
