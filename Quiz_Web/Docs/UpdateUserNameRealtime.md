# C?p nh?t Real-time T�n ng??i d�ng trong Dropdown

## V?n ??
Khi c?p nh?t h? t�n trong trang `/account/profile`, t�n trong dropdown menu kh�ng thay ??i cho ??n khi reload trang ho?c logout/login l?i.

## Nguy�n nh�n
- T�n ng??i d�ng trong dropdown ???c l?y t? `User.Identity.Name` (Claims)
- Claims ???c set khi ??ng nh?p v� ch? thay ??i khi SignIn l?i
- Khi update database, Claims v?n gi? gi� tr? c?

## Gi?i ph�p

### 1. Backend: C?p nh?t Claims sau khi update Profile

**File:** `Quiz_Web/Controllers/AccountController.cs`

```csharp
[Authorize]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<JsonResult> UpdateProfile(string fullName, string? phone)
{
    try
    {
        var userId = GetCurrentUserId();
        
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return Json(new { status = WebConstants.ERROR, message = "H? v� t�n kh�ng ???c ?? tr?ng" });
        }

        if (_userService.UpdateProfile(userId, fullName, phone))
        {
            // ? C?p nh?t Claims v?i FullName m?i
            var user = _userService.GetUserById(userId);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role?.Name ?? string.Empty)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity)
                );
            }

            // ? Tr? v? fullName m?i ?? update UI
            return Json(new { 
                status = WebConstants.SUCCESS, 
                message = "C?p nh?t h? s? th�nh c�ng", 
                fullName = fullName 
            });
        }
        else
        {
            return Json(new { status = WebConstants.ERROR, message = "Kh�ng th? c?p nh?t h? s?" });
        }
    }
    catch (Exception ex)
    {
        return Json(new { status = WebConstants.ERROR, message = "L?i h? th?ng", error = ex.ToString() });
    }
}
```

### 2. Frontend: Update UI ngay l?p t?c

**File:** `Quiz_Web/wwwroot/js/account-profile.js`

```javascript
$.ajax({
    url: '/Account/UpdateProfile',
    type: 'POST',
    data: {
        fullName: fullName,
        phone: phone,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    },
    success: function(response) {
        if (response.status === 'success' || response.status === 'SUCCESS') {
            Swal.fire({
                icon: 'success',
                title: 'Th�nh c�ng',
                text: 'C?p nh?t h? s? th�nh c�ng',
                confirmButtonText: 'OK'
            }).then(() => {
                // ? Update UI tr??c khi reload
                if (response.fullName) {
                    updateUserNameInUI(response.fullName);
                }
                
                // Reload ?? ??m b?o data ??ng b?
                window.location.reload();
            });
        }
    }
});

// ? Function update UI
function updateUserNameInUI(fullName) {
    // Update t�n trong dropdown
    $('.user-name').text(fullName);
    
    // Update initials trong avatar
    const initials = fullName.split(' ')
        .map(word => word.charAt(0))
        .slice(0, 2)
        .join('')
        .toUpperCase();
    
    $('.user-avatar').text(initials);
    $('.user-avatar-large').text(initials);
}
```

### 3. HTML Structure (?� c� s?n)

**File:** `Quiz_Web/Views/Shared/_Layout.cshtml`

```html
<!-- Avatar nh? trong navbar -->
<div class="user-avatar bg-dark text-white ...">
    TM <!-- Initials -->
</div>

<!-- Dropdown menu -->
<div class="user-avatar-large bg-dark text-white ...">
    TM <!-- Initials -->
</div>
<div class="user-details">
    <div class="user-name">Tr?n Minh Khoa</div>
    <div class="user-email">teacher@learn.vn</div>
</div>
```

## Flow ho?t ??ng

### Khi Update Profile:

1. **User nh?p h? t�n m?i** ? Click "L?u thay ??i"

2. **JavaScript g?i AJAX** ? `/Account/UpdateProfile`

3. **Backend x? l�:**
   - ? Validate input
   - ? C?p nh?t database
   - ? **C?p nh?t Claims (SignIn l?i)** ?? Quan tr?ng!
   - ? Tr? v? JSON v?i `fullName` m?i

4. **JavaScript nh?n response:**
   - ? Hi?n th? SweetAlert success
   - ? **G?i `updateUserNameInUI(fullName)`** ?? Update ngay
   - ? Reload trang

