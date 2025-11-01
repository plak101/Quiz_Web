// ============================================
// COURSE BUILDER - JAVASCRIPT
// ============================================

let currentStep = 1;
let courseData = {
    title: '',
    slug: '',
    summary: '',
    categoryId: null,
    coverUrl: '',
    price: 0,
    isPublished: false,
    chapters: []
};

let chapterCounter = 0;
let lessonCounter = 0;
let contentCounter = 0;
let autosaveTimer = null;
let ckEditorInstances = {};
let lastSlugCheck = { value: '', available: true, loading: false };
let slugDebounceTimer = null;
let lastSlugNotice = { value: '', at: 0 };

// Helper: notify by toastr (fallback to alert) and jump to Slug input (dedup)
function notifyAndFocusSlug(message, slugValue) {
    const slug = slugValue ?? (document.getElementById('Slug')?.value || '');
    const now = Date.now();
    if (lastSlugNotice.value === slug && now - lastSlugNotice.at < 2500) {
        // avoid spamming the same message repeatedly
        return;
    }
    lastSlugNotice = { value: slug, at: now };

    const msg = message || 'Slug này đã tồn tại, vui lòng chọn slug khác.';
    if (window.toastr && typeof toastr.error === 'function') {
        try { toastr.clear(); } catch {}
        toastr.error(msg, 'Thông báo');
    } else {
        try { alert(msg); } catch { /* no-op */ }
    }
    const slugInput = document.getElementById('Slug');
    if (slugInput) {
        slugInput.scrollIntoView({ behavior: 'smooth', block: 'center' });
        setTimeout(() => {
            slugInput.focus({ preventScroll: true });
            if (typeof slugInput.select === 'function') slugInput.select();
        }, 200);
    }
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    initializeCKEditor();
    setupEventListeners();
    
    // Load existing data if editing
    if (typeof window.existingCourseData !== 'undefined') {
        loadExistingData(window.existingCourseData);
    }
    
    // Start autosave
    startAutosave();
    
    // Initialize slug generation
    const titleInput = document.getElementById('Title');
    const slugInput = document.getElementById('Slug');
    if (titleInput) {
        titleInput.addEventListener('input', () => {
            generateSlug();
            // clear error while typing; re-validate with debounce
            hideFieldError('Slug');
            lastSlugCheck = { value: '', available: undefined, loading: false };
        });
    }
    if (slugInput) {
        const debouncedCheck = () => {
            const slug = slugInput.value.trim();
            if (!slug) { hideFieldError('Slug'); return; }
            clearTimeout(slugDebounceTimer);
            hideFieldError('Slug'); // clear old red text as user edits
            lastSlugCheck.loading = true;
            slugDebounceTimer = setTimeout(() => {
                checkSlugUnique(slug).then(avail => {
                    if (!avail) {
                        showFieldError('Slug', 'Slug này đã tồn tại');
                        notifyAndFocusSlug('Slug này đã tồn tại, vui lòng chọn slug khác.', slug);
                    } else {
                        // valid -> clear any previous toast
                        if (window.toastr && typeof toastr.clear === 'function') toastr.clear();
                    }
                }).catch(() => { /* no-op */ });
            }, 300);
        };
        slugInput.addEventListener('input', debouncedCheck);
        slugInput.addEventListener('blur', debouncedCheck);
    }
});
// Helper: get courseId from query when editing
function getCurrentCourseId() {
    const qs = new URLSearchParams(window.location.search);
    const id = qs.get('id');
    return id ? parseInt(id) : null;
}

// API call: check slug uniqueness (no toastr here; callers decide UI)
async function checkSlugUnique(slug) {
    const courseId = getCurrentCourseId();
    lastSlugCheck.loading = true;
    try {
        const url = `/courses/check-slug?slug=${encodeURIComponent(slug)}${courseId ? `&excludeId=${courseId}` : ''}`;
        const res = await fetch(url, { method: 'GET', credentials: 'same-origin' });
        const data = await res.json();
        lastSlugCheck = { value: slug, available: !!data.available, loading: false };
        if (data.available) {
            hideFieldError('Slug');
            if (window.toastr && typeof toastr.clear === 'function') toastr.clear();
        } else {
            showFieldError('Slug', 'Slug này đã tồn tại');
        }
        return !!data.available;
    } catch (e) {
        lastSlugCheck = { value: slug, available: true, loading: false };
        return true; // Không chặn trong trường hợp lỗi mạng tạm thời
    }
}
// ============================================
// CKEDITOR INITIALIZATION
// ============================================
function initializeCKEditor() {
    // Initialize Summary editor
    const summaryField = document.getElementById('Summary');
    if (summaryField) {
        ClassicEditor
            .create(summaryField, {
                toolbar: ['heading', '|', 'bold', 'italic', 'link', 'bulletedList', 'numberedList', '|', 'undo', 'redo'],
                placeholder: 'Nhập mô tả ngắn về khóa học...'
            })
            .then(editor => {
                ckEditorInstances['Summary'] = editor;
            })
            .catch(error => {
                console.error('Error initializing CKEditor:', error);
            });
    }
}

