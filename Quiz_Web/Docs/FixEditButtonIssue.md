# H??ng d?n s?a l?i nút Edit không click ???c

## V?n ??
Không th? click vào bi?u t??ng cây bút (edit button) ?? ch?nh s?a Email ho?c M?t kh?u trong trang Account Settings.

## Nguyên nhân
1. JavaScript ch?a load ?úng cách
2. Event listeners ch?a ???c attach
3. CSS có th? ch?n pointer events
4. Có th? có conflict v?i code khác

## Gi?i pháp ?ã áp d?ng

### 1. S?a JavaScript (account-settings.js)
- ? Thay ??i t? jQuery `$(document).ready()` sang native `DOMContentLoaded`
- ? Thêm logging ?? debug
- ? S? d?ng `addEventListener` thay vì jQuery event binding
- ? Thêm `e.preventDefault()` ?? ng?n default behavior
- ? Ki?m tra xem elements có t?n t?i tr??c khi attach events

### 2. C?p nh?t CSS (account-settings.css)
- ? Thêm `pointer-events: auto` cho `.edit-btn`
- ? Thêm `z-index: 1` ?? ??a button lên trên
- ? Thêm `pointer-events: none` cho icon bên trong
- ? Thêm hover và active effects

## Cách ki?m tra

### B??c 1: M? Developer Tools (F12)
```
1. Nh?n F12 trong trình duy?t
2. Vào tab Console
3. Refresh trang (Ctrl + F5)
4. Ki?m tra các log messages:
   - "Account settings JS file loaded"
   - "DOM Content Loaded"
   - "jQuery version: x.x.x"
   - "Email button found: true"
   - "Password button found: true"
```

### B??c 2: Ki?m tra Elements
```
1. Vào tab Elements
2. Tìm button v?i id="editEmailBtn"
3. Ki?m tra computed styles:
   - cursor: pointer
   - pointer-events: auto
   - z-index: 1
```

### B??c 3: Click vào nút Edit
```
1. Click vào icon cây bút bên Email
2. Console s? hi?n th?: "Edit email button clicked!"
3. Form edit s? slide down
4. Button edit s? ?n
```

## N?u v?n không ho?t ??ng

### Check 1: jQuery có load không?
M? Console và gõ:
```javascript
typeof $
```
K?t qu? ph?i là: "function"

### Check 2: SweetAlert2 có load không?
```javascript
typeof Swal
```
K?t qu? ph?i là: "object"

### Check 3: Button có event listener không?
```javascript
const btn = document.getElementById('editEmailBtn');
console.log(btn);
console.log(getEventListeners(btn)); // Chrome only
```

### Check 4: Clear cache và reload
```
1. Nh?n Ctrl + Shift + Delete
2. Ch?n "Cached images and files"
3. Click "Clear data"
4. Reload page (Ctrl + F5)
```

## Alternative: Manual Test
N?u button v?n không click ???c, th? ch?y code này trong Console:

```javascript
// Test Email button
document.getElementById('editEmailBtn').click();

// Or manually trigger
document.getElementById('emailEditForm').style.display = 'block';
document.getElementById('editEmailBtn').style.display = 'none';
```

## File ?ã thay ??i
1. ? `Quiz_Web/wwwroot/js/account-settings.js` - Event handlers m?i
2. ? `Quiz_Web/wwwroot/css/account-settings.css` - CSS fixes

## Testing Checklist

- [ ] Load trang /account/settings
- [ ] M? Developer Tools Console
- [ ] Ki?m tra các log messages
- [ ] Click vào nút Edit bên Email
- [ ] Xem console có log "Edit email button clicked!" không
- [ ] Form edit có hi?n ra không
- [ ] Nh?p email m?i và click Save
- [ ] Ki?m tra có g?i AJAX không
- [ ] Click Cancel ?? ?óng form
- [ ] L?p l?i v?i nút Edit bên Password

## Liên h? h? tr?
N?u v?n g?p v?n ??, vui lòng:
1. Ch?p màn hình Console (F12 > Console tab)
2. Ch?p màn hình Network tab (khi click button)
3. G?i thông tin trình duy?t và version
