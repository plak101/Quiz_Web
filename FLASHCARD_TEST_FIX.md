# H??ng d?n s?a l?i Flashcard và Test trong Course Builder

## V?n ??
- Flashcard và Test không ???c l?u vào database
- D? li?u b? m?t khi chuy?n ??i gi?a các b??c ho?c lessons

## Nguyên nhân
1. Hàm `saveStepData()` ? step 3 ch? l?u n?i dung lý thuy?t (Theory), không l?u Flashcard và Test
2. Không có c? ch? l?u d? li?u khi chuy?n ??i gi?a các lessons trong step 3
3. Hàm `saveStepData()` ? step 2 xóa m?t toàn b? contents khi rebuild DOM

## Gi?i pháp

### 1. S?a hàm `saveStepData()` - PH?N STEP 2
Thay th? ?o?n code:
```javascript
if (currentStep === 2) {
    // ? L?U L?I T?T C? CONTENTS TR??C KHI XÓA
    const oldContentsMap = new Map();
    courseData.chapters.forEach((chapter, chIdx) => {
        chapter.lessons.forEach((lesson, lIdx) => {
            const key = `${chIdx}_${lIdx}`;
            if (lesson.contents && lesson.contents.length > 0) {
                oldContentsMap.set(key, lesson.contents);
            }
        });
    });
```

Thành:
```javascript
if (currentStep === 2) {
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

### 2. S?a hàm `saveStepData()` - PH?N STEP 3
Thay th? toàn b? ph?n step 3 (t? dòng `// ? THÊM LOGIC CHO STEP 3` ??n h?t function) b?ng code sau:

