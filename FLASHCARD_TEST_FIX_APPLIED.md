# Flashcard và Test - ?ã S?a Xong ?

## Tóm t?t
?ã s?a thành công l?i Flashcard và Test không ???c l?u vào database trong Course Builder.

## Các thay ??i ?ã th?c hi?n

### 1. ? S?a hàm `saveStepData()` - Step 2
**V?n ??:** Contents b? m?t khi rebuild DOM ? step 2
**Gi?i pháp:** S? d?ng deep clone (JSON.parse/JSON.stringify) thay vì reference tr?c ti?p

```javascript
// ? L?U L?I T?T C? CONTENTS TR??C KHI C?P NH?T
const oldContentsMap = new Map();
courseData.chapters.forEach((chapter, chIdx) => {
    chapter.lessons.forEach((lesson, lIdx) => {
        const key = `${chIdx}_${lIdx}`;
        if (lesson.contents && lesson.contents.length > 0) {
            // Clone deep ?? tránh m?t reference
            oldContentsMap.set(key, JSON.parse(JSON.stringify(lesson.contents)));
        }
    });
});
```

### 2. ? S?a hàm `saveStepData()` - Step 3
**V?n ??:** Ch? l?u Theory, không l?u Flashcard và Test
**Gi?i pháp:** Thêm logic ??y ?? ?? l?u t?t c? content types

```javascript
if (currentStep === 3) {
    const currentLesson = window.currentLesson;
    if (currentLesson) {
        const lesson = courseData.chapters[currentLesson.chapterIndex].lessons[currentLesson.lessonIndex];

        // L?u t?t c? contents t? DOM
        lesson.contents.forEach((content, index) => {
            // L?u tiêu ?? và lo?i n?i dung
            
            // L?u n?i dung theo lo?i
            if (content.contentType === 'Theory') {
                // L?u t? CKEditor
            } else if (content.contentType === 'Video') {
                // Video URL ?ã ???c l?u trong handleVideoUpload
            } else if (content.contentType === 'FlashcardSet') {
                // L?u flashcard set title, description và t?ng flashcard
            } else if (content.contentType === 'Test') {
                // L?u test title, description, time, attempts và t?ng question v?i options
            }
        });
    }
}
```

### 3. ? Thêm hàm m?i `saveCurrentLessonContents()`
**M?c ?ích:** L?u d? li?u c?a lesson hi?n t?i tr??c khi chuy?n sang lesson khác

```javascript
function saveCurrentLessonContents() {
    const currentLesson = window.currentLesson;
    if (!currentLesson) return;

    const lesson = courseData.chapters[currentLesson.chapterIndex].lessons[currentLesson.lessonIndex];

    // L?u t?t c? contents t? DOM (Theory, Video, Flashcard, Test)
}
```

### 4. ? C?p nh?t các hàm ?i?u h??ng

#### 4.1. `nextStep()`
```javascript
async function nextStep() {
    // ? L?U D? LI?U C?A LESSON HI?N T?I N?U ?ANG ? STEP 3
    if (currentStep === 3 && window.currentLesson) {
        saveCurrentLessonContents();
    }
    // ...existing code...
}
```

#### 4.2. `prevStep()`
```javascript
function prevStep() {
    // ? L?U D? LI?U C?A LESSON HI?N T?I N?U ?ANG ? STEP 3
    if (currentStep === 3 && window.currentLesson) {
        saveCurrentLessonContents();
    }
    // ...existing code...
}
```

#### 4.3. `populateLessonSelector()`
```javascript
function populateLessonSelector() {
    // ? L?U D? LI?U C?A LESSON HI?N T?I TR??C KHI CHUY?N
    if (window.currentLesson) {
        saveCurrentLessonContents();
    }
    // ...existing code...
}
```

#### 4.4. `loadLessonContents()`
```javascript
function loadLessonContents() {
    // ? L?U D? LI?U C?A LESSON HI?N T?I TR??C KHI CHUY?N
    if (window.currentLesson) {
        saveCurrentLessonContents();
    }
    // ...existing code...
}
```

#### 4.5. `saveCourse()`
```javascript
function saveCourse(publish) {
    // ? L?U D? LI?U C?A LESSON HI?N T?I N?U CÓ
    if (window.currentLesson) {
        saveCurrentLessonContents();
    }
    // ...existing code...
}
```

