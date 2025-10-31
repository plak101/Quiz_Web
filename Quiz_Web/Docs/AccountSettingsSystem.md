# H? th?ng C�i ??t T�i kho?n Ng??i d�ng

## T?ng quan
H? th?ng c�i ??t t�i kho?n ng??i d�ng bao g?m 3 trang ch�nh:
1. **C�i ??t t�i kho?n** - Qu?n l� email v� m?t kh?u
2. **Ch?nh s?a h? s?** - C?p nh?t th�ng tin c� nh�n
3. **L?ch s? mua h�ng** - Xem c�c ??n h�ng ?� mua

## C?u tr�c File

### Controllers
- `Quiz_Web/Controllers/AccountController.cs` - X? l� c�c request cho account settings

### Views
- `Quiz_Web/Views/Account/Settings.cshtml` - Trang c�i ??t t�i kho?n
- `Quiz_Web/Views/Account/Profile.cshtml` - Trang ch?nh s?a h? s?
- `Quiz_Web/Views/Account/PurchaseHistory.cshtml` - Trang l?ch s? mua h�ng

### CSS
- `Quiz_Web/wwwroot/css/account-settings.css` - Stylesheet cho c�c trang account

### JavaScript
- `Quiz_Web/wwwroot/js/account-settings.js` - X? l� t??ng t�c cho Settings
- `Quiz_Web/wwwroot/js/account-profile.js` - X? l� t??ng t�c cho Profile

### Services
- `Quiz_Web/Services/UserService.cs` - Business logic cho user operations
- `Quiz_Web/Services/IServices/IUserService.cs` - Interface cho UserService

## T�nh n?ng

### 1. C�i ??t t�i kho?n (`/account/settings`)

#### C?p nh?t Email
- Click v�o icon b�t ch� b�n c?nh tr??ng Email
- Nh?p email m?i
- Click "L?u" ?? c?p nh?t
- Validation: Email ph?i ?�ng ??nh d?ng v� ch?a ???c s? d?ng

#### ??i m?t kh?u
- Click v�o icon b�t ch� b�n c?nh tr??ng M?t kh?u
- Nh?p m?t kh?u hi?n t?i
- Nh?p m?t kh?u m?i (�t nh?t 8 k� t?, bao g?m ch? hoa, ch? th??ng, s? v� k� t? ??c bi?t)
- X�c nh?n m?t kh?u m?i
- Click "L?u" ?? c?p nh?t

#### X�c th?c ?a y?u t?
- T�nh n?ng ?ang ph�t tri?n
- Button "B?t x�c th?c ?a y?u t?" hi?n th? th�ng b�o coming soon

### 2. Ch?nh s?a h? s? (`/account/profile`)

#### C?p nh?t th�ng tin
- H? v� t�n: C� th? ch?nh s?a
- T�n ng??i d�ng: Kh�ng th? thay ??i (disabled)
- S? ?i?n tho?i: C� th? ch?nh s?a (10 ch? s?)
- Click "L?u thay ??i" ?? c?p nh?t

### 3. L?ch s? mua h�ng (`/account/purchase-history`)
- Hi?n th? danh s�ch c�c ??n h�ng ?� mua
- Hi?n t?i hi?n th? "B?n ch?a c� ??n h�ng n�o"
- S? ???c ph�t tri?n th�m trong t??ng lai

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
  "message": "C?p nh?t email th�nh c�ng"
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
  "message": "C?p nh?t m?t kh?u th�nh c�ng"
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
  "message": "C?p nh?t h? s? th�nh c�ng"
}
```

## Security

### Authorization
- T?t c? c�c trang account settings y�u c?u authentication
- S? d?ng `[Authorize]` attribute tr�n controller actions
- Redirect v? trang login n?u ch?a ??ng nh?p

### CSRF Protection
- T?t c? c�c POST request ??u y�u c?u AntiForgeryToken
- S? d?ng `[ValidateAntiForgeryToken]` attribute
- Token ???c generate b?i `@Html.AntiForgeryToken()`

### Password Security
- M?t kh?u ???c hash b?ng SHA256 (HashHelper)
- Validate m?t kh?u hi?n t?i tr??c khi cho ph�p ??i
- Y�u c?u m?t kh?u m?nh: �t nh?t 8 k� t?, ch? hoa, ch? th??ng, s?, k� t? ??c bi?t

## UI/UX

### Design
- N?n tr?ng, clean v� simple
- S? d?ng m�u t�m (#5624d0) l�m m�u ch�nh
- Responsive design cho mobile v� tablet

### Navigation
- Tabs navigation v?i active state
- Active tab c� underline m�u t�m
- Hover effect cho t?t c? c�c interactive elements

### Feedback
- S? d?ng SweetAlert2 cho notifications
- Loading spinner khi processing requests
- Success/Error messages r� r�ng

## Testing

### Manual Testing Checklist
1. **Email Update:**
   - [ ] Update v?i email m?i valid
   - [ ] Update v?i email ?� t?n t?i
   - [ ] Update v?i email invalid format
   - [ ] Cancel edit

2. **Password Update:**
   - [ ] Update v?i m?t kh?u hi?n t?i ?�ng
   - [ ] Update v?i m?t kh?u hi?n t?i sai
   - [ ] Update v?i m?t kh?u m?i y?u
   - [ ] Update v?i confirm password kh�ng kh?p
   - [ ] Cancel edit

3. **Profile Update:**
   - [ ] Update h? t�n
   - [ ] Update s? ?i?n tho?i
   - [ ] Update v?i s? ?i?n tho?i invalid
   - [ ] Leave phone empty (optional field)

4. **Navigation:**
   - [ ] Click gi?a c�c tabs
   - [ ] Active state hi?n th? ?�ng
   - [ ] Responsive tr�n mobile

## Future Enhancements

1. **Multi-Factor Authentication**
   - T�ch h?p 2FA v?i email/SMS
   - QR code cho authenticator apps

2. **Purchase History**
   - Hi?n th? danh s�ch ??n h�ng
   - Filter v� search
   - Download invoices

3. **Avatar Upload**
   - Cho ph�p upload ?nh ??i di?n
   - Crop v� resize image

4. **Account Deletion**
   - Cho ph�p ng??i d�ng x�a t�i kho?n
   - Export data tr??c khi x�a

5. **Activity Log**
   - Hi?n th? l?ch s? ??ng nh?p
   - Thi?t b? ?� s? d?ng
   - IP addresses

## Troubleshooting

### L?i th??ng g?p

1. **"M?t kh?u hi?n t?i kh�ng ?�ng"**
   - Ki?m tra l?i m?t kh?u ?ang nh?p
   - ??m b?o Caps Lock kh�ng b?t

2. **"Email ?� ???c s? d?ng"**
   - Email n�y ?� t?n t?i trong h? th?ng
   - Th? email kh�c

3. **"M?t kh?u ph?i c� �t nh?t 8 k� t?..."**
   - M?t kh?u kh�ng ?? m?nh
   - ??m b?o c� ch? hoa, ch? th??ng, s? v� k� t? ??c bi?t

## Contact
N?u c� v?n ??, vui l�ng li�n h? team ph�t tri?n.