5. **UI ???c c?p nh?t:**
   - ? T�n trong dropdown ?� thay ??i
   - ? Initials trong avatar ?� thay ??i
   - ? Claims ?� ???c refresh

## Testing

### Test Case 1: Update Full Name
```
1. V�o /account/profile
2. Nh?p h? t�n m?i: "Nguy?n V?n A"
3. Click "L?u thay ??i"
4. ? Popup success hi?n "Th�nh c�ng"
5. ? T�n trong dropdown thay ??i th�nh "Nguy?n V?n A"
6. ? Avatar thay ??i th�nh "NV"
7. ? Trang reload, t�n v?n ?�ng
```

### Test Case 2: Update v?i k� t? ??c bi?t
```
1. Nh?p: "Tr?n Minh Khoa B�o"
2. Click "L?u thay ??i"
3. ? Initials: "TM" (l?y 2 ch? ??u)
4. ? T�n ??y ?? hi?n th? ?�ng
```

### Test Case 3: Ki?m tra Claims
```
1. Update h? t�n
2. M? DevTools ? Application ? Cookies
3. Check cookie `.AspNetCore.Cookies`
4. ? Claims ?� ???c c?p nh?t
```

## Debug

### N?u t�n kh�ng thay ??i:

**Check 1: Response c� tr? v? fullName kh�ng?**
```javascript
console.log('Response:', response);
console.log('Full Name:', response.fullName);
```

**Check 2: Function update c� ch?y kh�ng?**
```javascript
function updateUserNameInUI(fullName) {
    console.log('Updating UI with:', fullName);
    console.log('Found elements:', $('.user-name').length);
    // ...
}
```

**Check 3: Claims c� ???c update kh�ng?**
```csharp
// Trong Controller
_logger.LogInformation($"Updated claims for user {user.FullName}");
```

**Check 4: Class names c� ?�ng kh�ng?**
```html
<!-- Ph?i c� c�c class n�y -->
<div class="user-avatar">...</div>
<div class="user-avatar-large">...</div>
<div class="user-name">...</div>
```

## L?u �

### 1. Security
- ? Ph?i c� `[Authorize]` attribute
- ? Ph?i validate `__RequestVerificationToken`
- ? Ph?i ki?m tra userId match v?i current user

### 2. Performance
- Page reload v?n c?n thi?t ??:
  - ??m b?o t?t c? data ??ng b?
  - Load l?i t?t c? components
  - Refresh claims trong UI

### 3. UX Improvement
```javascript
// C� th? delay reload ?? user th?y UI update
updateUserNameInUI(response.fullName);
setTimeout(() => {
    window.location.reload();
}, 800); // 0.8 gi�y
```

## Files ?� thay ??i

1. ? `Quiz_Web/Controllers/AccountController.cs`
   - Th�m code c?p nh?t Claims
   - Tr? v? fullName trong response

2. ? `Quiz_Web/wwwroot/js/account-profile.js`
   - Th�m function `updateUserNameInUI()`
   - G?i function tr??c khi reload

3. ?? `Quiz_Web/Views/Shared/_Layout.cshtml`
   - ?� c� s?n class names ph� h?p
   - Kh�ng c?n s?a

## Related

- `Quiz_Web/wwwroot/js/account-settings.js` - Settings page (t??ng t?)
- `Quiz_Web/Controllers/AccountController.cs` - UpdateEmail, UpdatePassword
- `Quiz_Web/Views/Account/Profile.cshtml` - Profile form
- `Quiz_Web/Views/Shared/_Layout.cshtml` - User dropdown

## Future Enhancements

### 1. Update without reload (WebSocket/SignalR)
```javascript
// Real-time update across all tabs
connection.on("ProfileUpdated", function (fullName) {
    updateUserNameInUI(fullName);
});
```

### 2. Avatar Image Support
```html
<img src="@user.AvatarUrl" class="user-avatar" />
```

### 3. Animation
```javascript
$('.user-name').fadeOut(200, function() {
    $(this).text(fullName).fadeIn(200);
});
```

## Conclusion

Gi?i ph�p n�y ??m b?o:
- ? Update Claims ngay sau khi thay ??i database
- ? UI update ngay l?p t?c (tr??c khi reload)
- ? Kh�ng c?n logout/login l?i
- ? User experience m??t m�
- ? Data consistency ???c ??m b?o
