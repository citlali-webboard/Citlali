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
    setupFormValidation();
    addCustomValidators();

    const fcfsCheckbox = document.querySelector('.fcfs input[type="checkbox"]');
    if (fcfsCheckbox) {
        // Make sure the toggle works correctly by adding direct event listeners
        fcfsCheckbox.addEventListener('change', function () {
            // No need to update hidden inputs - ASP.NET will handle this automatically
        });

        // Add click handler for the parent label to prevent click blocking
        const slideBtn = document.querySelector('.slide-btn');
        if (slideBtn) {
            slideBtn.addEventListener('click', function (e) {
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

function setupFormValidation() {
    const formElements = document.querySelector('#create-event-form').querySelectorAll('input:not([type="hidden"]):not([id="FirstComeFirstServed"]), textarea, select');

    // Add blur (focus lost) event listener to each form element
    formElements.forEach(element => {
        element.addEventListener('blur', function () {
            this.classList.add('touched');
            validateField(this);
        });

        // Also validate on input change for better UX
        element.addEventListener('input', function () {
            if (this.classList.contains('touched')) {
                validateField(this);
            }
        });

        // For selects, also validate on change
        if (element.tagName === 'SELECT') {
            element.addEventListener('change', function () {
                this.classList.add('touched');
                validateField(this);
            });
        }
    });

    // Add form submit handler
    const form = document.querySelector('#edit-event-form');
    if (form) {
        form.addEventListener('submit', function (e) {
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
                    firstInvalid.scrollIntoView({
                        behavior: 'smooth',
                        block: 'center'
                    });
                    firstInvalid.focus();
                }
            } else {

            }
        });
    }
}

// Validate a single field
function validateField(field) {
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

function addCustomValidators() {
    // Get date-related fields
    const eventDateField = document.getElementById('EventDate');
    const expiryDateField = document.getElementById('PostExpiryDate');

    /**
     * Comprehensive date validation function that handles both fields
     * Called whenever either field changes to ensure consistent validation
     */
    function validateDates() {
        if (!eventDateField || !expiryDateField) return;
        if (!eventDateField.value || !expiryDateField.value) return;
    
        const now = new Date();
        const eventDate = new Date(eventDateField.value);
        const expiryDate = new Date(expiryDateField.value);
        
        // Get original dates from data attributes for comparison
        const originalEventDate = eventDateField.dataset.originalDate ? new Date(eventDateField.dataset.originalDate) : null;
        const originalExpiryDate = expiryDateField.dataset.originalDate ? new Date(expiryDateField.dataset.originalDate) : null;
    
        // Check if dates have been changed
        const isEventDateChanged = originalEventDate && eventDate.getTime() !== originalEventDate.getTime();
        const isExpiryDateChanged = originalExpiryDate && expiryDate.getTime() !== originalExpiryDate.getTime();
        
        // Check if new dates are in the past
        const isNewEventDateInPast = eventDate <= now;
        const isNewExpiryDateInPast = expiryDate <= now;
    
        // Clear previous validations
        eventDateField.setCustomValidity('');
        expiryDateField.setCustomValidity('');
    
        // Validation rules
        
        // Rule 1: If event date is being changed, it should be in the future
        if (isEventDateChanged && isNewEventDateInPast) {
            eventDateField.setCustomValidity('When changing event date, it must be set to a future date');
        } 
        // Rule 2: If expiry date is being changed, it should be in the future
        else if (isExpiryDateChanged && isNewExpiryDateInPast) {
            expiryDateField.setCustomValidity('When changing post expiry date, it must be set to a future date');
        }
        // Rule 3: Event date must be AFTER post expiry date
        else if (eventDate <= expiryDate) {
            eventDateField.setCustomValidity('Event date must be after post expiry date');
        }
    
        // Update validation UI for both fields
        if (eventDateField.classList.contains('touched')) {
            validateField(eventDateField);
        }
        if (expiryDateField.classList.contains('touched')) {
            validateField(expiryDateField);
        }
    }

    // Function to validate positive numbers
    function validatePositiveNumbers() {
        const costField = document.getElementById('Cost');
        const maxParticipantField = document.getElementById('MaxParticipant');

        if (costField) {
            costField.addEventListener('blur', function () {
                if (this.value && parseFloat(this.value) < 0) {
                    this.setCustomValidity('Cost cannot be negative');
                } else {
                    this.setCustomValidity('');
                }
                validateField(this);
            });

            // Also validate on input for better UX
            costField.addEventListener('input', debounce(function () {
                if (this.classList.contains('touched')) {
                    if (this.value && parseFloat(this.value) < 0) {
                        this.setCustomValidity('Cost cannot be negative');
                    } else {
                        this.setCustomValidity('');
                    }
                    validateField(this);
                }
            }, 300));
        }

        if (maxParticipantField) {
            // Get current participant count from label text
            let currentParticipants = 0;
            const label = document.querySelector('label[for="MaxParticipant"]');
            if (label) {
                const match = label.textContent.match(/Current: (\d+)/);
                if (match && match[1]) {
                    currentParticipants = parseInt(match[1]);
                }
            }

            const validateMaxParticipants = function () {
                if (!this.value) {
                    this.setCustomValidity('Maximum participants is required');
                } else if (parseInt(this.value) <= 0) {
                    this.setCustomValidity('Maximum participants must be greater than 0');
                } else if (parseInt(this.value) < currentParticipants) {
                    this.setCustomValidity(`Cannot set max participants lower than current participants (${currentParticipants})`);
                } else {
                    this.setCustomValidity('');
                }
                validateField(this);
            };

            maxParticipantField.addEventListener('blur', validateMaxParticipants);
            maxParticipantField.addEventListener('input', debounce(function () {
                if (this.classList.contains('touched')) {
                    validateMaxParticipants.call(this);
                }
            }, 300));
        }
    }

    // Add event listeners for date fields
    if (eventDateField && expiryDateField) {
        // Validate on blur
        eventDateField.addEventListener('blur', validateDates);
        expiryDateField.addEventListener('blur', validateDates);

        // Validate on change
        eventDateField.addEventListener('change', validateDates);
        expiryDateField.addEventListener('change', validateDates);

        // Validate on input (with debounce to avoid too many validations)
        eventDateField.addEventListener('input', debounce(validateDates, 300));
        expiryDateField.addEventListener('input', debounce(validateDates, 300));

        // Initial validation on page load
        validateDates();
    }

    // Run validators
    validatePositiveNumbers();
}

// Add a debounce function to limit function calls
function debounce(func, wait) {
    let timeout;
    return function (...args) {
        clearTimeout(timeout);
        timeout = setTimeout(() => func.apply(this, args), wait);
    };
}