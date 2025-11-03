# H??ng D?n T?o Flashcard và Test Trong Course Builder

## T?ng Quan

Tính n?ng m?i này cho phép gi?ng viên t?o **Flashcard Sets** và **Tests** tr?c ti?p trong quá trình xây d?ng khóa h?c, không c?n ph?i t?o riêng bi?t và sau ?ó liên k?t.

## Các Tính N?ng Chính

### 1. T?o Flashcard Set

Khi t?o n?i dung bài h?c và ch?n lo?i "Flashcard", b?n có th?:

#### Thông tin Flashcard Set:
- **Tiêu ??**: Tên c?a b? flashcard
- **Mô t?**: Mô t? ng?n v? b? flashcard

#### Qu?n lý Flashcards:
- **Thêm th?**: Click nút "Thêm th? flashcard" ?? t?o th? m?i
- **M?t tr??c**: Câu h?i ho?c thu?t ng?
- **M?t sau**: Câu tr? l?i ho?c ??nh ngh?a
- **G?i ý** (tùy ch?n): G?i ý giúp h?c viên
- **Xóa th?**: Click icon thùng rác ?? xóa th?

#### Tính n?ng:
- T? ??ng l?u v? trí th? (OrderIndex)
- Hi?n th? s? th? t? th?
- Cu?n trang n?u có nhi?u th?
- Visibility m?c ??nh: "Course" (ch? dành cho khóa h?c)

### 2. T?o Test (Bài Ki?m Tra)

Khi t?o n?i dung bài h?c và ch?n lo?i "Test", b?n có th?:

#### Thông tin Test:
- **Tiêu ??**: Tên bài test
- **Mô t?**: Mô t? v? bài test
- **Th?i gian** (phút): Gi?i h?n th?i gian làm bài (m?c ??nh: 30 phút)
- **S? l?n làm t?i ?a**: S? l?n h?c viên ???c làm l?i (m?c ??nh: 3 l?n)

#### Qu?n lý Câu H?i:

##### Lo?i câu h?i h? tr?:
1. **Tr?c nghi?m (1 ?áp án)** - MCQ_Single
   - Ch? 1 ?áp án ?úng
   - T? ??ng b? ch?n các ?áp án khác khi ch?n ?áp án ?úng

2. **Tr?c nghi?m (nhi?u ?áp án)** - MCQ_Multi
   - Nhi?u ?áp án ?úng
   - H?c viên ph?i ch?n t?t c? ?áp án ?úng

3. **?úng/Sai** - TrueFalse
   - Câu h?i ch? có 2 l?a ch?n: ?úng ho?c Sai

##### Thao tác v?i câu h?i:
- **Thêm câu h?i**: Click "Thêm câu h?i"
- **Câu h?i**: Nh?p n?i dung câu h?i
- **?i?m**: S? ?i?m cho câu h?i (m?c ??nh: 1 ?i?m)
- **Thêm ?áp án**: M?i câu h?i có th? có nhi?u ?áp án
- **Ch?n ?áp án ?úng**: Tick checkbox bên c?nh ?áp án
- **Xóa câu h?i/?áp án**: Click icon thùng rác

#### Tính n?ng:
- M?i câu h?i ph?i có ít nh?t 2 ?áp án
- T? ??ng tính toán ?i?m t?i ?a (MaxScore)
- GradingMode: "Auto" (ch?m ?i?m t? ??ng)
- Visibility: "Course" (ch? dành cho khóa h?c)

## Quy Trình T?o Khóa H?c V?i Flashcard/Test

### B??c 1: Thông tin khóa h?c
Nh?p thông tin c? b?n nh? tiêu ??, mô t?, giá, ?nh bìa.

### B??c 2: C?u trúc n?i dung
T?o các ch??ng và bài h?c.

### B??c 3: N?i dung bài h?c
1. Ch?n bài h?c c?n thêm n?i dung
2. Click "Thêm n?i dung m?i"
3. Ch?n lo?i n?i dung: **FlashcardSet** ho?c **Test**
4. Nh?p thông tin và t?o flashcards/questions
5. L?u và chuy?n sang bài h?c khác

### B??c 4: Xem tr??c & Xu?t b?n
Ki?m tra l?i toàn b? khóa h?c và xu?t b?n.

