# Quick Fix: L?i 404 trang /Onboarding

## Checklist ki?m tra nhanh

### 1. Ki?m tra c?u trúc th? m?c
??m b?o file t?n t?i ?úng v? trí:
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

### 3. Th? các URL sau
M? browser và test t?ng URL:
- `https://localhost:7158/Onboarding`
- `https://localhost:7158/Onboarding/Index`
- `https://localhost:7158/onboarding`
- `https://localhost:7158/onboarding/index`

### 4. Ki?m tra routing trong Program.cs
File `Program.cs` ph?i có:
```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Welcome}/{id?}")
    .WithStaticAssets();
```

### 5. Ki?m tra Controller không có l?i
M? file `OnboardingController.cs`, xác nh?n:
- Namespace: `Quiz_Web.Controllers` ?
- Class public: `public class OnboardingController : Controller` ?
- Action public: `public async Task<IActionResult> Index()` ?

### 6. Test tr?c ti?p b?ng code
Thêm action test vào `HomeController.cs`:

```csharp
[Route("/test-onboarding")]
public IActionResult TestOnboarding()
{
    return RedirectToAction("Index", "Onboarding");
}
```

Sau ?ó truy c?p: `https://localhost:7158/test-onboarding`

### 7. Ki?m tra logs
M? **Output** window trong Visual Studio (View ? Output):
- Ch?n "ASP.NET Core Web Server"
- Xem có error log gì không khi truy c?p /Onboarding

### 8. Th? t?o route t??ng minh
N?u v?n không ???c, thêm route attribute vào controller:

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
Xác nh?n connection string ?úng:
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

N?u hi?n th? text "Onboarding page is working!" ? View có v?n ??
N?u v?n 404 ? Routing có v?n ??

---

## Gi?i pháp t?m th?i: Test v?i HomeController

N?u v?n không ???c, thêm action vào `HomeController.cs` ?? test:

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

Sau ?ó truy c?p: `https://localhost:7158/onboarding-test`

---

## Ki?m tra terminal logs

Khi ch?y ?ng d?ng, ki?m tra Output window ho?c Terminal, tìm dòng:
```
Now listening on: https://localhost:7158
```

N?u không th?y ho?c có l?i, ki?m tra:
1. Port có b? conflict không
2. Certificate SSL có h?p l? không
3. Firewall có block không

---

## N?u t?t c? ??u fail

Th? t?o l?i controller và view t? ??u:
1. Xóa `OnboardingController.cs`
2. Add New Item ? Controller ? Empty Controller
3. ??t tên: `OnboardingController`
4. Copy l?i code
5. Rebuild solution

---

Hãy th? t?ng b??c trên và báo l?i k?t qu?!
