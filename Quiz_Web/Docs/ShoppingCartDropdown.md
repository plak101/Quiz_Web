# Shopping Cart Dropdown - H??ng d?n

## T?ng quan
Component dropdown gi? hàng hi?n th? danh sách các khóa h?c trong gi? hàng, cho phép ng??i dùng xem và qu?n lý gi? hàng tr?c ti?p t? navigation bar.

## C?u trúc

### 1. Database
- **B?ng**: `dbo.ShoppingCarts`
  - `CartId`: ID gi? hàng
  - `UserId`: ID ng??i dùng
  - `CreatedAt`: Th?i gian t?o
  - `UpdatedAt`: Th?i gian c?p nh?t cu?i

- **B?ng**: `dbo.CartItems`
  - `CartItemId`: ID item
  - `CartId`: Foreign key ??n ShoppingCarts
  - `CourseId`: Foreign key ??n Courses
  - `AddedAt`: Th?i gian thêm vào gi?

### 2. Backend (Service Layer)

#### ICartService Interface
```csharp
- Task<ShoppingCart?> GetOrCreateCartAsync(int userId)
- Task<List<CartItem>> GetCartItemsAsync(int userId)
- Task<int> GetCartItemCountAsync(int userId)
- Task<decimal> GetCartTotalAsync(int userId)
- Task<bool> AddToCartAsync(int userId, int courseId)
- Task<bool> RemoveFromCartAsync(int userId, int courseId)
- Task<bool> ClearCartAsync(int userId)
- Task<bool> IsInCartAsync(int userId, int courseId)
```

#### CartService Implementation
- T? ??ng t?o cart n?u ch?a có
- Include Course và Owner data khi load items
- Ki?m tra duplicate items
- Ki?m tra ?ã mua hay ch?a
- Tính t?ng giá tr? gi? hàng

### 3. API Controller

#### CartController
Các endpoints:
- `GET /api/cart/items` - L?y danh sách items
- `POST /api/cart/add/{courseId}` - Thêm course vào gi?
- `DELETE /api/cart/remove/{courseId}` - Xóa course kh?i gi?
- `DELETE /api/cart/clear` - Xóa toàn b? gi? hàng
- `GET /api/cart/count` - L?y s? l??ng items

### 4. Frontend (View)

#### _CartDropdown.cshtml
- Inject ICartService ?? l?y s? l??ng items
- Badge hi?n th? s? items (ch? hi?n khi > 0)
- Loading state khi ?ang t?i d? li?u
- Empty state khi gi? hàng tr?ng
- List items v?i thumbnail, title, instructor, price
- Footer v?i t?ng ti?n và nút "Chuy?n ??n gi? hàng"

### 5. Styling (CSS)

#### cart-dropdown.css
**Classes chính:**
- `.cart-dropdown`: Container
- `.cart-dropdown-menu`: Dropdown panel
- `.cart-badge`: Badge hi?n th? s? items
- `.cart-item`: Single cart item
- `.cart-item-image`: Thumbnail 80x50px
- `.cart-item-remove`: Nút xóa item
- `.cart-total`: Ph?n t?ng ti?n
- `.btn-purple`: Nút chính

**Features:**
- ? Min width: 350px, Max width: 400px
- ? Max height: 600px v?i scrollbar
- ? Border radius: 8px
- ? Box shadow
- ? Hover effects
- ? Smooth transitions
- ? Custom scrollbar
- ? Responsive design

### 6. JavaScript Logic

#### cart-dropdown.js
**Functions:**
- `loadCartItems()`: Load và render items t? API
- `renderCartItems(items, total)`: Render HTML
- `formatPrice(price, currency)`: Format giá ti?n
- `removeFromCart(courseId)`: Xóa item
- `addToCart(courseId)`: Thêm item (global function)
- `updateCartBadge()`: C?p nh?t badge s? l??ng
- `showToast(type, message)`: Hi?n th? thông báo

**Events:**
- T? ??ng load items khi dropdown ???c m?
- Listen for `cartUpdated` custom event
- Update badge on page load (n?u authenticated)

## Tính n?ng

### Giao di?n

#### Header
- Tiêu ?? "Gi? hàng c?a b?n"
- Background: #f7f9fa
- Border bottom

#### Cart Items
- Thumbnail 80x50px bên trái
- Title (max 2 lines v?i ellipsis)
- Instructor name v?i icon
- Price và nút "Xóa" trên cùng hàng
- Hover background: #f7f9fa