```javascript
// ? THÊM LOGIC CHO STEP 3: L?U N?I DUNG T? DOM VÀ CKEDITOR
if (currentStep === 3) {
    const currentLesson = window.currentLesson;
    if (currentLesson) {
        const lesson = courseData.chapters[currentLesson.chapterIndex].lessons[currentLesson.lessonIndex];

        // L?u t?t c? contents t? DOM
        lesson.contents.forEach((content, index) => {
            // L?u tiêu ??
            const titleInput = document.querySelector(`.content-item[data-content-index="${index}"] .content-title-input`);
            if (titleInput) {
                content.title = titleInput.value.trim();
            }

            // L?u lo?i n?i dung
            const typeSelect = document.querySelector(`.content-item[data-content-index="${index}"] .content-type-select`);
            if (typeSelect) {
                content.contentType = typeSelect.value;
            }

            // L?u n?i dung theo lo?i
            if (content.contentType === 'Theory') {
                // L?u n?i dung lý thuy?t t? CKEditor
                // Tìm CKEditor instance theo pattern contentBody_*_index
                for (const [key, editor] of Object.entries(ckEditorInstances)) {
                    if (key.includes(`contentBody_`) && key.includes(`_${index}`)) {
                        content.body = editor.getData();
                        break;
                    }
                }
            } else if (content.contentType === 'Video') {
                // Video URL ?ã ???c l?u trong handleVideoUpload
                // Không c?n làm gì thêm
            } else if (content.contentType === 'FlashcardSet') {
                // L?u flashcard set title và description
                const setTitleInput = document.querySelector(`.content-item[data-content-index="${index}"] .flashcard-set-title`);
                const setDescInput = document.querySelector(`.content-item[data-content-index="${index}"] .flashcard-set-desc`);
                
                if (setTitleInput) {
                    content.flashcardSetTitle = setTitleInput.value.trim();
                }
                if (setDescInput) {
                    content.flashcardSetDesc = setDescInput.value.trim();
                }

                // L?u t?ng flashcard t? DOM
                const flashcardItems = document.querySelectorAll(`.content-item[data-content-index="${index}"] .flashcard-item`);
                if (!content.flashcards) {
                    content.flashcards = [];
                }
                
                flashcardItems.forEach((flashcardEl, cardIndex) => {
                    const frontText = flashcardEl.querySelector('.flashcard-front')?.value || '';
                    const backText = flashcardEl.querySelector('.flashcard-back')?.value || '';
                    const hint = flashcardEl.querySelector('.flashcard-hint')?.value || '';
                    
                    if (content.flashcards[cardIndex]) {
                        content.flashcards[cardIndex].frontText = frontText;
                        content.flashcards[cardIndex].backText = backText;
                        content.flashcards[cardIndex].hint = hint;
                        content.flashcards[cardIndex].orderIndex = cardIndex;
                    }
                });
            } else if (content.contentType === 'Test') {
                // L?u test title, description, time limit, max attempts
                const testTitleInput = document.querySelector(`.content-item[data-content-index="${index}"] .test-title`);
                const testDescInput = document.querySelector(`.content-item[data-content-index="${index}"] .test-desc`);
                const testTimeInput = document.querySelector(`.content-item[data-content-index="${index}"] .test-time`);
                const testAttemptsInput = document.querySelector(`.content-item[data-content-index="${index}"] .test-attempts`);
                
                if (testTitleInput) {
                    content.testTitle = testTitleInput.value.trim();
                }
                if (testDescInput) {
                    content.testDesc = testDescInput.value.trim();
                }
                if (testTimeInput) {
                    content.timeLimitMinutes = parseInt(testTimeInput.value) || 30;
                }
                if (testAttemptsInput) {
                    content.maxAttempts = parseInt(testAttemptsInput.value) || 3;
                }

                // L?u t?ng question t? DOM
                const questionItems = document.querySelectorAll(`.content-item[data-content-index="${index}"] .test-question-item`);
                if (!content.questions) {
                    content.questions = [];
                }
                
                questionItems.forEach((questionEl, qIndex) => {
                    const questionTypeSelect = questionEl.querySelector('.form-control');
                    const questionTextArea = questionEl.querySelectorAll('textarea')[0];
                    const questionPointsInput = questionEl.querySelector('input[type="number"]');
                    
                    if (content.questions[qIndex]) {
                        if (questionTypeSelect) {
                            content.questions[qIndex].type = questionTypeSelect.value;
                        }
                        if (questionTextArea) {
                            content.questions[qIndex].stemText = questionTextArea.value.trim();
                        }
                        if (questionPointsInput) {
                            content.questions[qIndex].points = parseFloat(questionPointsInput.value) || 1;
                        }
                        content.questions[qIndex].orderIndex = qIndex;

                        // L?u options
                        const optionItems = questionEl.querySelectorAll('.option-item');
                        if (!content.questions[qIndex].options) {
                            content.questions[qIndex].options = [];
                        }
                        
                        optionItems.forEach((optionEl, oIndex) => {
                            const checkbox = optionEl.querySelector('input[type="checkbox"]');
                            const textInput = optionEl.querySelector('input[type="text"]');
                            
                            if (content.questions[qIndex].options[oIndex]) {
                                if (checkbox) {
                                    content.questions[qIndex].options[oIndex].isCorrect = checkbox.checked;
                                }
                                if (textInput) {
                                    content.questions[qIndex].options[oIndex].optionText = textInput.value.trim();
                                }
                                content.questions[qIndex].options[oIndex].orderIndex = oIndex;
                            }
                        });
                    }
                });
            }
        });
    }
}
```

### 3. Thêm hàm m?i `saveCurrentLessonContents()`
Thêm hàm này TR??C hàm `populateLessonSelector()`:

