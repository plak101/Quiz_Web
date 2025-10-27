// Quiz State
let currentQuestionIndex = 0;
let userAnswers = {};
let startTime = Date.now();

// Option Colors
const optionColors = ['option-teal', 'option-lime', 'option-orange', 'option-pink'];

// Initialize Quiz
document.addEventListener('DOMContentLoaded', function() {
    if (quizData.questions.length === 0) {
        showError('No questions available for this quiz.');
        return;
    }

    // Start timer if time limit exists
    if (quizData.timeLimit > 0) {
        startTimer(quizData.timeLimit);
    }

    // Load first question
    loadQuestion(currentQuestionIndex);

    // Setup navigation buttons
    setupNavigation();
});

// Load Question
function loadQuestion(index) {
    if (index < 0 || index >= quizData.questions.length) {
        return;
    }

    const question = quizData.questions[index];
    
    // Update progress
    updateProgress();

    // Update question text
    document.getElementById('questionTitle').textContent = `Question ${index + 1}`;
    document.getElementById('questionText').textContent = question.stemText;

    // Load options
    const optionsSection = document.getElementById('optionsSection');
    optionsSection.innerHTML = '';

    question.options.forEach((option, idx) => {
        const button = document.createElement('button');
        button.className = `option-btn ${optionColors[idx % optionColors.length]}`;
        button.textContent = option.optionText;
        button.dataset.optionId = option.optionId;
        button.dataset.questionId = question.questionId;

        // Restore previous selection
        if (userAnswers[question.questionId] == option.optionId) {
            button.classList.add('selected');
        }

        button.addEventListener('click', function() {
            selectOption(this);
        });

        optionsSection.appendChild(button);
    });

    // Update navigation buttons
    updateNavigationButtons();
}

// Select Option
function selectOption(button) {
    const questionId = button.dataset.questionId;
    const optionId = button.dataset.optionId;

    // Remove selection from other options
    const allOptions = document.querySelectorAll('.option-btn');
    allOptions.forEach(opt => {
        if (opt.dataset.questionId === questionId) {
            opt.classList.remove('selected');
        }
    });

    // Add selection to clicked option
    button.classList.add('selected');

    // Store answer
    userAnswers[questionId] = optionId;
}

// Update Progress
function updateProgress() {
    const answeredCount = Object.keys(userAnswers).length;
    const totalQuestions = quizData.questions.length;
    const percentage = (currentQuestionIndex / totalQuestions) * 100;

    document.getElementById('progressBar').style.width = percentage + '%';
    document.getElementById('progressText').textContent = `${currentQuestionIndex} / ${totalQuestions}`;
}

// Setup Navigation
function setupNavigation() {
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const submitBtn = document.getElementById('submitBtn');

    prevBtn.addEventListener('click', function() {
        if (currentQuestionIndex > 0) {
            currentQuestionIndex--;
            loadQuestion(currentQuestionIndex);
        }
    });

    nextBtn.addEventListener('click', function() {
        if (currentQuestionIndex < quizData.questions.length - 1) {
            currentQuestionIndex++;
            loadQuestion(currentQuestionIndex);
        }
    });

    submitBtn.addEventListener('click', function() {
        if (confirm('Are you sure you want to submit your quiz? You cannot change your answers after submission.')) {
            submitQuiz();
        }
    });
}

// Update Navigation Buttons
function updateNavigationButtons() {
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const submitBtn = document.getElementById('submitBtn');

    // Previous button
    prevBtn.disabled = currentQuestionIndex === 0;

    // Next/Submit buttons
    const isLastQuestion = currentQuestionIndex === quizData.questions.length - 1;
    
    if (isLastQuestion) {
        nextBtn.style.display = 'none';
        submitBtn.style.display = 'block';
    } else {
        nextBtn.style.display = 'block';
        submitBtn.style.display = 'none';
    }
}

// Submit Quiz
function submitQuiz() {
    const timeSpent = Math.floor((Date.now() - startTime) / 1000);

    // Get anti-forgery token
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!tokenElement) {
        console.error('Anti-forgery token not found');
        showError('Security token not found. Please refresh the page and try again.');
        return;
    }
    
    const token = tokenElement.value;

    // Create form data
    const formData = new FormData();
    formData.append('attemptId', quizData.attemptId);
    formData.append('__RequestVerificationToken', token);
    
    // Add all answers
    for (const [questionId, optionId] of Object.entries(userAnswers)) {
        formData.append(`answers[${questionId}]`, optionId);
    }

    console.log('Submitting quiz:', {
        attemptId: quizData.attemptId,
        answersCount: Object.keys(userAnswers).length,
        answers: userAnswers
    });

    // Show loading
    showLoading();

    // Submit to server
    fetch(quizData.submitUrl, {
        method: 'POST',
        body: formData
    })
    .then(response => {
        console.log('Response status:', response.status);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
    })
    .then(data => {
        console.log('Response data:', data);
        if (data.success) {
            showResults(data.score, data.maxScore);
        } else {
            showError(data.message || 'Failed to submit quiz');
            // Re-enable buttons on error
            document.getElementById('prevBtn').disabled = false;
            document.getElementById('nextBtn').disabled = false;
            document.getElementById('submitBtn').disabled = false;
            loadQuestion(currentQuestionIndex);
        }
    })
    .catch(error => {
        console.error('Error submitting quiz:', error);
        showError('An error occurred while submitting your quiz. Please try again.');
        // Re-enable buttons on error
        document.getElementById('prevBtn').disabled = false;
        document.getElementById('nextBtn').disabled = false;
        document.getElementById('submitBtn').disabled = false;
        loadQuestion(currentQuestionIndex);
    });
}

// Show Results
function showResults(score, maxScore) {
    // Hide quiz card
    document.querySelector('.quiz-card').style.display = 'none';
    
    // Update result values
    document.getElementById('scoreValue').textContent = Math.round(score);
    document.getElementById('maxScore').textContent = Math.round(maxScore);
    
    // Show result card with animation
    const resultCard = document.getElementById('resultCard');
    resultCard.style.display = 'block';
    
    // Update progress to 100%
    document.getElementById('progressBar').style.width = '100%';
    document.getElementById('progressText').textContent = `${quizData.questions.length} / ${quizData.questions.length}`;
}

// Show Loading
function showLoading() {
    const optionsSection = document.getElementById('optionsSection');
    optionsSection.innerHTML = '<div class="loading">Submitting your quiz</div>';
    
    // Disable all buttons
    document.getElementById('prevBtn').disabled = true;
    document.getElementById('nextBtn').disabled = true;
    document.getElementById('submitBtn').disabled = true;
}

// Show Error
function showError(message) {
    alert(message);
}

// Timer Function
function startTimer(seconds) {
    let remainingTime = seconds;
    
    const timerInterval = setInterval(function() {
        remainingTime--;
        
        // Update timer display (you can add a timer element if needed)
        
        if (remainingTime <= 0) {
            clearInterval(timerInterval);
            alert('Time is up! Your quiz will be submitted automatically.');
            submitQuiz();
        }
    }, 1000);
}

// Prevent leaving page accidentally
window.addEventListener('beforeunload', function(e) {
    if (Object.keys(userAnswers).length > 0 && document.getElementById('resultCard').style.display === 'none') {
        e.preventDefault();
        e.returnValue = '';
    }
});