function initializeChapterDescriptionEditor(chapterId) {
    const textarea = document.getElementById(`chapterDesc_${chapterId}`);
    if (textarea && !ckEditorInstances[`chapterDesc_${chapterId}`]) {
        ClassicEditor
            .create(textarea, {
                toolbar: ['heading', '|', 'bold', 'italic', 'link', '|', 'undo', 'redo'],
                placeholder: 'Mô tả ngắn về chương...'
            })
            .then(editor => {
                ckEditorInstances[`chapterDesc_${chapterId}`] = editor;
            })
            .catch(error => {
                console.error('Error initializing chapter description editor:', error);
            });
    }
}

function initializeContentBodyEditor(contentId) {
    const textarea = document.getElementById(`contentBody_${contentId}`);
    if (textarea && !ckEditorInstances[`contentBody_${contentId}`]) {
        ClassicEditor
            .create(textarea, {
                toolbar: {
                    items: [
                        'heading', '|',
                        'bold', 'italic', 'link', '|',
                        'bulletedList', 'numberedList', '|',
                        'imageUpload', 'blockQuote', 'insertTable', '|',
                        'undo', 'redo', '|',
                        'code', 'codeBlock'
                    ]
                },
                placeholder: 'Nhập nội dung bài học...',
                image: {
                    toolbar: ['imageStyle:inline', 'imageStyle:block', 'imageStyle:side', '|', 'imageTextAlternative']
                }
            })
            .then(editor => {
                ckEditorInstances[`contentBody_${contentId}`] = editor;
            })
            .catch(error => {
                console.error('Error initializing content body editor:', error);
            });
    }
}

// ============================================
// EVENT LISTENERS
// ============================================
function setupEventListeners() {
    // File upload preview
    const coverFileInput = document.getElementById('CoverFile');
    if (coverFileInput) {
        coverFileInput.addEventListener('change', handleFileUpload);
    }
    
    // Remove image button
    const removeImageBtn = document.querySelector('.remove-image');
    if (removeImageBtn) {
        removeImageBtn.addEventListener('click', removeImage);
    }
}

// ============================================
// STEP NAVIGATION
// ============================================
async function nextStep() {
    // Save first so validation reads the latest DOM-derived state (especially for step 2)
    saveStepData();

    if (!validateCurrentStep()) {
        return;
    }
    // Extra: step 1 must check slug uniqueness server-side
    if (currentStep === 1) {
        const slug = document.getElementById('Slug')?.value.trim();
        if (slug) {
            // Only re-check if slug changed or no previous result
            if (lastSlugCheck.value !== slug || lastSlugCheck.loading || lastSlugCheck.available !== true) {
                const available = await checkSlugUnique(slug);
                if (!available) { notifyAndFocusSlug('Slug này đã tồn tại, vui lòng chọn slug khác.', slug); return; }
            }
        }
    }

    if (currentStep < 4) {
        currentStep++;
        updateStepDisplay();
        
        // Special handling for step 3 - populate lesson selector
        if (currentStep === 3) {
            populateLessonSelector();
        }
        
        // Special handling for step 4 - show preview
        if (currentStep === 4) {
            renderPreview();
        }
    }
}

function prevStep() {
    if (currentStep > 1) {
        currentStep--;
        updateStepDisplay();
    }
}

