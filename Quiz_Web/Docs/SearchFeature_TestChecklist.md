# ? Test Checklist - Thanh Tìm Ki?m

## Quick Test (2 phút)

### Test 1: Click Icon Search
- [ ] M? trang ch?
- [ ] Nh?p "javascript" vào search box
- [ ] **Click vào icon ??**
- [ ] ? Chuy?n ??n `/courses/search?q=javascript`
- [ ] ? Hi?n th? k?t qu? tìm ki?m

### Test 2: Enter Key
- [ ] M? trang ch?
- [ ] Nh?p "python" vào search box
- [ ] **Nh?n Enter**
- [ ] ? Chuy?n ??n `/courses/search?q=python`
- [ ] ? Hi?n th? k?t qu? tìm ki?m

### Test 3: Visual Feedback
- [ ] Di chu?t vào icon search
- [ ] ? Icon chuy?n màu tím (#5624d0)
- [ ] ? Cursor chuy?n thành pointer (bàn tay)
- [ ] ? Tooltip "Tìm ki?m" hi?n th?

### Test 4: Empty Search
- [ ] ?? tr?ng search box
- [ ] Click icon search
- [ ] ? Redirect v? `/courses` (t?t c? khóa h?c)

### Test 5: Responsive
- [ ] **Desktop (>1200px)**: Search bar full width
- [ ] **Tablet (768-1199px)**: Search bar v?n hi?n th?
- [ ] **Mobile (<768px)**: Search bar trong menu collapse

## Detailed Test (5 phút)

### Functional Tests

#### ? Search v?i t? khóa h?p l?
```
Input: "web development"
Expected: /courses/search?q=web+development
Result: [ ] PASS / [ ] FAIL
```

#### ? Search v?i ký t? ??c bi?t
```
Input: "c++ programming"
Expected: /courses/search?q=c%2B%2B+programming
Result: [ ] PASS / [ ] FAIL
```

#### ? Search v?i s?
```
Input: "html5"
Expected: /courses/search?q=html5
Result: [ ] PASS / [ ] FAIL
```

### UI/UX Tests

#### Hover States
- [ ] Icon search: Normal (#6a6f73)
- [ ] Icon search: Hover (#5624d0)
- [ ] Icon search: Active (#401b9c)
- [ ] Search box: Focus border thicker

#### Layout Tests
- [ ] Icon không b? crop
- [ ] Input field không b? overlap
- [ ] Border radius 50px (rounded)
- [ ] Height = 48px

### Browser Tests

#### Chrome/Edge
- [ ] Click ho?t ??ng
- [ ] Enter ho?t ??ng
- [ ] Hover effect smooth

#### Firefox
- [ ] Click ho?t ??ng
- [ ] Enter ho?t ??ng
- [ ] Hover effect smooth

#### Safari
- [ ] Click ho?t ??ng
- [ ] Enter ho?t ??ng
- [ ] Hover effect smooth

### Mobile Tests

#### iOS Safari
- [ ] Touch click ho?t ??ng
- [ ] Keyboard xu?t hi?n khi focus
- [ ] Submit ho?t ??ng

#### Android Chrome
- [ ] Touch click ho?t ??ng
- [ ] Keyboard xu?t hi?n khi focus
- [ ] Submit ho?t ??ng

## Regression Tests

### Header Navigation
- [ ] Logo link ho?t ??ng
- [ ] "Khám phá" dropdown ho?t ??ng
- [ ] "Gi?ng viên" link ho?t ??ng
- [ ] User dropdown ho?t ??ng
- [ ] Cart dropdown ho?t ??ng

### Search không ?nh h??ng ??n
- [ ] Sticky navbar
- [ ] Responsive menu
- [ ] Dropdown menus
- [ ] Other input fields

## Performance Tests

### Load Time
- [ ] CSS load < 100ms
- [ ] No blocking JavaScript
- [ ] Smooth transitions (0.2s)

### Memory
- [ ] No memory leaks
- [ ] No console errors
- [ ] Clean network tab

## Accessibility Tests

### Keyboard Navigation
- [ ] Tab ??n search input
- [ ] Tab ??n search button
- [ ] Enter submit form
- [ ] Esc clear input (optional)

### Screen Reader
- [ ] Aria label ??c ?úng
- [ ] Tooltip announce
- [ ] Form role correct

## Security Tests

### XSS Prevention
- [ ] Input: `<script>alert('xss')</script>`
- [ ] Expected: Escaped trong URL
- [ ] No script execution

### SQL Injection
- [ ] Input: `'; DROP TABLE Courses;--`
- [ ] Expected: Treated as string
- [ ] No database error

## Edge Cases

### Long Input
```
Input: "Lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt"
Expected: Accept và search
Result: [ ] PASS / [ ] FAIL
```

### Unicode Characters
```
Input: "????"
Expected: /courses/search?q=????
Result: [ ] PASS / [ ] FAIL
```

### Empty Spaces
```
Input: "   javascript   "
Expected: Trim và search "javascript"
Result: [ ] PASS / [ ] FAIL
```

## Sign-off

### Test Summary
- Total Tests: ____ 
- Passed: ____
- Failed: ____
- Skipped: ____

### Issues Found
1. _____________________
2. _____________________
3. _____________________

### Tester Information
- Name: _____________________
- Date: _____________________
- Environment: _____________________
- Browser: _____________________

### Approval
- [ ] All critical tests passed
- [ ] No major bugs found
- [ ] Ready for production
- [ ] Documentation updated

**Signature**: _________________ **Date**: _________________

---

## Quick Commands

### Clear Browser Cache
```
Chrome: Ctrl+Shift+Delete
Firefox: Ctrl+Shift+Delete
```

### Hard Reload
```
Windows: Ctrl+F5
Mac: Cmd+Shift+R
```

### Open DevTools
```
F12 or Ctrl+Shift+I
```

### Check Console
```
F12 ? Console tab
Look for errors (red text)
```