```javascript
// ? HÀM M?I: L?U N?I DUNG C?A LESSON HI?N T?I
function saveCurrentLessonContents() {
    const currentLesson = window.currentLesson;
    if (!currentLesson) return;

    const lesson = courseData.chapters[currentLesson.chapterIndex].lessons[currentLesson.lessonIndex];

    // L?u t?t c? contents t? DOM
    lesson.contents.forEach((content, index) => {
        // L?u tiêu ??
        const titleInput = document.querySelector(`.content-item[data-content-index="${index}"] .content-title-input`);
        if (titleInput) {
            content.title = titleInput.value.trim();
        }

        // L?u lo?i n?i dung
        const typeSelect = document.querySelector(`.content-item[data-content-index="${index}"] .content-type-select`);
        if (typeSelect) {
            content.contentType = typeSelect.value;
        }

        // L?u n?i dung theo lo?i
        if (content.contentType === 'Theory') {
            // L?u n?i dung lý thuy?t t? CKEditor
            for (const [key, editor] of Object.entries(ckEditorInstances)) {
                if (key.includes(`contentBody_`) && key.includes(`_${index}`)) {
                    content.body = editor.getData();
                    break;
                }
            }
        } else if (content.contentType === 'FlashcardSet') {
            // L?u flashcard set info
            const setTitleInput = document.querySelector(`.content-item[data-content-index="${index}"] .flashcard-set-title`);
            const setDescInput = document.querySelector(`.content-item[data-content-index="${index}"] .flashcard-set-desc`);
            
            if (setTitleInput) content.flashcardSetTitle = setTitleInput.value.trim();
            if (setDescInput) content.flashcardSetDesc = setDescInput.value.trim();

            // L?u t?ng flashcard
            const flashcardItems = document.querySelectorAll(`.content-item[data-content-index="${index}"] .flashcard-item`);
            if (!content.flashcards) content.flashcards = [];
            
            flashcardItems.forEach((flashcardEl, cardIndex) => {
                const frontText = flashcardEl.querySelector('.flashcard-front')?.value || '';
                const backText = flashcardEl.querySelector('.flashcard-back')?.value || '';
                const hint = flashcardEl.querySelector('.flashcard-hint')?.value || '';
                
                if (content.flashcards[cardIndex]) {
                    content.flashcards[cardIndex].frontText = frontText;
                    content.flashcards[cardIndex].backText = backText;
                    content.flashcards[cardIndex].hint = hint;
                    content.flashcards[cardIndex].orderIndex = cardIndex;
                }
            });
        } else if (content.contentType === 'Test') {
            // L?u test info
            const testTitleInput = document.querySelector(`.content-item[data-content-index="${index}"] .test-title`);
            const testDescInput = document.querySelector(`.content-item[data-content-index="${index}"] .test-desc`);
            const testTimeInput = document.querySelector(`.content-item[data-content-index="${index}"] .test-time`);
            const testAttemptsInput = document.querySelector(`.content-item[data-content-index="${index}"] .test-attempts`);
            
            if (testTitleInput) content.testTitle = testTitleInput.value.trim();
            if (testDescInput) content.testDesc = testDescInput.value.trim();
            if (testTimeInput) content.timeLimitMinutes = parseInt(testTimeInput.value) || 30;
            if (testAttemptsInput) content.maxAttempts = parseInt(testAttemptsInput.value) || 3;

            // L?u t?ng question
            const questionItems = document.querySelectorAll(`.content-item[data-content-index="${index}"] .test-question-item`);
            if (!content.questions) content.questions = [];
            
            questionItems.forEach((questionEl, qIndex) => {
                const questionTypeSelect = questionEl.querySelector('.form-control');
                const questionTextArea = questionEl.querySelectorAll('textarea')[0];
                const questionPointsInput = questionEl.querySelector('input[type="number"]');
                
                if (content.questions[qIndex]) {
                    if (questionTypeSelect) content.questions[qIndex].type = questionTypeSelect.value;
                    if (questionTextArea) content.questions[qIndex].stemText = questionTextArea.value.trim();
                    if (questionPointsInput) content.questions[qIndex].points = parseFloat(questionPointsInput.value) || 1;
                    content.questions[qIndex].orderIndex = qIndex;

                    // L?u options
                    const optionItems = questionEl.querySelectorAll('.option-item');
                    if (!content.questions[qIndex].options) content.questions[qIndex].options = [];
                    
                    optionItems.forEach((optionEl, oIndex) => {
                        const checkbox = optionEl.querySelector('input[type="checkbox"]');
                        const textInput = optionEl.querySelector('input[type="text"]');
                        
                        if (content.questions[qIndex].options[oIndex]) {
                            if (checkbox) content.questions[qIndex].options[oIndex].isCorrect = checkbox.checked;
                            if (textInput) content.questions[qIndex].options[oIndex].optionText = textInput.value.trim();
                            content.questions[qIndex].options[oIndex].orderIndex = oIndex;
                        }
                    });
                }
            });
        }
    });
}
```

