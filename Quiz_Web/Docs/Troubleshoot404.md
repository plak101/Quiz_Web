# Quick Fix: L?i 404 trang /Onboarding

## Checklist ki?m tra nhanh

### 1. Ki?m tra c?u tr�c th? m?c
??m b?o file t?n t?i ?�ng v? tr�:
```
Quiz_Web/
??? Controllers/
?   ??? OnboardingController.cs ?
??? Views/
?   ??? Onboarding/
?       ??? Index.cshtml ?
??? Models/
    ??? ViewModels/
        ??? OnboardingViewModel.cs ?
```

### 2. Restart ?ng d?ng
- Stop ?ng d?ng (Shift + F5)
- Clean Solution (Build ? Clean Solution)
- Rebuild Solution (Ctrl + Shift + B)
- Run l?i (F5)

### 3. Th? c�c URL sau
M? browser v� test t?ng URL:
- `https://localhost:7158/Onboarding`
- `https://localhost:7158/Onboarding/Index`
- `https://localhost:7158/onboarding`
- `https://localhost:7158/onboarding/index`

### 4. Ki?m tra routing trong Program.cs
File `Program.cs` ph?i c�:
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}")
    .WithStaticAssets();
```

### 5. Ki?m tra Controller kh�ng c� l?i
M? file `OnboardingController.cs`, x�c nh?n:
- Namespace: `Quiz_Web.Controllers` ?
- Class public: `public class OnboardingController : Controller` ?
- Action public: `public async Task<IActionResult> Index()` ?

### 6. Test tr?c ti?p b?ng code
Th�m action test v�o `HomeController.cs`:

```csharp
[Route("/test-onboarding")]
public IActionResult TestOnboarding()
{
    return RedirectToAction("Index", "Onboarding");
}
```

Sau ?� truy c?p: `https://localhost:7158/test-onboarding`

### 7. Ki?m tra logs
M? **Output** window trong Visual Studio (View ? Output):
- Ch?n "ASP.NET Core Web Server"
- Xem c� error log g� kh�ng khi truy c?p /Onboarding

### 8. Th? t?o route t??ng minh
N?u v?n kh�ng ???c, th�m route attribute v�o controller:

```csharp
[Route("onboarding")]
public class OnboardingController : Controller
{
    [Route("")]
    [Route("index")]
    public async Task<IActionResult> Index()
    {
        // ...
    }
}
```

### 9. Ki?m tra appsettings.json
X�c nh?n connection string ?�ng:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1434;Initial Catalog=LearningPlatform;..."
  }
}
```

### 10. Test v?i action ??n gi?n
Thay th? t?m th?i method Index() trong OnboardingController:

```csharp
public IActionResult Index()
{
    return Content("Onboarding page is working!");
}
```

N?u hi?n th? text "Onboarding page is working!" ? View c� v?n ??
N?u v?n 404 ? Routing c� v?n ??

---

## Gi?i ph�p t?m th?i: Test v?i HomeController

N?u v?n kh�ng ???c, th�m action v�o `HomeController.cs` ?? test:

```csharp
[Route("/onboarding-test")]
public async Task<IActionResult> OnboardingTest()
{
    var categories = await _context.CourseCategories.ToListAsync();
    var viewModel = new OnboardingViewModel
    {
        Categories = categories
    };
    return View("~/Views/Onboarding/Index.cshtml", viewModel);
}
```

Sau ?� truy c?p: `https://localhost:7158/onboarding-test`

---

## Ki?m tra terminal logs

Khi ch?y ?ng d?ng, ki?m tra Output window ho?c Terminal, t�m d�ng:
```
Now listening on: https://localhost:7158
```

N?u kh�ng th?y ho?c c� l?i, ki?m tra:
1. Port c� b? conflict kh�ng
2. Certificate SSL c� h?p l? kh�ng
3. Firewall c� block kh�ng

---

## N?u t?t c? ??u fail

Th? t?o l?i controller v� view t? ??u:
1. X�a `OnboardingController.cs`
2. Add New Item ? Controller ? Empty Controller
3. ??t t�n: `OnboardingController`
4. Copy l?i code
5. Rebuild solution

---

H�y th? t?ng b??c tr�n v� b�o l?i k?t qu?!
