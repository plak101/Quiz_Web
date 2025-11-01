# C?p nh?t Real-time Tên ng??i dùng trong Dropdown

## V?n ??
Khi c?p nh?t h? tên trong trang `/account/profile`, tên trong dropdown menu không thay ??i cho ??n khi reload trang ho?c logout/login l?i.

## Nguyên nhân
- Tên ng??i dùng trong dropdown ???c l?y t? `User.Identity.Name` (Claims)
- Claims ???c set khi ??ng nh?p và ch? thay ??i khi SignIn l?i
- Khi update database, Claims v?n gi? giá tr? c?

## Gi?i pháp

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
            return Json(new { status = WebConstants.ERROR, message = "H? và tên không ???c ?? tr?ng" });
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
                message = "C?p nh?t h? s? thành công", 
                fullName = fullName 
            });
        }
        else
        {
            return Json(new { status = WebConstants.ERROR, message = "Không th? c?p nh?t h? s?" });
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
                title: 'Thành công',
                text: 'C?p nh?t h? s? thành công',
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
    // Update tên trong dropdown
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

### 3. HTML Structure (?ã có s?n)

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

1. **User nh?p h? tên m?i** ? Click "L?u thay ??i"

2. **JavaScript g?i AJAX** ? `/Account/UpdateProfile`

3. **Backend x? lý:**
   - ? Validate input
   - ? C?p nh?t database
   - ? **C?p nh?t Claims (SignIn l?i)** ?? Quan tr?ng!
   - ? Tr? v? JSON v?i `fullName` m?i

4. **JavaScript nh?n response:**
   - ? Hi?n th? SweetAlert success
   - ? **G?i `updateUserNameInUI(fullName)`** ?? Update ngay
   - ? Reload trang

5. **UI ???c c?p nh?t:**
   - ? Tên trong dropdown ?ã thay ??i
   - ? Initials trong avatar ?ã thay ??i
   - ? Claims ?ã ???c refresh

## Testing

### Test Case 1: Update Full Name
```
1. Vào /account/profile
2. Nh?p h? tên m?i: "Nguy?n V?n A"
3. Click "L?u thay ??i"
4. ? Popup success hi?n "Thành công"
5. ? Tên trong dropdown thay ??i thành "Nguy?n V?n A"
6. ? Avatar thay ??i thành "NV"
7. ? Trang reload, tên v?n ?úng
```

### Test Case 2: Update v?i ký t? ??c bi?t
```
1. Nh?p: "Tr?n Minh Khoa Béo"
2. Click "L?u thay ??i"
3. ? Initials: "TM" (l?y 2 ch? ??u)
4. ? Tên ??y ?? hi?n th? ?úng
```

### Test Case 3: Ki?m tra Claims
```
1. Update h? tên
2. M? DevTools ? Application ? Cookies
3. Check cookie `.AspNetCore.Cookies`
4. ? Claims ?ã ???c c?p nh?t
```

## Debug

### N?u tên không thay ??i:

**Check 1: Response có tr? v? fullName không?**
```javascript
console.log('Response:', response);
console.log('Full Name:', response.fullName);
```

**Check 2: Function update có ch?y không?**
```javascript
function updateUserNameInUI(fullName) {
    console.log('Updating UI with:', fullName);
    console.log('Found elements:', $('.user-name').length);
    // ...
}
```

**Check 3: Claims có ???c update không?**
```csharp
// Trong Controller
_logger.LogInformation($"Updated claims for user {user.FullName}");
```

**Check 4: Class names có ?úng không?**
```html
<!-- Ph?i có các class này -->
<div class="user-avatar">...</div>
<div class="user-avatar-large">...</div>
<div class="user-name">...</div>
```

## L?u ý

### 1. Security
- ? Ph?i có `[Authorize]` attribute
- ? Ph?i validate `__RequestVerificationToken`
- ? Ph?i ki?m tra userId match v?i current user

### 2. Performance
- Page reload v?n c?n thi?t ??:
  - ??m b?o t?t c? data ??ng b?
  - Load l?i t?t c? components
  - Refresh claims trong UI

### 3. UX Improvement
```javascript
// Có th? delay reload ?? user th?y UI update
updateUserNameInUI(response.fullName);
setTimeout(() => {
    window.location.reload();
}, 800); // 0.8 giây
```

## Files ?ã thay ??i

1. ? `Quiz_Web/Controllers/AccountController.cs`
   - Thêm code c?p nh?t Claims
   - Tr? v? fullName trong response

2. ? `Quiz_Web/wwwroot/js/account-profile.js`
   - Thêm function `updateUserNameInUI()`
   - G?i function tr??c khi reload

3. ?? `Quiz_Web/Views/Shared/_Layout.cshtml`
   - ?ã có s?n class names phù h?p
   - Không c?n s?a

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

Gi?i pháp này ??m b?o:
- ? Update Claims ngay sau khi thay ??i database
- ? UI update ngay l?p t?c (tr??c khi reload)
- ? Không c?n logout/login l?i
- ? User experience m??t mà
- ? Data consistency ???c ??m b?o
