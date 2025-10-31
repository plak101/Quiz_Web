# H? th?ng Cài ??t Tài kho?n Ng??i dùng

## T?ng quan
H? th?ng cài ??t tài kho?n ng??i dùng bao g?m 3 trang chính:
1. **Cài ??t tài kho?n** - Qu?n lý email và m?t kh?u
2. **Ch?nh s?a h? s?** - C?p nh?t thông tin cá nhân
3. **L?ch s? mua hàng** - Xem các ??n hàng ?ã mua

## C?u trúc File

### Controllers
- `Quiz_Web/Controllers/AccountController.cs` - X? lý các request cho account settings

### Views
- `Quiz_Web/Views/Account/Settings.cshtml` - Trang cài ??t tài kho?n
- `Quiz_Web/Views/Account/Profile.cshtml` - Trang ch?nh s?a h? s?
- `Quiz_Web/Views/Account/PurchaseHistory.cshtml` - Trang l?ch s? mua hàng

### CSS
- `Quiz_Web/wwwroot/css/account-settings.css` - Stylesheet cho các trang account

### JavaScript
- `Quiz_Web/wwwroot/js/account-settings.js` - X? lý t??ng tác cho Settings
- `Quiz_Web/wwwroot/js/account-profile.js` - X? lý t??ng tác cho Profile

### Services
- `Quiz_Web/Services/UserService.cs` - Business logic cho user operations
- `Quiz_Web/Services/IServices/IUserService.cs` - Interface cho UserService

## Tính n?ng

### 1. Cài ??t tài kho?n (`/account/settings`)

#### C?p nh?t Email
- Click vào icon bút chì bên c?nh tr??ng Email
- Nh?p email m?i
- Click "L?u" ?? c?p nh?t
- Validation: Email ph?i ?úng ??nh d?ng và ch?a ???c s? d?ng

#### ??i m?t kh?u
- Click vào icon bút chì bên c?nh tr??ng M?t kh?u
- Nh?p m?t kh?u hi?n t?i
- Nh?p m?t kh?u m?i (ít nh?t 8 ký t?, bao g?m ch? hoa, ch? th??ng, s? và ký t? ??c bi?t)
- Xác nh?n m?t kh?u m?i
- Click "L?u" ?? c?p nh?t

#### Xác th?c ?a y?u t?
- Tính n?ng ?ang phát tri?n
- Button "B?t xác th?c ?a y?u t?" hi?n th? thông báo coming soon

### 2. Ch?nh s?a h? s? (`/account/profile`)

#### C?p nh?t thông tin
- H? và tên: Có th? ch?nh s?a
- Tên ng??i dùng: Không th? thay ??i (disabled)
- S? ?i?n tho?i: Có th? ch?nh s?a (10 ch? s?)
- Click "L?u thay ??i" ?? c?p nh?t

### 3. L?ch s? mua hàng (`/account/purchase-history`)
- Hi?n th? danh sách các ??n hàng ?ã mua
- Hi?n t?i hi?n th? "B?n ch?a có ??n hàng nào"
- S? ???c phát tri?n thêm trong t??ng lai

## API Endpoints

### POST `/Account/UpdateEmail`
**Request:**
```json
{
  "newEmail": "newemail@example.com",
  "__RequestVerificationToken": "token"
}
```

**Response:**
```json
{
  "status": "SUCCESS",
  "message": "C?p nh?t email thành công"
}
```

### POST `/Account/UpdatePassword`
**Request:**
```json
{
  "currentPassword": "oldpassword",
  "newPassword": "newpassword",
  "confirmPassword": "newpassword",
  "__RequestVerificationToken": "token"
}
```

**Response:**
```json
{
  "status": "SUCCESS",
  "message": "C?p nh?t m?t kh?u thành công"
}
```

### POST `/Account/UpdateProfile`
**Request:**
```json
{
  "fullName": "Nguy?n V?n A",
  "phone": "0123456789",
  "__RequestVerificationToken": "token"
}
```

**Response:**
```json
{
  "status": "SUCCESS",
  "message": "C?p nh?t h? s? thành công"
}
```

## Security

### Authorization
- T?t c? các trang account settings yêu c?u authentication
- S? d?ng `[Authorize]` attribute trên controller actions
- Redirect v? trang login n?u ch?a ??ng nh?p

### CSRF Protection
- T?t c? các POST request ??u yêu c?u AntiForgeryToken
- S? d?ng `[ValidateAntiForgeryToken]` attribute
- Token ???c generate b?i `@Html.AntiForgeryToken()`

### Password Security
- M?t kh?u ???c hash b?ng SHA256 (HashHelper)
- Validate m?t kh?u hi?n t?i tr??c khi cho phép ??i
- Yêu c?u m?t kh?u m?nh: ít nh?t 8 ký t?, ch? hoa, ch? th??ng, s?, ký t? ??c bi?t

## UI/UX

### Design
- N?n tr?ng, clean và simple
- S? d?ng màu tím (#5624d0) làm màu chính
- Responsive design cho mobile và tablet

### Navigation
- Tabs navigation v?i active state
- Active tab có underline màu tím
- Hover effect cho t?t c? các interactive elements

### Feedback
- S? d?ng SweetAlert2 cho notifications
- Loading spinner khi processing requests
- Success/Error messages rõ ràng

## Testing

### Manual Testing Checklist
1. **Email Update:**
   - [ ] Update v?i email m?i valid
   - [ ] Update v?i email ?ã t?n t?i
   - [ ] Update v?i email invalid format
   - [ ] Cancel edit

2. **Password Update:**
   - [ ] Update v?i m?t kh?u hi?n t?i ?úng
   - [ ] Update v?i m?t kh?u hi?n t?i sai
   - [ ] Update v?i m?t kh?u m?i y?u
   - [ ] Update v?i confirm password không kh?p
   - [ ] Cancel edit

3. **Profile Update:**
   - [ ] Update h? tên
   - [ ] Update s? ?i?n tho?i
   - [ ] Update v?i s? ?i?n tho?i invalid
   - [ ] Leave phone empty (optional field)

4. **Navigation:**
   - [ ] Click gi?a các tabs
   - [ ] Active state hi?n th? ?úng
   - [ ] Responsive trên mobile

## Future Enhancements

1. **Multi-Factor Authentication**
   - Tích h?p 2FA v?i email/SMS
   - QR code cho authenticator apps

2. **Purchase History**
   - Hi?n th? danh sách ??n hàng
   - Filter và search
   - Download invoices

3. **Avatar Upload**
   - Cho phép upload ?nh ??i di?n
   - Crop và resize image

4. **Account Deletion**
   - Cho phép ng??i dùng xóa tài kho?n
   - Export data tr??c khi xóa

5. **Activity Log**
   - Hi?n th? l?ch s? ??ng nh?p
   - Thi?t b? ?ã s? d?ng
   - IP addresses

## Troubleshooting

### L?i th??ng g?p

1. **"M?t kh?u hi?n t?i không ?úng"**
   - Ki?m tra l?i m?t kh?u ?ang nh?p
   - ??m b?o Caps Lock không b?t

2. **"Email ?ã ???c s? d?ng"**
   - Email này ?ã t?n t?i trong h? th?ng
   - Th? email khác

3. **"M?t kh?u ph?i có ít nh?t 8 ký t?..."**
   - M?t kh?u không ?? m?nh
   - ??m b?o có ch? hoa, ch? th??ng, s? và ký t? ??c bi?t

## Contact
N?u có v?n ??, vui lòng liên h? team phát tri?n.