function updateStepDisplay() {
    // Update progress bar
    const progressFill = document.querySelector('.progress-fill');
    const progressPercentage = ((currentStep - 1) / 3) * 100;
    progressFill.style.width = `${progressPercentage}%`;
    
    // Update step indicators
    document.querySelectorAll('.step').forEach((step, index) => {
        const stepNum = index + 1;
        step.classList.remove('active', 'completed');
        
        if (stepNum === currentStep) {
            step.classList.add('active');
        } else if (stepNum < currentStep) {
            step.classList.add('completed');
        }
    });
    
    // Update step content
    document.querySelectorAll('.step-content').forEach(content => {
        content.classList.remove('active');
    });
    
    const activeContent = document.querySelector(`.step-content[data-step="${currentStep}"]`);
    if (activeContent) {
        activeContent.classList.add('active');
    }
    
    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ============================================
// STEP VALIDATION
// ============================================
function validateCurrentStep() {
    let isValid = true;
    
    // Clear previous errors
    document.querySelectorAll('.field-error').forEach(el => {
        el.classList.remove('show');
        el.textContent = '';
    });
    
    if (currentStep === 1) {
        // Validate course info
        const title = document.getElementById('Title').value.trim();
        const slug = document.getElementById('Slug').value.trim();
        
        if (!title) {
            showFieldError('Title', 'Tiêu đề khóa học là bắt buộc');
            isValid = false;
        }
        
        if (!slug) {
            showFieldError('Slug', 'Slug là bắt buộc');
            isValid = false;
        } else if (!/^[a-z0-9-]+$/.test(slug)) {
            showFieldError('Slug', 'Slug chỉ được chứa chữ thường, số và dấu gạch ngang');
            isValid = false;
        }
    }
    
    if (currentStep === 2) {
        // Validate chapters and lessons
        if (courseData.chapters.length === 0) {
            alert('Vui lòng thêm ít nhất một chương');
            isValid = false;
        } else {
            let hasLesson = false;
            for (const chapter of courseData.chapters) {
                if (chapter.lessons && chapter.lessons.length > 0) {
                    hasLesson = true;
                    break;
                }
            }
            if (!hasLesson) {
                alert('Vui lòng thêm ít nhất một bài học');
                isValid = false;
            }
        }
    }
    
    return isValid;
}

function showFieldError(fieldName, message) {
    const errorSpan = document.querySelector(`.field-error[data-field="${fieldName}"]`);
    if (errorSpan) {
        errorSpan.textContent = message;
        errorSpan.classList.add('show');
    }
}

function hideFieldError(fieldName) {
    const errorSpan = document.querySelector(`.field-error[data-field="${fieldName}"]`);
    if (errorSpan) {
        errorSpan.textContent = '';
        errorSpan.classList.remove('show');
    }
}

// ============================================
// SAVE STEP DATA
// ============================================
function saveStepData() {
    if (currentStep === 1) {
        // Save course info
        courseData.title = document.getElementById('Title').value.trim();
        courseData.slug = document.getElementById('Slug').value.trim();
        
        const summaryEditor = ckEditorInstances['Summary'];
        courseData.summary = summaryEditor ? summaryEditor.getData() : '';
        
        const categorySelect = document.getElementById('CategoryId');
        courseData.categoryId = categorySelect.value ? parseInt(categorySelect.value) : null;
        
        courseData.coverUrl = document.getElementById('CoverUrl').value;
        courseData.price = parseFloat(document.getElementById('Price').value) || 0;
        courseData.isPublished = document.getElementById('IsPublished').checked;
    }
    
    if (currentStep === 2) {
        // Save chapters and lessons from DOM
        courseData.chapters = [];
        
        document.querySelectorAll('.chapter-item').forEach((chapterEl, chapterIndex) => {
            const chapterId = chapterEl.dataset.chapterId;
            const chapterTitle = chapterEl.querySelector('.chapter-title-input').value.trim();
            
            const chapterDescEditor = ckEditorInstances[`chapterDesc_${chapterId}`];
            const chapterDesc = chapterDescEditor ? chapterDescEditor.getData() : '';
            
            const chapter = {
                chapterId: null,
                title: chapterTitle,
                description: chapterDesc,
                orderIndex: chapterIndex,
                lessons: []
            };
            
            chapterEl.querySelectorAll('.lesson-item').forEach((lessonEl, lessonIndex) => {
                const lessonTitle = lessonEl.querySelector('.lesson-title-input').value.trim();
                const lessonVisibility = lessonEl.querySelector('.lesson-visibility').value;
                
                const lesson = {
                    lessonId: null,
                    title: lessonTitle,
                    description: '',
                    orderIndex: lessonIndex,
                    visibility: lessonVisibility,
                    contents: []
                };
                
                // Find existing lesson contents if any
                const existingLesson = findLessonInData(lessonEl.dataset.lessonId);
                if (existingLesson && existingLesson.contents) {
                    lesson.contents = existingLesson.contents;
                }
                
                chapter.lessons.push(lesson);
            });
            
            courseData.chapters.push(chapter);
        });
    }
}

function findLessonInData(lessonId) {
    for (const chapter of courseData.chapters) {
        for (const lesson of chapter.lessons) {
            if (lesson.lessonId === lessonId) {
                return lesson;
            }
        }
    }
    return null;
}

// ============================================
// SLUG GENERATION
// ============================================
function generateSlug() {
    const titleInput = document.getElementById('Title');
    const slugInput = document.getElementById('Slug');
    
    if (!titleInput || !slugInput) return;
    
    // Vietnamese to ASCII mapping
    const vietnameseMap = {
        'à': 'a', 'á': 'a', 'ạ': 'a', 'ả': 'a', 'ã': 'a', 'â': 'a', 'ầ': 'a', 'ấ': 'a', 'ậ': 'a', 'ẩ': 'a', 'ẫ': 'a', 'ă': 'a', 'ằ': 'a', 'ắ': 'a', 'ặ': 'a', 'ẳ': 'a', 'ẵ': 'a',
        'è': 'e', 'é': 'e', 'ẹ': 'e', 'ẻ': 'e', 'ẽ': 'e', 'ê': 'e', 'ề': 'e', 'ế': 'e', 'ệ': 'e', 'ể': 'e', 'ễ': 'e',
        'ì': 'i', 'í': 'i', 'ị': 'i', 'ỉ': 'i', 'ĩ': 'i',
        'ò': 'o', 'ó': 'o', 'ọ': 'o', 'ỏ': 'o', 'õ': 'o', 'ô': 'o', 'ồ': 'o', 'ố': 'o', 'ộ': 'o', 'ổ': 'o', 'ỗ': 'o', 'ơ': 'o', 'ờ': 'o', 'ớ': 'o', 'ợ': 'o', 'ở': 'o', 'ỡ': 'o',
        'ù': 'u', 'ú': 'u', 'ụ': 'u', 'ủ': 'u', 'ũ': 'u', 'ư': 'u', 'ừ': 'u', 'ứ': 'u', 'ự': 'u', 'ử': 'u', 'ữ': 'u',
        'ỳ': 'y', 'ý': 'y', 'ỵ': 'y', 'ỷ': 'y', 'ỹ': 'y',
        'đ': 'd',
        'À': 'A', 'Á': 'A', 'Ạ': 'A', 'Ả': 'A', 'Ã': 'A', 'Â': 'A', 'Ầ': 'A', 'Ấ': 'A', 'Ậ': 'A', 'Ả': 'A', 'Ẫ': 'A', 'Ă': 'A', 'Ằ': 'A', 'Ắ': 'A', 'Ặ': 'A', 'Ẳ': 'A', 'Ẵ': 'A',
        'È': 'E', 'É': 'E', 'Ẹ': 'E', 'Ẻ': 'E', 'Ẽ': 'E', 'Ê': 'E', 'Ề': 'E', 'Ế': 'E', 'ệ': 'E', 'Ể': 'E', 'Ễ': 'E',
        'Ì': 'I', 'Í': 'I', 'Ị': 'I', 'Ỉ': 'I', 'Ĩ': 'I',
        'Ò': 'O', 'Ó': 'O', 'Ọ': 'O', 'Ỏ': 'O', 'Õ': 'O', 'Ô': 'O', 'Ồ': 'O', 'Ố': 'O', 'Ộ': 'O', 'Ổ': 'O', 'Ỗ': 'O', 'Ơ': 'O', 'Ờ': 'O', 'Ớ': 'O', 'Ợ': 'O', 'Ở': 'O', 'Ỡ': 'O',
        'Ù': 'U', 'Ú': 'U', 'Ụ': 'U', 'Ủ': 'U', 'Ũ': 'U', 'Ư': 'U', 'Ừ': 'U', 'Ứ': 'U', 'Ự': 'U', 'Ử': 'U', 'Ữ': 'U',
        'Ỳ': 'Y', 'Ý': 'Y', 'Ỵ': 'Y', 'Ỷ': 'Y', 'Ỹ': 'Y',
        'Đ': 'D'
    };
    
    let slug = titleInput.value;
    
    // Replace Vietnamese characters
    for (let char in vietnameseMap) {
        slug = slug.replace(new RegExp(char, 'g'), vietnameseMap[char]);
    }
    
    // Convert to slug format
    slug = slug
        .toLowerCase()
        .replace(/[^a-z0-9\s-]/g, '') // Remove special characters
        .replace(/\s+/g, '-') // Replace spaces with hyphens
        .replace(/-+/g, '-') // Replace multiple hyphens with single hyphen
        .replace(/^-+|-+$/g, ''); // Remove leading/trailing hyphens
    
    //const slugInput = document.getElementById('Slug');
    slugInput.value = slug;
}

// ============================================
// FILE UPLOAD
// ============================================
function handleFileUpload(event) {
    const file = event.target.files[0];
    if (!file) return;
    
    const reader = new FileReader();
    reader.onload = function(e) {
        const preview = document.getElementById('imagePreview');
        const img = preview.querySelector('img');
        
        img.src = e.target.result;
        preview.style.display = 'block';
    };
    
    reader.readAsDataURL(file);
}

function removeImage() {
    const preview = document.getElementById('imagePreview');
    const fileInput = document.getElementById('CoverFile');
    const coverUrlInput = document.getElementById('CoverUrl');
    
    preview.style.display = 'none';
    fileInput.value = '';
    coverUrlInput.value = '';
}

// ============================================
// CHAPTERS MANAGEMENT
// ============================================
function addChapter() {
    chapterCounter++;
    const chapterId = `chapter_${chapterCounter}`;
    
    const chapterHTML = `
        <div class="chapter-item" data-chapter-id="${chapterId}">
            <div class="chapter-header">
                <div class="chapter-header-left">
                    <i class="fas fa-grip-vertical chapter-drag-handle"></i>
                    <div class="chapter-toggle">
                        <i class="fas fa-chevron-down"></i>
                    </div>
                    <input type="text" 
                           class="chapter-title-input" 
                           placeholder="Tên chương" 
                           value="Chương ${chapterCounter}" 
                           required>
                </div>
                <div class="chapter-actions">
                    <button type="button" class="icon-btn danger" onclick="removeChapter('${chapterId}')">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
            <div class="chapter-body">
                <div class="chapter-description">
                    <label>Mô tả chương:</label>
                    <textarea id="chapterDesc_${chapterId}" class="form-control" rows="3"></textarea>
                </div>
                <div class="lessons-container" data-chapter-id="${chapterId}">
                    <!-- Lessons will be added here -->
                </div>
                <button type="button" class="btn btn-outline-primary btn-add-lesson" onclick="addLesson('${chapterId}')">
                    <i class="fas fa-plus"></i> Thêm bài học
                </button>
            </div>
        </div>
    `;
    
    const container = document.getElementById('chaptersContainer');
    container.insertAdjacentHTML('beforeend', chapterHTML);
    
    // Initialize CKEditor for chapter description
    setTimeout(() => {
        initializeChapterDescriptionEditor(chapterId);
    }, 100);
    
    // Add collapse/expand functionality
    const chapterItem = container.lastElementChild;
    const chapterHeader = chapterItem.querySelector('.chapter-header');
    chapterHeader.addEventListener('click', function(e) {
        if (e.target.closest('.chapter-actions')) return;
        chapterItem.classList.toggle('collapsed');
    });
    
    // Initialize Sortable for chapters
    initializeChaptersSortable();
}

function removeChapter(chapterId) {
    if (confirm('Bạn có chắc muốn xóa chương này?')) {
        const chapterItem = document.querySelector(`.chapter-item[data-chapter-id="${chapterId}"]`);
        if (chapterItem) {
            // Destroy CKEditor instance
            const editorId = `chapterDesc_${chapterId}`;
            if (ckEditorInstances[editorId]) {
                ckEditorInstances[editorId].destroy();
                delete ckEditorInstances[editorId];
            }
            
            chapterItem.remove();
        }
    }
}

function initializeChaptersSortable() {
    const container = document.getElementById('chaptersContainer');
    if (container && !container.sortableInitialized) {
        new Sortable(container, {
            animation: 150,
            handle: '.chapter-drag-handle',
            ghostClass: 'sortable-ghost',
            dragClass: 'sortable-drag'
        });
        container.sortableInitialized = true;
    }
}

// ============================================
// LESSONS MANAGEMENT
// ============================================
function addLesson(chapterId) {
    lessonCounter++;
    const lessonId = `lesson_${lessonCounter}`;
    
    const lessonHTML = `
        <div class="lesson-item" data-lesson-id="${lessonId}">
            <i class="fas fa-grip-vertical lesson-drag-handle"></i>
            <div class="lesson-info">
                <input type="text" 
                       class="lesson-title-input" 
                       placeholder="Tên bài học" 
                       value="Bài học ${lessonCounter}" 
                       required>
            </div>
            <select class="lesson-visibility form-control">
                <option value="Course">Course</option>
                <option value="Public">Public</option>
                <option value="Private">Private</option>
            </select>
            <button type="button" class="icon-btn danger" onclick="removeLesson('${lessonId}')">
                <i class="fas fa-trash"></i>
            </button>
        </div>
    `;
    
    const lessonsContainer = document.querySelector(`.lessons-container[data-chapter-id="${chapterId}"]`);
    if (lessonsContainer) {
        lessonsContainer.insertAdjacentHTML('beforeend', lessonHTML);
        
        // Initialize Sortable for lessons
        initializeLessonsSortable(lessonsContainer);
    }
}

function removeLesson(lessonId) {
    if (confirm('Bạn có chắc muốn xóa bài học này?')) {
        const lessonItem = document.querySelector(`.lesson-item[data-lesson-id="${lessonId}"]`);
        if (lessonItem) {
            lessonItem.remove();
        }
    }
}

function initializeLessonsSortable(container) {
    if (container && !container.sortableInitialized) {
        new Sortable(container, {
            animation: 150,
            handle: '.lesson-drag-handle',
            ghostClass: 'sortable-ghost',
            dragClass: 'sortable-drag'
        });
        container.sortableInitialized = true;
    }
}

// ============================================
// LESSON CONTENTS MANAGEMENT
// ============================================
function populateLessonSelector() {
    saveStepData(); // Make sure we have latest data
    
    const selector = document.getElementById('lessonSelector');
    selector.innerHTML = '<option value="">-- Chọn bài học --</option>';
    
    courseData.chapters.forEach((chapter, chapterIndex) => {
        chapter.lessons.forEach((lesson, lessonIndex) => {
            const lessonKey = `${chapterIndex}_${lessonIndex}`;
            const option = document.createElement('option');
            option.value = lessonKey;
            option.textContent = `${chapter.title} → ${lesson.title}`;
            selector.appendChild(option);
        });
    });
}

function loadLessonContents() {
    const selector = document.getElementById('lessonSelector');
    const selectedKey = selector.value;
    
    const contentsContainer = document.getElementById('contentsContainer');
    const contentsList = document.getElementById('contentsList');
    
    if (!selectedKey) {
        contentsContainer.style.display = 'none';
        return;
    }
    
    contentsContainer.style.display = 'block';
    
    const [chapterIndex, lessonIndex] = selectedKey.split('_').map(Number);
    const lesson = courseData.chapters[chapterIndex].lessons[lessonIndex];
    
    // Store current lesson for adding contents
    window.currentLesson = { chapterIndex, lessonIndex };
    
    // Render existing contents
    contentsList.innerHTML = '';
    if (lesson.contents && lesson.contents.length > 0) {
        lesson.contents.forEach((content, index) => {
            renderContentItem(content, index);
        });
    }
}

function addContent() {
    const currentLesson = window.currentLesson;
    if (!currentLesson) {
        alert('Vui lòng chọn bài học');
        return;
    }
    
    contentCounter++;
    const contentId = `content_${contentCounter}`;
    
    const content = {
        contentId: null,
        contentType: 'Theory',
        refId: null,
        title: `Nội dung ${contentCounter}`,
        body: '',
        orderIndex: courseData.chapters[currentLesson.chapterIndex].lessons[currentLesson.lessonIndex].contents.length
    };
    
    courseData.chapters[currentLesson.chapterIndex].lessons[currentLesson.lessonIndex].contents.push(content);
    
    const contentsList = document.getElementById('contentsList');
    renderContentItem(content, content.orderIndex);
}

function renderContentItem(content, index) {
    const contentId = `content_${Date.now()}_${index}`;
    
    const contentHTML = `
        <div class="content-item" data-content-index="${index}">
            <div class="content-header">
                <div>
                    <span class="content-type-badge ${content.contentType.toLowerCase()}">${getContentTypeLabel(content.contentType)}</span>
                    <strong style="margin-left: 1rem;">${content.title || 'Chưa có tiêu đề'}</strong>
                </div>
                <div>
                    <button type="button" class="icon-btn" onclick="toggleContentBody(${index})">
                        <i class="fas fa-chevron-down"></i>
                    </button>
                    <button type="button" class="icon-btn danger" onclick="removeContent(${index})">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
            <div class="content-body" style="display: none;">
                <div class="content-form">
                    <div class="form-group">
                        <label>Loại nội dung:</label>
                        <select class="form-control content-type-select" onchange="updateContentType(${index}, this.value)">
                            <option value="Theory" ${content.contentType === 'Theory' ? 'selected' : ''}>Lý thuyết</option>
                            <option value="Video" ${content.contentType === 'Video' ? 'selected' : ''}>Video</option>
                            <option value="FlashcardSet" ${content.contentType === 'FlashcardSet' ? 'selected' : ''}>Flashcard</option>
                            <option value="Test" ${content.contentType === 'Test' ? 'selected' : ''}>Test</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label>Tiêu đề:</label>
                        <input type="text" class="form-control content-title-input" value="${content.title || ''}" onchange="updateContentTitle(${index}, this.value)">
                    </div>
                    ${content.contentType === 'Theory' ? `
                        <div class="form-group">
                            <label>Nội dung:</label>
                            <textarea id="contentBody_${contentId}" class="form-control" rows="10">${content.body || ''}</textarea>
                        </div>
                    ` : ''}
                </div>
            </div>
        </div>
    `;
    
    const contentsList = document.getElementById('contentsList');
    contentsList.insertAdjacentHTML('beforeend', contentHTML);
    
    // Initialize CKEditor for theory content
    if (content.contentType === 'Theory') {
        setTimeout(() => {
            initializeContentBodyEditor(contentId);
        }, 100);
    }
}

function toggleContentBody(index) {
    const contentItem = document.querySelector(`.content-item[data-content-index="${index}"]`);
    if (contentItem) {
        const body = contentItem.querySelector('.content-body');
        const icon = contentItem.querySelector('.content-header button i');
        
        if (body.style.display === 'none') {
            body.style.display = 'block';
            icon.classList.replace('fa-chevron-down', 'fa-chevron-up');
        } else {
            body.style.display = 'none';
            icon.classList.replace('fa-chevron-up', 'fa-chevron-down');
        }
    }
}

function updateContentType(index, type) {
    const currentLesson = window.currentLesson;
    if (currentLesson) {
        courseData.chapters[currentLesson.chapterIndex].lessons[currentLesson.lessonIndex].contents[index].contentType = type;
        loadLessonContents(); // Reload to show appropriate fields
    }
}

function updateContentTitle(index, title) {
    const currentLesson = window.currentLesson;
    if (currentLesson) {
        courseData.chapters[currentLesson.chapterIndex].lessons[currentLesson.lessonIndex].contents[index].title = title;
    }
}

function removeContent(index) {
    if (confirm('Bạn có chắc muốn xóa nội dung này?')) {
        const currentLesson = window.currentLesson;
        if (currentLesson) {
            courseData.chapters[currentLesson.chapterIndex].lessons[currentLesson.lessonIndex].contents.splice(index, 1);
            loadLessonContents();
        }
    }
}

function getContentTypeLabel(type) {
    const labels = {
        'Theory': 'Lý thuyết',
        'Video': 'Video',
        'FlashcardSet': 'Flashcard',
        'Test': 'Kiểm tra'
    };
    return labels[type] || type;
}

// ============================================
// PREVIEW
// ============================================
function renderPreview() {
    saveStepData();
    
    const previewContainer = document.getElementById('previewContainer');
    
    let totalLessons = 0;
    let totalContents = 0;
    
    courseData.chapters.forEach(chapter => {
        totalLessons += chapter.lessons.length;
        chapter.lessons.forEach(lesson => {
            totalContents += lesson.contents ? lesson.contents.length : 0;
        });
    });
    
    let chaptersHTML = '';
    courseData.chapters.forEach(chapter => {
        let lessonsHTML = '<div class="preview-lessons">';
        chapter.lessons.forEach(lesson => {
            lessonsHTML += `
                <div class="preview-lesson">
                    <i class="fas fa-book"></i> ${lesson.title} (${lesson.contents ? lesson.contents.length : 0} nội dung)
                </div>
            `;
        });
        lessonsHTML += '</div>';
        
        chaptersHTML += `
            <div class="preview-chapter">
                <div class="preview-chapter-title">
                    <i class="fas fa-folder"></i> ${chapter.title}
                </div>
                ${lessonsHTML}
            </div>
        `;
    });
    
    previewContainer.innerHTML = `
        <div class="preview-section">
            <h3>Thông tin khóa học</h3>
            <div class="preview-grid">
                <div class="preview-item">
                    <div class="preview-label">Tiêu đề:</div>
                    <div class="preview-value">${courseData.title}</div>
                </div>
                <div class="preview-item">
                    <div class="preview-label">Slug:</div>
                    <div class="preview-value">${courseData.slug}</div>
                </div>
                <div class="preview-item">
                    <div class="preview-label">Giá:</div>
                    <div class="preview-value">${courseData.price === 0 ? 'Miễn phí' : courseData.price.toLocaleString('vi-VN') + ' VNĐ'}</div>
                </div>
                <div class="preview-item">
                    <div class="preview-label">Trạng thái:</div>
                    <div class="preview-value">${courseData.isPublished ? 'Xuất bản' : 'Bản nháp'}</div>
                </div>
            </div>
        </div>
        
        <div class="preview-section">
            <h3>Tổng quan cấu trúc</h3>
            <div class="preview-grid">
                <div class="preview-item">
                    <div class="preview-label">Số chương:</div>
                    <div class="preview-value">${courseData.chapters.length}</div>
                </div>
                <div class="preview-item">
                    <div class="preview-label">Số bài học:</div>
                    <div class="preview-value">${totalLessons}</div>
                </div>
                <div class="preview-item">
                    <div class="preview-label">Tổng nội dung:</div>
                    <div class="preview-value">${totalContents}</div>
                </div>
            </div>
        </div>
        
        <div class="preview-section">
            <h3>Cấu trúc khóa học</h3>
            ${chaptersHTML}
        </div>
    `;
}

// ============================================
// SAVE COURSE
// ============================================
function saveCourse(publish) {
    saveStepData();
    
    // Update publish status
    courseData.isPublished = publish;
    
    // Prepare form data
    const form = document.getElementById('courseBuilderForm');
    const jsonDataInput = document.getElementById('jsonDataInput');
    jsonDataInput.value = JSON.stringify(courseData);
    
    // Set form action based on whether it's create or update
    const courseId = new URLSearchParams(window.location.search).get('id');
    if (courseId) {
        form.action = `/courses/builder/update/${courseId}`;
    } else {
        form.action = '/courses/builder/save';
    }
    
    // Submit the form
    form.submit();
}

// ============================================
// AUTOSAVE
// ============================================
function startAutosave() {
    // Autosave every 10 seconds
    autosaveTimer = setInterval(() => {
        if (currentStep === 1) {
            performAutosave();
        }
    }, 10000);
}

async function performAutosave() {
    saveStepData();

    // Skip autosave when slug is invalid or not confirmed available for current value
    const slug = courseData.slug?.trim();
    if (!slug || !/^[a-z0-9-]+$/.test(slug)) return;
    if (lastSlugCheck.value !== slug || lastSlugCheck.available !== true || lastSlugCheck.loading) return;

    const autosaveData = {
        courseId: new URLSearchParams(window.location.search).get('id'),
        title: courseData.title,
        slug: courseData.slug,
        summary: courseData.summary,
        categoryId: courseData.categoryId,
        coverUrl: courseData.coverUrl,
        price: courseData.price,
        isPublished: courseData.isPublished
    };

    try {
        const res = await fetch('/courses/builder/autosave', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify(autosaveData),
            credentials: 'same-origin'
        });

        let data = {};
        try { data = await res.json(); } catch { /* no json body */ }

        // If API signals duplicate slug, show UI error and mark cache
        if (res.status === 409 && (data?.code === 'DuplicateSlug' || data?.message)) {
            showFieldError('Slug', 'Slug này đã tồn tại');
            lastSlugCheck = { value: autosaveData.slug, available: false, loading: false };
            notifyAndFocusSlug('Slug này đã tồn tại, vui lòng chọn slug khác.', autosaveData.slug);
            return;
        }

        if (!res.ok || data?.success === false) {
            console.warn('Autosave failed', data);
            return;
        }

        if (data?.success) {
            showAutosaveStatus();
        }
    } catch (error) {
        console.error('Autosave error:', error);
    }
}