## L?u Ý Quan Tr?ng

### Flashcard Set:
- ? M?i th? ph?i có c? m?t tr??c và m?t sau
- ? G?i ý là tùy ch?n
- ? FlashcardSet ???c t?o t? ??ng v?i Visibility="Course"
- ? Không th? truy c?p t? trang Flashcard chung (ch? trong khóa h?c)

### Test:
- ? M?i câu h?i ph?i có ít nh?t 2 ?áp án
- ? Ph?i có ít nh?t 1 ?áp án ?úng
- ? Test ???c t?o t? ??ng v?i Visibility="Course"
- ? Ch?m ?i?m t? ??ng (GradingMode="Auto")
- ? Không th? truy c?p t? trang Test chung (ch? trong khóa h?c)

### Khi C?p Nh?t Khóa H?c:
- ?? FlashcardSets và Tests c? s? b? xóa m?m (IsDeleted=true)
- ?? FlashcardSets và Tests m?i s? ???c t?o l?i
- ?? D? li?u luy?n t?p c?a h?c viên v?n ???c gi? nguyên

## Ki?n Trúc D? Li?u

```
Course
  ?? CourseChapter
      ?? Lesson
          ?? LessonContent
              ?? ContentType = "FlashcardSet"
              ?   ?? RefId ? FlashcardSet
              ?       ?? Flashcards
              ?
              ?? ContentType = "Test"
              ?   ?? RefId ? Test
              ?       ?? Questions
              ?           ?? QuestionOptions
              ?
              ?? ContentType = "Theory"
              ?   ?? Body (HTML content)
              ?
              ?? ContentType = "Video"
                  ?? VideoUrl
```

## Các File Liên Quan

### Frontend:
- `wwwroot/js/course-builder.js` - Logic JavaScript
- `wwwroot/css/course/course-builder.css` - Styles

### Backend:
- `Models/ViewModels/CourseBuilderViewModels.cs` - ViewModels
- `Services/CourseService.cs` - Business logic
- `Controllers/CourseController.cs` - API endpoints

### Database:
- `FlashcardSets` & `Flashcards` tables
- `Tests`, `Questions`, `QuestionOptions` tables
- `LessonContents` table (RefId links)

## Tips & Best Practices

### Flashcards:
1. **Ng?n g?n**: M?t tr??c và sau nên ng?n g?n, d? nh?
2. **Hình ?nh**: Có th? s? d?ng emoji ho?c ký t? ??c bi?t
3. **G?i ý**: S? d?ng g?i ý ?? giúp h?c viên ghi nh? t?t h?n
4. **S? l??ng**: 10-20 th? cho m?i b? là lý t??ng

### Tests:
1. **Câu h?i rõ ràng**: Tránh câu h?i m? h?
2. **?áp án h?p lý**: T?t c? ?áp án nên có kh? n?ng ?úng (avoid obvious wrong answers)
3. **Phân b? ?i?m**: Câu khó nên có ?i?m cao h?n
4. **Th?i gian**: 1-2 phút/câu là h?p lý

## Kh?c Ph?c S? C?

### Không th?y nút "Thêm th? flashcard":
- ??m b?o ?ã ch?n lo?i n?i dung là "Flashcard"
- Refresh trang và th? l?i

### Không th? thêm ?áp án:
- ??m b?o câu h?i ?ã ???c t?o
- M?i câu h?i ph?i có ít nh?t 2 ?áp án

### D? li?u b? m?t khi chuy?n bài h?c:
- H? th?ng t? ??ng l?u khi b?n thay ??i n?i dung
- Ki?m tra l?i tr??c khi chuy?n sang Step 4

### L?i khi l?u khóa h?c:
- Ki?m tra t?t c? flashcards ph?i có m?t tr??c và m?t sau
- Ki?m tra t?t c? câu h?i ph?i có ít nh?t 1 ?áp án ?úng
- Xem Console (F12) ?? bi?t thông tin chi ti?t

## H? Tr?

N?u g?p v?n ??, vui lòng ki?m tra:
1. Console Browser (F12 ? Console tab)
2. Network tab ?? xem API requests
3. Server logs ?? xem chi ti?t l?i

---

**Version**: 1.0.0  
**Last Updated**: 2024-01-20  
**Author**: Quiz_Web Development Team
