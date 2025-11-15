# S?a l?i Revenue Reports - CoursePurchases hi?n th? 0 VN?

## V?n ?? phát hi?n

### Hi?n t??ng:
Trong trang **Revenue Reports** (`/admin/RevenueReports`):
- **"Giao d?ch g?n ?ây"** (RecentPurchases): Hi?n th? **0 VN?** ho?c không có d? li?u
- **"Thanh toán cao nh?t"** (TopPayments): Có d? li?u 299,000 VN? và 199,000 VN?

### Nguyên nhân g?c r?:

1. **Payments table** có d? li?u ?úng (299K, 199K) t? MoMo
2. **CoursePurchases table** ???c t?o b?i `GrantAccessAsync()` v?i `PricePaid = 0`
3. Query l?y `RecentPurchases` t? `CoursePurchases` nên hi?n th? 0 VN?
4. Query l?y `TopPayments` t? `Payments` nên hi?n th? giá ?úng

### Root Cause Code:

```csharp
// ? TR??C KHI S?A (SAI)
public async Task GrantAccessAsync(int userId, int courseId)
{
    var exists = await _context.CoursePurchases
        .AnyAsync(x => x.BuyerId == userId && x.CourseId == courseId && x.Status == "Paid");

    if (!exists)
    {
        _context.CoursePurchases.Add(new CoursePurchase
        {
            BuyerId = userId,
            CourseId = courseId,
            PricePaid = 0,  // ? HARDCODE = 0 ? SAI!
            Currency = "VND",
            Status = "Paid"
            // ? Thi?u PurchasedAt!
        });

        await _context.SaveChangesAsync();
    }
}
```

## Gi?i pháp ?ã áp d?ng

### 1. **PurchaseService.cs - GrantAccessAsync()**

#### ? S?a ?? l?y giá th?c c?a khóa h?c:

```csharp
public async Task GrantAccessAsync(int userId, int courseId)
{
    var exists = await _context.CoursePurchases
        .AnyAsync(x => x.BuyerId == userId && x.CourseId == courseId && x.Status == "Paid");

    if (!exists)
    {
        // ? L?Y GIÁ KHÓA H?C TH?C T?
        var course = await _context.Courses.FindAsync(courseId);
        var price = course?.Price ?? 0;

        _context.CoursePurchases.Add(new CoursePurchase
        {
            BuyerId = userId,
            CourseId = courseId,
            PricePaid = price,              // ? L?u giá th?c
            Currency = "VND",
            Status = "Paid",
            PurchasedAt = DateTime.UtcNow   // ? Thêm timestamp
        });

        await _context.SaveChangesAsync();
    }
}
```

### 2. **AdminController.cs - RevenueReports()**

#### ? Thêm debug logging và null-safe:

```csharp
public async Task<IActionResult> RevenueReports()
{
    // ? DEBUG: Ki?m tra d? li?u
    var allPurchases = await _context.CoursePurchases.ToListAsync();
    var paidPurchases = await _context.CoursePurchases.Where(p => p.Status == "Paid").ToListAsync();
    
    Console.WriteLine($"=== REVENUE REPORTS DEBUG ===");
    Console.WriteLine($"Total CoursePurchases: {allPurchases.Count}");
    Console.WriteLine($"Paid CoursePurchases: {paidPurchases.Count}");
    Console.WriteLine($"Statuses: {string.Join(", ", allPurchases.Select(p => p.Status).Distinct())}");

    // ? Null-safe queries
    var totalGrossRevenue = await _context.CoursePurchases
        .Where(p => p.Status == "Paid")
        .SumAsync(p => (decimal?)p.PricePaid) ?? 0;

    var recentPurchases = await _context.CoursePurchases
        .Include(p => p.Buyer)
        .Include(p => p.Course)
        .Where(p => p.Status == "Paid")
        .OrderByDescending(p => p.PurchasedAt)
        .Take(15)
        .Select(p => new { 
            FullName = p.Buyer != null ? p.Buyer.FullName : "Unknown",
            Title = p.Course != null ? p.Course.Title : "Unknown Course",
            PricePaid = p.PricePaid,
            PurchasedAt = p.PurchasedAt
        })
        .ToListAsync();
    
    // ...
}
```

### 3. **RevenueReports.cshtml - View**

#### ? Thêm null-check và thông báo chi ti?t:

```csharp
@if (Model.RecentPurchases != null && ((IEnumerable<dynamic>)Model.RecentPurchases).Any())
{
    <div class="table-responsive">
        <table class="table table-sm table-hover">
            <!-- ... -->
        </table>
    </div>
}
else
{
    <div class="alert alert-warning">
        <i class="bi bi-exclamation-triangle me-2"></i>
        <strong>Ch?a có giao d?ch nào!</strong>
        <p class="mb-0 mt-2">Có th? do:</p>
        <ul class="mb-0 mt-1">
            <li>Ch?a có ??n hàng nào ???c thanh toán thành công</li>
            <li>??n hàng ?ang ? tr?ng thái "Pending" - ch?a c?p nh?t thành "Paid"</li>
        </ul>
    </div>
}
```

## Flow thanh toán MoMo hi?n t?i

### Khi user thanh toán thành công:

```
1. User ch?n thanh toán MoMo
   ?
2. CreateMoMoPayment() t?o:
   - Order (Pending)
   - OrderItems (Course IDs + Prices)
   - Payment (Pending)
   ?
3. User thanh toán trên MoMo
   ?
4. MoMo callback ? MoMoReturn() / MoMoCallback()
   - Update Payment.Status = "Paid"
   - Update Order.Status = "Paid"
   ?
5. G?i GrantAccessAsync() cho t?ng OrderItem
   - ? L?Y GIÁ T? COURSES TABLE
   - ? T?O COURSEPURCHASES v?i giá th?c
   ?
6. Clear cart
```

