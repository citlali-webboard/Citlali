function escapeHtml(unsafe) {
    return unsafe.replace(/[&<>"']/g, function (match) {
        const escapeMap = {
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            '"': "&quot;",
            "'": "&#39;"
        };
        return escapeMap[match];
    });
}

document.addEventListener('DOMContentLoaded', function () {
    let addQuestionBtn = document.getElementById("add-question");
    let newQuestionInput = document.getElementById("new-question");
    let questionList = document.querySelector(".added-question ol");

    // Question input handling
    newQuestionInput.addEventListener("keypress", function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            addQuestionBtn.click();
        }
    });

    updateQuestionsVisibility();

    addQuestionBtn.addEventListener("click", function () {
        let questionText = newQuestionInput.value.trim();
        
        if (questionText) {
            let newQuestionItem = document.createElement("li");
            newQuestionItem.classList.add("question-item");
            
            newQuestionItem.innerHTML = `
                <input type="hidden" name="Questions[]" value="${escapeHtml(questionText)}" />
                <a class="popup-action-button" href="javascript:void(0);" onclick="removeQuestion(this)">
                    <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M10 11V17" stroke="#000000" stroke-width="0.984" stroke-linecap="round" stroke-linejoin="round"></path> <path d="M14 11V17" stroke="#000000" stroke-width="0.984" stroke-linecap="round" stroke-linejoin="round"></path> <path d="M4 7H20" stroke="#000000" stroke-width="0.984" stroke-linecap="round" stroke-linejoin="round"></path> <path d="M6 7H12H18V18C18 19.6569 16.6569 21 15 21H9C7.34315 21 6 19.6569 6 18V7Z" stroke="#000000" stroke-width="0.984" stroke-linecap="round" stroke-linejoin="round"></path> <path d="M9 5C9 3.89543 9.89543 3 11 3H13C14.1046 3 15 3.89543 15 5V7H9V5Z" stroke="#000000" stroke-width="0.984" stroke-linecap="round" stroke-linejoin="round"></path> </g></svg>
                </a>
                <p>${escapeHtml(questionText)}</p>
            `;
            
            questionList.appendChild(newQuestionItem);
            newQuestionInput.value = '';

            updateQuestionsVisibility();

            newQuestionItem.classList.add('highlight');
            setTimeout(() => {
                newQuestionItem.classList.remove('highlight');
            }, 1000);
        } else {
            // Show validation error for empty question
            showFieldError(newQuestionInput, "Please enter a valid question");
        }
    });

    // Validation system setup
    setupFormValidation();
    
    // Add custom validators
    addCustomValidators();

    const fcfsCheckbox = document.querySelector('.fcfs input[type="checkbox"]');
    if (fcfsCheckbox) {
        // Make sure the toggle works correctly by adding direct event listeners
        fcfsCheckbox.addEventListener('change', function() {
            // No need to update hidden inputs - ASP.NET will handle this automatically
        });

        // Add click handler for the parent label to prevent click blocking
        const slideBtn = document.querySelector('.slide-btn');
        if (slideBtn) {
            slideBtn.addEventListener('click', function(e) {
                // Prevent default only if clicking directly on the label (not the checkbox)
                if (e.target !== fcfsCheckbox) {
                    e.preventDefault();
                    // Toggle checkbox state
                    fcfsCheckbox.checked = !fcfsCheckbox.checked;
                    // Trigger change event
                    fcfsCheckbox.dispatchEvent(new Event('change'));
                }
            });
        }
    }


});

// Form validation setup
function setupFormValidation() {
    // Get all form inputs, textareas and selects
    const formElements = document.querySelectorAll('input:not([type="hidden"]), textarea, select');
    
    // Add blur (focus lost) event listener to each form element
    formElements.forEach(element => {
        element.addEventListener('blur', function() {
            // Add touched class when user leaves the field
            this.classList.add('touched');
            
            // Validate the field
            validateField(this);
        });
        
        // Also validate on input change for better UX
        element.addEventListener('input', function() {
            if (this.classList.contains('touched')) {
                validateField(this);
            }
        });
        
        // For selects, also validate on change
        if (element.tagName === 'SELECT') {
            element.addEventListener('change', function() {
                this.classList.add('touched');
                validateField(this);
            });
        }
    });
    
    // Add form submit handler
    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function(e) {
            // Mark all fields as touched
            formElements.forEach(element => {
                element.classList.add('touched');
                validateField(element);
            });
            
            // Check if form is valid
            if (!isFormValid()) {
                e.preventDefault();
                
                // Scroll to the first invalid field
                const firstInvalid = document.querySelector('.touched:invalid');
                if (firstInvalid) {
                    firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    firstInvalid.focus();
                }
            } else {
                // Check for at least one question (if questions are required)
                const questions = document.querySelectorAll('input[name="Questions[]"]');
                if (questions.length === 0) {
                    e.preventDefault();
                    showFieldError(document.getElementById('new-question'), 'Please add at least one question for participants');
                    document.getElementById('new-question').scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
            }
        });
    }
}

// Validate a single field
function validateField(field) {
    // Check validity using HTML5 validation API
    const isValid = field.checkValidity();
    
    // Find or create error message element
    let errorMessage = field.nextElementSibling;
    if (!errorMessage || !errorMessage.classList.contains('field-validation-error')) {
        errorMessage = document.createElement('span');
        errorMessage.className = 'field-validation-error';
        field.parentNode.insertBefore(errorMessage, field.nextSibling);
    }
    
    // Toggle validation classes and display error
    if (isValid) {
        field.classList.remove('input-validation-error');
        errorMessage.textContent = '';
        errorMessage.style.display = 'none';
    } else {
        field.classList.add('input-validation-error');
        errorMessage.textContent = field.validationMessage || 'This field is not valid';
        errorMessage.style.display = 'block';
    }
}