#### Footer
- T?ng ti?n v?i font bold
- Nút "Chuy?n ??n gi? hàng" màu tím (#5624d0)
- Full width button

#### Empty State
- Icon gi? hàng r?ng (48px)
- Text "Gi? hàng c?a b?n ?ang tr?ng"
- Link "Ti?p t?c mua s?m"

### T??ng tác

#### Badge
- Ch? hi?n th? khi có items
- Background tím (#5624d0)
- T? ??ng update khi thêm/xóa

#### Dropdown
- Click icon ?? m?/?óng
- T? ??ng load items khi m?
- Loading spinner trong lúc load
- Smooth animation (fadeIn)

#### Add to Cart
- Ki?m tra ?ã mua hay ch?a
- Ki?m tra duplicate
- Show toast notification
- Update badge

#### Remove from Cart
- Confirm dialog
- Update UI ngay l?p t?c
- Update t?ng ti?n
- Show toast notification
- Update badge

### Authentication
- Redirect ??n login n?u ch?a ??ng nh?p
- Return URL ?? quay l?i trang hi?n t?i

## Responsive

### Desktop (> 992px)
- Width: 350-400px
- Full features

### Mobile (< 576px)
- Width: 300px ho?c 90vw
- Smaller thumbnail (60x40px)
- Smaller font sizes

## Integration

### Thêm vào Course Detail Page
```html
<button onclick="addToCart(@Model.CourseId)" class="btn btn-primary">
    <i class="bi bi-cart-plus"></i> Thêm vào gi? hàng
</button>
```

### Thêm vào Course Card
```html
<button onclick="addToCart(@course.CourseId)" class="btn btn-sm btn-outline-primary">
    <i class="bi bi-cart-plus"></i>
</button>
```

### Listen for Cart Updates
```javascript
document.addEventListener('cartUpdated', function() {
    // Your custom logic
    console.log('Cart was updated');
});
```

### Custom Toast Notifications
Dropdown s? d?ng toastr n?u có, n?u không s? fallback sang alert().

?? s? d?ng toastr, thêm vào layout:
```html
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.css">
<script src="https://cdnjs.cloudflare.com/ajax/libs/toastr.js/latest/toastr.min.js"></script>
```

## Security

### Authorization
- T?t c? API endpoints yêu c?u [Authorize]
- Verify userId t? claims
- Throw UnauthorizedException n?u không authenticated

### Validation
- Check course exists và published
- Check không duplicate items
- Check ch?a mua tr??c ?ó
- Validate userId matches cart owner

## Performance

### Caching
Hi?n t?i ch?a implement caching. Có th? c?i thi?n b?ng:
```csharp
// Memory cache for cart count
_cache.GetOrCreate($"cart_count_{userId}", entry => {
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    return _context.CartItems.Count(ci => ci.Cart.UserId == userId);
});
```

### Database Indexing
??m b?o có index trên:
```sql
CREATE INDEX IX_CartItems_CartId ON dbo.CartItems(CartId);
CREATE INDEX IX_CartItems_CourseId ON dbo.CartItems(CourseId);
CREATE INDEX IX_ShoppingCarts_UserId ON dbo.ShoppingCarts(UserId);
```

## Error Handling

### Frontend
- Loading state v?i spinner
- Error state v?i icon và message
- Fallback image cho missing course covers
- Confirm dialog tr??c khi xóa

### Backend
- Try-catch trong t?t c? methods
- Log errors v?i ILogger
- Return success/failure responses
- Meaningful error messages

## Testing

### Manual Testing
1. ??ng nh?p
2. Click icon gi? hàng
3. Verify dropdown m? và load items
4. Add course t? course detail page
5. Verify badge t?ng
6. Verify item hi?n th? trong dropdown
7. Click "Xóa" và verify item b? xóa
8. Verify t?ng ti?n ???c tính ?úng
9. Click "Chuy?n ??n gi? hàng"
10. ??ng xu?t và verify redirect ??n login

### Browser Console
```javascript
// Check cart count
fetch('/api/cart/count').then(r => r.json()).then(console.log);

// Add to cart
addToCart(1);

// Remove from cart
removeFromCart(1);
```

## Troubleshooting

### Badge không hi?n th?
- Check user authenticated
- Check cart có items
- Check JavaScript loaded
- Check console for errors

### Items không load
- Check API endpoint
- Check authentication
- Check database connection
- Check browser network tab

### Không th? add to cart
- Check course exists
- Check course published
- Check not already purchased
- Check not duplicate in cart

### Styling không ?úng
- Check CSS file loaded
- Check CSS order (cart-dropdown.css sau bootstrap)
- Clear browser cache
- Check responsive breakpoints

## Future Enhancements

### Wishlist Integration
- Move items gi?a cart và wishlist
- Save for later feature

### Coupon/Discount
- Apply discount codes
- Show original + discounted price

### Quick Checkout
- Checkout tr?c ti?p t? dropdown
- Payment methods dropdown

### Recommendations
- "Ng??i khác c?ng mua" section
- "Bundle deals" suggestions

### Persistence
- Save cart for guest users (localStorage)
- Merge cart khi login

## Files Created

```
Quiz_Web/
??? Services/
?   ??? IServices/
?   ?   ??? ICartService.cs
?   ??? CartService.cs
??? Controllers/
?   ??? CartController.cs
??? Views/
?   ??? Shared/
?       ??? _CartDropdown.cshtml
??? wwwroot/
?   ??? css/
?   ?   ??? cart-dropdown.css
?   ??? js/
?       ??? cart-dropdown.js
??? Docs/
    ??? ShoppingCartDropdown.md (this file)
```

## Dependencies

- ASP.NET Core 9.0
- Entity Framework Core
- Bootstrap 5
- Bootstrap Icons
- jQuery (optional, for AJAX)
- Toastr (optional, for notifications)

## References

- [Bootstrap Dropdown](https://getbootstrap.com/docs/5.3/components/dropdowns/)
- [Bootstrap Icons](https://icons.getbootstrap.com/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
