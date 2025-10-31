# H??ng d?n s?a l?i n�t Edit kh�ng click ???c

## V?n ??
Kh�ng th? click v�o bi?u t??ng c�y b�t (edit button) ?? ch?nh s?a Email ho?c M?t kh?u trong trang Account Settings.

## Nguy�n nh�n
1. JavaScript ch?a load ?�ng c�ch
2. Event listeners ch?a ???c attach
3. CSS c� th? ch?n pointer events
4. C� th? c� conflict v?i code kh�c

## Gi?i ph�p ?� �p d?ng

### 1. S?a JavaScript (account-settings.js)
- ? Thay ??i t? jQuery `$(document).ready()` sang native `DOMContentLoaded`
- ? Th�m logging ?? debug
- ? S? d?ng `addEventListener` thay v� jQuery event binding
- ? Th�m `e.preventDefault()` ?? ng?n default behavior
- ? Ki?m tra xem elements c� t?n t?i tr??c khi attach events

### 2. C?p nh?t CSS (account-settings.css)
- ? Th�m `pointer-events: auto` cho `.edit-btn`
- ? Th�m `z-index: 1` ?? ??a button l�n tr�n
- ? Th�m `pointer-events: none` cho icon b�n trong
- ? Th�m hover v� active effects

## C�ch ki?m tra

### B??c 1: M? Developer Tools (F12)
```
1. Nh?n F12 trong tr�nh duy?t
2. V�o tab Console
3. Refresh trang (Ctrl + F5)
4. Ki?m tra c�c log messages:
   - "Account settings JS file loaded"
   - "DOM Content Loaded"
   - "jQuery version: x.x.x"
   - "Email button found: true"
   - "Password button found: true"
```

### B??c 2: Ki?m tra Elements
```
1. V�o tab Elements
2. T�m button v?i id="editEmailBtn"
3. Ki?m tra computed styles:
   - cursor: pointer
   - pointer-events: auto
   - z-index: 1
```

### B??c 3: Click v�o n�t Edit
```
1. Click v�o icon c�y b�t b�n Email
2. Console s? hi?n th?: "Edit email button clicked!"
3. Form edit s? slide down
4. Button edit s? ?n
```

## N?u v?n kh�ng ho?t ??ng

### Check 1: jQuery c� load kh�ng?
M? Console v� g�:
```javascript
typeof $
```
K?t qu? ph?i l�: "function"

### Check 2: SweetAlert2 c� load kh�ng?
```javascript
typeof Swal
```
K?t qu? ph?i l�: "object"

### Check 3: Button c� event listener kh�ng?
```javascript
const btn = document.getElementById('editEmailBtn');
console.log(btn);
console.log(getEventListeners(btn)); // Chrome only
```

### Check 4: Clear cache v� reload
```
1. Nh?n Ctrl + Shift + Delete
2. Ch?n "Cached images and files"
3. Click "Clear data"
4. Reload page (Ctrl + F5)
```

## Alternative: Manual Test
N?u button v?n kh�ng click ???c, th? ch?y code n�y trong Console:

```javascript
// Test Email button
document.getElementById('editEmailBtn').click();

// Or manually trigger
document.getElementById('emailEditForm').style.display = 'block';
document.getElementById('editEmailBtn').style.display = 'none';
```

## File ?� thay ??i
1. ? `Quiz_Web/wwwroot/js/account-settings.js` - Event handlers m?i
2. ? `Quiz_Web/wwwroot/css/account-settings.css` - CSS fixes

## Testing Checklist

- [ ] Load trang /account/settings
- [ ] M? Developer Tools Console
- [ ] Ki?m tra c�c log messages
- [ ] Click v�o n�t Edit b�n Email
- [ ] Xem console c� log "Edit email button clicked!" kh�ng
- [ ] Form edit c� hi?n ra kh�ng
- [ ] Nh?p email m?i v� click Save
- [ ] Ki?m tra c� g?i AJAX kh�ng
- [ ] Click Cancel ?? ?�ng form
- [ ] L?p l?i v?i n�t Edit b�n Password

## Li�n h? h? tr?
N?u v?n g?p v?n ??, vui l�ng:
1. Ch?p m�n h�nh Console (F12 > Console tab)
2. Ch?p m�n h�nh Network tab (khi click button)
3. G?i th�ng tin tr�nh duy?t v� version