// Show error message for a field
function showFieldError(field, message) {
    field.classList.add('touched');
    field.classList.add('input-validation-error');
    field.setCustomValidity(message);
    
    // Find or create error message element
    let errorMessage = field.nextElementSibling;
    if (!errorMessage || !errorMessage.classList.contains('field-validation-error')) {
        errorMessage = document.createElement('span');
        errorMessage.className = 'field-validation-error';
        field.parentNode.insertBefore(errorMessage, field.nextSibling);
    }
    
    errorMessage.textContent = message;
    errorMessage.style.display = 'block';
    
    // Clear custom validity after a short delay (for next validation)
    setTimeout(() => {
        field.setCustomValidity('');
    }, 100);
}

// Check if entire form is valid
function isFormValid() {
    const invalidFields = document.querySelectorAll('.touched:invalid');
    return invalidFields.length === 0;
}

// Remove Question functionality
function removeQuestion(button) {
    // Get the parent list item of the clicked remove button
    let questionItem = button.closest(".question-item");
    
    // Add fade-out animation
    questionItem.classList.add('removing');
    
    // Remove after animation completes
    questionItem.addEventListener('transitionend', function() {
        questionItem.remove();
        updateQuestionsVisibility();
    });
    
    // Fallback in case transition doesn't trigger the event
    setTimeout(() => {
        if (questionItem.parentNode) {
            questionItem.remove();
            updateQuestionsVisibility();
        }
    }, 300);
}

// Add custom validators
function addCustomValidators() {
    // Function to check if event date is in the future
    function validateEventDate() {
        const eventDateField = document.getElementById('EventDate');
        if (eventDateField) {
            eventDateField.addEventListener('blur', function() {
                if (this.value) {
                    const now = new Date();
                    const eventDate = new Date(this.value);
                    if (eventDate <= now) {
                        this.setCustomValidity('Event date must be in the future');
                    } else {
                        this.setCustomValidity('');
                    }
                    validateField(this);
                }
            });
            
            // Initial validation with no error message yet
            if (eventDateField.value) {
                const now = new Date();
                const eventDate = new Date(eventDateField.value);
                if (eventDate <= now) {
                    eventDateField.setCustomValidity('Event date must be in the future');
                } else {
                    eventDateField.setCustomValidity('');
                }
            }
        }
    }
    
    // Function to check if post expiry date is after event date
    function validateExpiryDate() {
        const expiryDateField = document.getElementById('PostExpiryDate');
        const eventDateField = document.getElementById('EventDate');
        
        if (expiryDateField && eventDateField) {
            expiryDateField.addEventListener('blur', function() {
                if (this.value && eventDateField.value) {
                    const eventDate = new Date(eventDateField.value);
                    const expiryDate = new Date(this.value);
                    if (expiryDate > eventDate) {
                        this.setCustomValidity('Post expiry date must be before the event date');
                    } else {
                        this.setCustomValidity('');
                    }
                    validateField(this);
                }
            });
            
            // Also re-validate expiry date when event date changes
            eventDateField.addEventListener('change', function() {
                if (expiryDateField.value) {
                    const eventDate = new Date(this.value);
                    const expiryDate = new Date(expiryDateField.value);
                    if (expiryDate > eventDate) {
                        expiryDateField.setCustomValidity('Post expiry date must be before the event date');
                    } else {
                        expiryDateField.setCustomValidity('');
                    }
                    
                    if (expiryDateField.classList.contains('touched')) {
                        validateField(expiryDateField);
                    }
                }
            });
            
            // Initial validation
            if (expiryDateField.value && eventDateField.value) {
                const eventDate = new Date(eventDateField.value);
                const expiryDate = new Date(expiryDateField.value);
                if (expiryDate > eventDate) {
                    expiryDateField.setCustomValidity('Post expiry date must be before the event date');
                } else {
                    expiryDateField.setCustomValidity('');
                }
            }
        }
    }
    
    // Function to validate positive values
    function validatePositiveNumbers() {
        const costField = document.getElementById('Cost');
        const maxParticipantField = document.getElementById('MaxParticipant');
        
        if (costField) {
            costField.addEventListener('blur', function() {
                if (this.value && parseFloat(this.value) < 0) {
                    this.setCustomValidity('Cost cannot be negative');
                } else {
                    this.setCustomValidity('');
                }
                validateField(this);
            });
        }
        
        if (maxParticipantField) {
            maxParticipantField.addEventListener('blur', function() {
                if (this.value && parseInt(this.value) <= 0) {
                    this.setCustomValidity('Maximum participants must be greater than 0');
                } else {
                    this.setCustomValidity('');
                }
                validateField(this);
            });
            
            // Check for required
            maxParticipantField.addEventListener('invalid', function(e) {
                if (!this.value) {
                    this.setCustomValidity('Maximum participants is required');
                }
            });
        }
    }
    
    // Run all validators
    validateEventDate();
    validateExpiryDate();
    validatePositiveNumbers();
}

// Add a debounce function to limit function calls
function debounce(func, wait) {
    let timeout;
    return function(...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}

function updateQuestionsVisibility() {
    const questionContainer = document.querySelector('.added-question');
    const questionItems = document.querySelectorAll('.question-item');
    
    if (questionItems.length > 0) {
        questionContainer.classList.remove('no-questions');
    } else {
        questionContainer.classList.add('no-questions');
    }
}