## So sánh tr??c và sau

### ? Tr??c khi s?a:

| B?ng | D? li?u |
|------|---------|
| **Payments** | 299,000 VN?, 199,000 VN? ? |
| **CoursePurchases** | **0 VN?, 0 VN?** ? |

**K?t qu?:**
- "Thanh toán cao nh?t": Hi?n th? ?úng (t? Payments)
- "Giao d?ch g?n ?ây": Hi?n th? **0 VN?** (t? CoursePurchases)

### ? Sau khi s?a:

| B?ng | D? li?u |
|------|---------|
| **Payments** | 299,000 VN?, 199,000 VN? ? |
| **CoursePurchases** | **299,000 VN?, 199,000 VN?** ? |

**K?t qu?:**
- "Thanh toán cao nh?t": Hi?n th? ?úng ?
- "Giao d?ch g?n ?ây": Hi?n th? **?úng giá** ?

## H??ng d?n fix d? li?u c?

### N?u database ?ã có CoursePurchases v?i PricePaid = 0:

```sql
-- Script 1: C?p nh?t PricePaid t? Course.Price
UPDATE cp
SET cp.PricePaid = c.Price
FROM CoursePurchases cp
INNER JOIN Courses c ON cp.CourseId = c.CourseId
WHERE cp.PricePaid = 0 AND cp.Status = 'Paid';

-- Script 2: C?p nh?t PurchasedAt t? Payment n?u null
UPDATE cp
SET cp.PurchasedAt = p.PaidAt
FROM CoursePurchases cp
INNER JOIN Payments p ON p.OrderId IN (
    SELECT OrderId FROM OrderItems WHERE CourseId = cp.CourseId
)
WHERE cp.PurchasedAt IS NULL AND p.PaidAt IS NOT NULL;

-- Script 3: Verify d? li?u
SELECT 
    cp.PurchaseId,
    u.FullName AS Buyer,
    c.Title AS Course,
    cp.PricePaid,
    c.Price AS ActualPrice,
    cp.Status,
    cp.PurchasedAt
FROM CoursePurchases cp
INNER JOIN Users u ON cp.BuyerId = u.UserId
INNER JOIN Courses c ON cp.CourseId = c.CourseId
WHERE cp.Status = 'Paid'
ORDER BY cp.PurchasedAt DESC;
```

## Testing Checklist

### Manual Testing:

1. **Test New Purchase Flow:**
   - [ ] T?o ??n hàng m?i v?i khóa h?c giá 100,000 VN?
   - [ ] Thanh toán thành công qua MoMo
   - [ ] Ki?m tra CoursePurchases.PricePaid = 100,000 (không ph?i 0)
   - [ ] Ki?m tra RevenueReports hi?n th? ?úng giá

2. **Test Debug Logging:**
   - [ ] Truy c?p `/admin/RevenueReports`
   - [ ] Check console logs trong Visual Studio Output
   - [ ] Verify s? l??ng CoursePurchases và statuses

3. **Test Edge Cases:**
   - [ ] Mua nhi?u khóa h?c cùng lúc (gi? hàng)
   - [ ] Khóa h?c mi?n phí (Price = 0)
   - [ ] User/Course b? xóa sau khi mua

### Database Verification:

```sql
-- Check 1: So sánh Payments vs CoursePurchases
SELECT 
    'Payments' AS Source,
    SUM(Amount) AS Total,
    COUNT(*) AS Count
FROM Payments
WHERE Status = 'Paid'

UNION ALL

SELECT 
    'CoursePurchases' AS Source,
    SUM(PricePaid) AS Total,
    COUNT(*) AS Count
FROM CoursePurchases
WHERE Status = 'Paid';

-- Check 2: Find mismatched prices
SELECT 
    cp.PurchaseId,
    c.Title,
    cp.PricePaid AS PurchasedPrice,
    c.Price AS CurrentPrice,
    cp.PricePaid - c.Price AS Difference
FROM CoursePurchases cp
INNER JOIN Courses c ON cp.CourseId = c.CourseId
WHERE cp.PricePaid != c.Price AND cp.Status = 'Paid';
```

## Files Changed

1. `Quiz_Web\Services\PurchaseService.cs`
   - Method: `GrantAccessAsync()`
   - Changes: L?y giá th?c t? Courses table, thêm PurchasedAt

2. `Quiz_Web\Controllers\AdminController.cs`
   - Method: `RevenueReports()`
   - Changes: Thêm debug logging, null-safe queries

3. `Quiz_Web\Views\Admin\RevenueReports.cshtml`
   - Changes: Null-check, thông báo chi ti?t, responsive table

## Build Status
? **Build successful** - Không có l?i compilation

## Summary

### V?n ??:
- `CoursePurchases.PricePaid` ???c hardcode = 0 trong `GrantAccessAsync()`
- Revenue Reports hi?n th? 0 VN? cho "Giao d?ch g?n ?ây"

### Gi?i pháp:
- ? L?y giá th?c t? `Courses.Price`
- ? Thêm `PurchasedAt` timestamp
- ? Null-safe queries
- ? Debug logging
- ? Thông báo rõ ràng khi không có d? li?u

### K?t qu?:
- ? Giao d?ch m?i s? l?u giá ?úng
- ? Revenue Reports hi?n th? chính xác
- ? H? tr? debug v?i console logs
- ? UX t?t h?n v?i thông báo chi ti?t

### Next Steps:
1. Run SQL scripts ?? fix d? li?u c? (n?u có)
2. Test v?i ??n hàng m?i
3. Monitor console logs trong vài ngày ??u
4. Remove debug logs sau khi stable