### 4. S?a các hàm ?i?u h??ng
Thêm dòng g?i `saveCurrentLessonContents()` vào:

#### 4.1. Hàm `nextStep()` - Thêm ? ??u function:
```javascript
async function nextStep() {
    // ? L?U D? LI?U C?A LESSON HI?N T?I N?U ?ANG ? STEP 3
    if (currentStep === 3 && window.currentLesson) {
        saveCurrentLessonContents();
    }

    // Save first so validation reads the latest DOM-derived state (especially for step 2)
    saveStepData();
    // ... ph?n còn l?i
}
```

#### 4.2. Hàm `prevStep()` - Thêm ? ??u function:
```javascript
function prevStep() {
    // ? L?U D? LI?U C?A LESSON HI?N T?I N?U ?ANG ? STEP 3
    if (currentStep === 3 && window.currentLesson) {
        saveCurrentLessonContents();
    }

    if (currentStep > 1) {
        saveStepData();
        currentStep--;
        updateStepDisplay();
    }
}
```

#### 4.3. Hàm `populateLessonSelector()` - Thêm ? ??u function:
```javascript
function populateLessonSelector() {
    // ? L?U D? LI?U C?A LESSON HI?N T?I TR??C KHI CHUY?N
    if (window.currentLesson) {
        saveCurrentLessonContents();
    }

    saveStepData(); // Make sure we have latest data
    // ... ph?n còn l?i
}
```

#### 4.4. Hàm `loadLessonContents()` - Thêm ? ??u function:
```javascript
function loadLessonContents() {
    // ? L?U D? LI?U C?A LESSON HI?N T?I TR??C KHI CHUY?N
    if (window.currentLesson) {
        saveCurrentLessonContents();
    }

    const selector = document.getElementById('lessonSelector');
    // ... ph?n còn l?i
}
```

#### 4.5. Hàm `saveCourse()` - Thêm ? ??u function:
```javascript
function saveCourse(publish) {
    // ? L?U D? LI?U C?A LESSON HI?N T?I N?U CÓ
    if (window.currentLesson) {
        saveCurrentLessonContents();
    }

    saveStepData();
    // ... ph?n còn l?i
}
```

## Tóm t?t các thay ??i

1. **Step 2**: S? d?ng deep clone (JSON.parse/JSON.stringify) khi l?u contents vào Map
2. **Step 3**: B? sung logic ??y ?? ?? l?u Flashcard và Test t? DOM
3. **Thêm hàm m?i**: `saveCurrentLessonContents()` ?? l?u lesson hi?n t?i
4. **Các hàm ?i?u h??ng**: G?i `saveCurrentLessonContents()` tr??c khi chuy?n ??i

## Ki?m tra sau khi s?a

1. T?o m?t khóa h?c m?i v?i 1 ch??ng, 2 bài h?c
2. ? bài h?c 1: Thêm 1 flashcard set v?i 3 th?
3. ? bài h?c 2: Thêm 1 test v?i 2 câu h?i
4. Chuy?n qua l?i gi?a 2 bài h?c ? Ki?m tra d? li?u không b? m?t
5. L?u khóa h?c ? Ki?m tra database có ?? d? li?u không

## L?u ý

- File g?c ?ã ???c backup t?i: `Quiz_Web/wwwroot/js/course-builder.js` (git checkout)
- N?u có l?i, có th? rollback b?ng: `git checkout Quiz_Web/wwwroot/js/course-builder.js`