## Cách ho?t ??ng

### Lu?ng l?u d? li?u Flashcard/Test

1. **Ng??i dùng nh?p d? li?u**
   - Nh?p flashcard cards (front, back, hint)
   - Nh?p test questions, options, ?i?m s?

2. **Khi chuy?n lesson ho?c step**
   - G?i `saveCurrentLessonContents()`
   - ??c t?t c? d? li?u t? DOM
   - L?u vào `courseData` object

3. **Khi submit form**
   - G?i `saveCurrentLessonContents()` tr??c
   - G?i `saveStepData()` ?? finalize
   - Convert `courseData` sang JSON
   - Submit form lên server

### Deep Clone trong Step 2

**T?i sao c?n?**
- Step 2 rebuild DOM t? ??u khi ??c chapters/lessons
- N?u ch? copy reference, contents s? b? clear khi rebuild
- Deep clone t?o b?n sao hoàn toàn ??c l?p

**Cách ho?t ??ng:**
```javascript
// ? SAI: Ch? copy reference
oldContentsMap.set(key, lesson.contents);

// ? ?ÚNG: Deep clone
oldContentsMap.set(key, JSON.parse(JSON.stringify(lesson.contents)));
```

## Ki?m tra ch?c n?ng

### Test Case 1: Flashcard Set
1. T?o khóa h?c v?i 1 ch??ng, 2 bài h?c
2. Bài h?c 1: Thêm Flashcard set v?i 3 th?
3. Chuy?n sang bài h?c 2
4. Quay l?i bài h?c 1
5. **K? v?ng:** 3 th? flashcard v?n còn nguyên

### Test Case 2: Test
1. T?o khóa h?c v?i 1 ch??ng, 1 bài h?c
2. Thêm Test v?i 2 câu h?i, m?i câu 4 ?áp án
3. Chuy?n qua Step 4 (Preview)
4. Quay l?i Step 3
5. **K? v?ng:** T?t c? questions và options v?n còn

### Test Case 3: L?u khóa h?c
1. T?o khóa h?c hoàn ch?nh v?i Flashcard và Test
2. Click "L?u khóa h?c"
3. Ki?m tra database
4. **K? v?ng:** 
   - FlashcardSets table có d? li?u
   - Flashcards table có d? li?u
   - Tests table có d? li?u
   - Questions và Options table có d? li?u

## L?i ?ã s?a

### ? Tr??c khi s?a
- Flashcard và Test không ???c l?u vào database
- D? li?u b? m?t khi chuy?n ??i gi?a lessons
- Ch? l?u ???c Theory content

### ? Sau khi s?a
- T?t c? content types ??u ???c l?u ??y ??
- D? li?u ???c b?o toàn khi chuy?n ??i
- Deep clone ??m b?o không m?t d? li?u ? step 2

## Build Status
? **Build successful** - Không có l?i compilation

## Files Changed
- `Quiz_Web/wwwroot/js/course-builder.js`

## Commit Message G?i Ý
```
fix: Flashcard và Test không ???c l?u vào database

- S? d?ng deep clone trong step 2 ?? b?o toàn contents
- Thêm logic ??y ?? cho Flashcard và Test trong step 3
- Thêm hàm saveCurrentLessonContents() ?? l?u tr??c khi chuy?n lesson
- C?p nh?t t?t c? navigation functions ?? g?i save tr??c khi chuy?n

Fixes #[issue-number]
```

## Ghi chú quan tr?ng

1. **Không dùng `...existing code...` trong production**
   - Code ?ã ???c áp d?ng ??y ??
   - Không còn placeholder nào

2. **Deep clone gi?i h?n**
   - `JSON.parse(JSON.stringify())` không clone functions
   - ?? cho tr??ng h?p này vì ch? l?u plain objects

3. **Performance**
   - `saveCurrentLessonContents()` ch? ch?y khi c?n thi?t
   - Không ?nh h??ng ??n performance chung

## Rollback (n?u c?n)
```bash
git checkout Quiz_Web/wwwroot/js/course-builder.js
```

---
**Ngày s?a:** 2025-01-XX
**Ng??i s?a:** GitHub Copilot
**Status:** ? COMPLETED
