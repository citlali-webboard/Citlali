document.addEventListener('DOMContentLoaded', () => {
    const image = document.getElementById('profileImage');
    const imageInput = document.querySelector('input[type="file"]');
    let currentImageSrc = image.src;

    imageInput.addEventListener('change', (e) => {
            if (e.target.files && e.target.files[0]) {
                const file = e.target.files[0];
                
                // Validate before updating preview
                if (validateImageFile(file)) {
                    // Valid file, update the preview
                    currentImageSrc = URL.createObjectURL(file);
                    image.src = currentImageSrc;
                } else {
                    // Invalid file, reset the file input
                    imageInput.value = '';
                }
            }
    });
    
    function toggleTagSelection(tagElement, tagId) {
        let inputField = document.getElementById(`tagInput_${tagId}`);
        
        if (tagElement.classList.contains("selected")) {
            tagElement.classList.remove("selected");
            inputField.value = "";
        } else {
            tagElement.classList.add("selected");
            inputField.value = tagId;
        }
    }

    window.toggleTagSelection = toggleTagSelection;
    
    // Setup form validation
    setupFormValidation();
    
    // Add custom validators
    addCustomValidators();

    const bioField = document.getElementById('UserBio');
    const bioCharCount = document.getElementById('bioCharCount');

    if (bioField && bioCharCount) {
        // Update character count on load
        bioCharCount.textContent = bioField.value.length;
        
        // Update character count as user types
        bioField.addEventListener('input', function() {
            bioCharCount.textContent = this.value.length;
            
            // Visual indication when approaching limit
            if (this.value.length > 450) {
                bioCharCount.classList.add('char-count-warning');
            } else {
                bioCharCount.classList.remove('char-count-warning');
            }
        });
    }
    
});

// Form validation setup
function setupFormValidation() {
    // Get all form inputs and textareas
    const formElements = document.querySelectorAll('input:not([type="hidden"]):not([disabled]), textarea');
    
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
                
                // Show validation summary
                showValidationSummary();
                
                // Scroll to the first invalid field
                const firstInvalid = document.querySelector('.touched:invalid');
                if (firstInvalid) {
                    firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
                    firstInvalid.focus();
                }
            }
        });
    }
}

// Validate image file size and type
function validateImageFile(file) {
    const maxSize = 10 * 1024 * 1024; // 10 MB
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png'];
    const imageInput = document.getElementById('ProfileImageInput');
    
    if (file.size > maxSize) {
        showFieldError(imageInput, 'Image size should be less than 10MB');
        return false;
    }
    
    if (!allowedTypes.includes(file.type)) {
        showFieldError(imageInput, 'Only JPEG, JPG and PNG files are allowed');
        return false;
    }
    
    return true;
}

// Validate a single field
function validateField(field) {
    // Check validity using HTML5 validation API
    const isValid = field.checkValidity();
    
    // Find or create error message element
    let errorMessage = field.nextElementSibling;
    if (!errorMessage || !errorMessage.classList.contains('text-danger')) {
        errorMessage = field.parentNode.querySelector('.text-danger');
        if (!errorMessage) {
            errorMessage = document.createElement('span');
            errorMessage.className = 'text-danger';
            field.parentNode.appendChild(errorMessage);
        }
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
    if (!errorMessage || !errorMessage.classList.contains('text-danger')) {
        errorMessage = field.parentNode.querySelector('.text-danger');
        if (!errorMessage) {
            errorMessage = document.createElement('span');
            errorMessage.className = 'text-danger';
            field.parentNode.appendChild(errorMessage);
        }
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

// Show validation summary at top of form
function showValidationSummary() {
    // Remove existing summary if present
    const existingSummary = document.querySelector('.validation-summary-errors');
    if (existingSummary) {
        existingSummary.remove();
    }
    
    // Get all invalid fields
    const invalidFields = document.querySelectorAll('.touched:invalid');
    if (invalidFields.length === 0) return;
    
    // Create summary element
    const summary = document.createElement('div');
    summary.className = 'validation-summary-errors';
    
    // Create title and list
    const title = document.createElement('p');
    title.textContent = 'Please fix the following errors:';
    summary.appendChild(title);
    
    const list = document.createElement('ul');
    invalidFields.forEach(field => {
        const item = document.createElement('li');
        
        // Get field label text
        const labelElement = document.querySelector(`label[for="${field.id}"]`);
        const fieldName = labelElement ? labelElement.textContent.replace(' *', '') : field.id;
        
        item.textContent = `${fieldName}: ${field.validationMessage || 'Invalid value'}`;
        list.appendChild(item);
    });
    
    summary.appendChild(list);
    
    // Insert at top of form
    const form = document.querySelector('form');
    form.insertBefore(summary, form.firstChild);
}

// Add custom validators
function addCustomValidators() {
    // Username validation - alphanumeric with underscores
    const usernameField = document.getElementById('Username');
    if (usernameField) {
        usernameField.addEventListener('blur', function() {
            if (this.value) {
                if (this.value.length < 3) {
                    this.setCustomValidity('Username must be at least 3 characters long');
                } else if (this.value.length > 30) {
                    this.setCustomValidity('Username cannot exceed 30 characters');
                } else if (!/^[A-Za-z][A-Za-z0-9_]{3,29}$/.test(this.value)) {
                    this.setCustomValidity('Username must start with a letter and can only contain letters, numbers, and underscores');
                } else {
                    this.setCustomValidity('');
                    validateField(this);
                }
            }
        });
    }
    
    // Display name validation
    const displayNameField = document.getElementById('DisplayName');
    if (displayNameField) {
        displayNameField.addEventListener('blur', function() {
            if (this.value) {
                if (this.value.length < 2) {
                    this.setCustomValidity('Display name must be at least 2 characters long');
                } else if (this.value.length > 50) {
                    this.setCustomValidity('Display name cannot exceed 50 characters');
                } else {
                    this.setCustomValidity('');
                }
                validateField(this);
            }
        });
    }
    
    // Bio validation (optional, just length check)
    const bioField = document.getElementById('UserBio');
    if (bioField) {
        bioField.addEventListener('blur', function() {
            if (this.value && this.value.length > 500) {
                this.setCustomValidity('Bio cannot exceed 500 characters');
            } else {
                this.setCustomValidity('');
            }
            validateField(this);
        });
    }
}