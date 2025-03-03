const inputs = document.querySelectorAll(".input-field input");
const button = document.querySelector("button");
const otpInput = document.getElementById("Otp");

// Reset the form to initial state
function reset() {
    otpInput.value = "";
    button.classList.remove("active");

    inputs.forEach((input, index) => {
        input.value = "";
        input.disabled = index !== 0;
    });

    inputs[0].focus();
}

// Update the hidden OTP field value and check completion
function updateOtpValue() {
    const otpValue = Array.from(inputs).map(input => input.value).join("");
    otpInput.value = otpValue;
    
    const isComplete = otpValue.length === 6;
    
    if (isComplete) {
        button.classList.add("active");
        button.click();
        setTimeout(reset, 1000);
    } else {
        button.classList.remove("active");
    }
    
    return { otpValue, isComplete };
}

// Handle pasting an OTP code
function handlePaste(e, index) {
    e.preventDefault();
    
    const pastedData = (e.clipboardData || window.clipboardData).getData("text").trim();
    // Only allow digits (0-9), no minus sign or other characters
    if (!pastedData.match(/^[0-9]+$/)) return;
    
    const digits = pastedData.slice(0, 6).split("");
    
    digits.forEach((digit, i) => {
        if (index + i < inputs.length) {
            inputs[index + i].value = digit;
            inputs[index + i].disabled = false;
        }
    });
    
    inputs[Math.min(index + digits.length - 1, inputs.length - 1)].focus();


    
    updateOtpValue();
}

// Setup event listeners for all input fields
inputs.forEach((input, index) => {
    // Validate keypress - prevent non-digit keys including minus sign
    input.addEventListener("keypress", (e) => {
        // Allow only digit keys (0-9) and control keys
        if (!/^\d$/.test(e.key)) {
            e.preventDefault();
        }
    });
    
    // Prevent form submission when pressing Enter
    input.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            e.preventDefault();
            if (otpInput.value.length === 6) {
                button.click();
            }
        }
        
        // Handle backspace key
        if (e.key === "Backspace") {
            const prevInput = inputs[index - 1];
            
            if (input.value === "" && prevInput) {
                prevInput.focus();
                prevInput.value = "";
                input.disabled = true;
                e.preventDefault(); // Prevent double backspace
            }
            
            setTimeout(updateOtpValue, 0); // Update after the current event
        }
    });
    
    // Handle paste events
    input.addEventListener("paste", (e) => handlePaste(e, index));

    // Handle input changes
    input.addEventListener("input", (e) => {
        const currentInput = input;
        const nextInput = inputs[index + 1];
        
        // Ensure only digits are entered (extra validation)
        currentInput.value = currentInput.value.replace(/[^0-9]/g, '');
        
        // Handle multi-character input (e.g. from autocomplete)
        if (currentInput.value.length > 1) {
            // Distribute characters across fields
            const chars = currentInput.value.split("");
            currentInput.value = chars[0];
            
            chars.slice(1).forEach((char, i) => {
                if (index + i + 1 < inputs.length) {
                    inputs[index + i + 1].disabled = false;
                    inputs[index + i + 1].value = char;
                }
            });
        }
        
        // Auto-move to next field if available
        if (nextInput && currentInput.value !== "") {
            nextInput.disabled = false;
            nextInput.focus();
        }
        
        updateOtpValue();
    });
});

// Focus first input on page load
window.addEventListener("load", () => inputs[0].focus());