function showAutosaveStatus() {
    const status = document.querySelector('.autosave-status');
    status.style.display = 'flex';
    
    setTimeout(() => {
        status.style.display = 'none';
    }, 3000);
}

// ============================================
// LOAD EXISTING DATA
// ============================================
function loadExistingData(data) {
    courseData = data;
    
    // Load step 1 data
    document.getElementById('Title').value = data.title || '';
    document.getElementById('Slug').value = data.slug || '';
    
    const categorySelect = document.getElementById('CategoryId');
    if (data.categoryId && categorySelect) {
        categorySelect.value = data.categoryId;
    }
    
    document.getElementById('Price').value = data.price || 0;
    document.getElementById('IsPublished').checked = data.isPublished || false;
    document.getElementById('CoverUrl').value = data.coverUrl || '';
    
    if (data.coverUrl) {
        const preview = document.getElementById('imagePreview');
        const img = preview.querySelector('img');
        img.src = data.coverUrl;
        preview.style.display = 'block';
    }
    
    // Load CKEditor data after initialization
    setTimeout(() => {
        if (ckEditorInstances['Summary'] && data.summary) {
            ckEditorInstances['Summary'].setData(data.summary);
        }
    }, 500);
    
    // Load chapters and lessons
    if (data.chapters && data.chapters.length > 0) {
        data.chapters.forEach(chapter => {
            addChapter();
            const chapterItems = document.querySelectorAll('.chapter-item');
            const lastChapterItem = chapterItems[chapterItems.length - 1];
            const chapterId = lastChapterItem.dataset.chapterId;
            
            lastChapterItem.querySelector('.chapter-title-input').value = chapter.title;
            
            // Load chapter description
            setTimeout(() => {
                const chapterDescEditor = ckEditorInstances[`chapterDesc_${chapterId}`];
                if (chapterDescEditor && chapter.description) {
                    chapterDescEditor.setData(chapter.description);
                }
            }, 500);
            
            // Load lessons
            if (chapter.lessons && chapter.lessons.length > 0) {
                chapter.lessons.forEach(lesson => {
                    addLesson(chapterId);
                    const lessonItems = lastChapterItem.querySelectorAll('.lesson-item');
                    const lastLessonItem = lessonItems[lessonItems.length - 1];
                    
                    lastLessonItem.querySelector('.lesson-title-input').value = lesson.title;
                    lastLessonItem.querySelector('.lesson-visibility').value = lesson.visibility;
                });
            }
        });
    }
}
