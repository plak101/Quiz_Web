/* ===================================
   INDEX.JS - FLASHCARD LEARNING LOGIC
   ================================== */

// Global variables
let flashcards = [];
let currentCardIndex = 0;
let isFlipped = false;
let totalCards = 0;
let currentSetId = 0;

/**
 * Initialize flashcards with data
 * @param {Array} data - Array of flashcard objects with FrontText and BackText
 * @param {Number} total - Total number of flashcards
 * @param {Number} setId - Current flashcard set ID
 */
function initializeFlashcards(data, total, setId) {
    flashcards = data;
    totalCards = total;
    currentCardIndex = 0;
    isFlipped = false;
    currentSetId = setId || 0;
    
    console.log('Initializing flashcards:', flashcards.length, 'cards');
    
    if (flashcards.length > 0) {
        updateCard();
    }
}

/**
 * Update the current card display
 */
function updateCard() {
    const frontText = document.getElementById('frontText');
    const backText = document.getElementById('backText');
    
    if (frontText && backText && flashcards.length > 0) {
        frontText.textContent = flashcards[currentCardIndex].frontText || flashcards[currentCardIndex].FrontText || 'No text';
        backText.textContent = flashcards[currentCardIndex].backText || flashcards[currentCardIndex].BackText || 'No text';
    }
    
    // Reset flip state
    const card = document.getElementById('flashcard');
    if (card && isFlipped) {
        card.classList.remove('flipped');
        isFlipped = false;
    }
    
    updateCounter();
    updateProgress();
    updateNavigationButtons();
    checkCompletion();
}

/**
 * Update navigation button states
 */
function updateNavigationButtons() {
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    
    if (prevBtn) {
        prevBtn.disabled = currentCardIndex === 0;
    }
    
    if (nextBtn) {
        nextBtn.disabled = currentCardIndex >= flashcards.length - 1;
    }
}

/**
 * Check if all flashcards are completed and show completion button
 */
function checkCompletion() {
    const progress = ((currentCardIndex + 1) / totalCards) * 100;
    const completeSection = document.getElementById('completeSection');
    
    if (completeSection) {
        if (progress >= 100) {
            completeSection.style.display = 'block';
            completeSection.classList.add('show-complete');
        } else {
            completeSection.style.display = 'none';
            completeSection.classList.remove('show-complete');
        }
    }
}

/**
 * Navigate to finish page
 */
function goToFinish() {
    window.location.href = `/Flashcard/Finish?setId=${currentSetId}`;
}

/**
 * Flip the current card
 */
function flipCard() {
    const card = document.getElementById('flashcard');
    if (card) {
        card.classList.toggle('flipped');
        isFlipped = !isFlipped;
    }
}

/**
 * Navigate to next card
 */
function nextCard() {
    if (currentCardIndex < flashcards.length - 1) {
        currentCardIndex++;
        updateCard();
    }
}

/**
 * Navigate to previous card
 */
function previousCard() {
    if (currentCardIndex > 0) {
        currentCardIndex--;
        updateCard();
    }
}

/**
 * Update card counter display
 */
function updateCounter() {
    const counter = document.getElementById('cardCounter');
    if (counter) {
        counter.textContent = `${currentCardIndex + 1} / ${totalCards} Flashcards`;
    }
}

/**
 * Update progress bar and percentage
 */
function updateProgress() {
    const progress = ((currentCardIndex + 1) / totalCards) * 100;
    const progressFill = document.getElementById('progressFill');
    const progressText = document.getElementById('progressText');
    
    if (progressFill) {
        progressFill.style.width = progress + '%';
    }
    if (progressText) {
        progressText.textContent = Math.round(progress) + '%';
    }
}

// ===================================
// EVENT LISTENERS
// ===================================

// Keyboard navigation
document.addEventListener('keydown', function(event) {
    if (event.code === 'Space') {
        event.preventDefault();
        flipCard();
    } else if (event.code === 'ArrowLeft') {
        event.preventDefault();
        previousCard();
    } else if (event.code === 'ArrowRight') {
        event.preventDefault();
        nextCard();
    }
});

// Click to flip card
document.addEventListener('DOMContentLoaded', function() {
    const flashcard = document.getElementById('flashcard');
    if (flashcard) {
        flashcard.addEventListener('click', function(event) {
            flipCard();
        });
    }
});
