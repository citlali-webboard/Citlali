const inputs = document.querySelectorAll(".input-field input");
const button = document.querySelector("button");

console.log(inputs, button);

inputs.forEach((input, index) => {
    input.addEventListener("keyup", (e) => {
        if (e.key === "Enter") {
            console.log("Enter key pressed");
            e.preventDefault(); // Prevent form submission
            return;
        }

        if(!inputs[5].disabled && inputs[5].value.length == 1){
            button.classList.add("active");
        }
        else{
            button.classList.remove("active");
        }

        const currentInput = input;
        const nextInput = inputs[index + 1]; 
        const prevInput = inputs[index - 1]; 

        // if user pastes OTP 
        if (currentInput.value.length == 6) {
            const value = currentInput.value;
            for (let i = 0; i < value.length && i < inputs.length; i++) {
                inputs[i].value = value[i];
                inputs[i].disabled = false;
            }
            inputs[5].focus(); 
            return;
        }

      
        if (currentInput.value.length > 1) {
            currentInput.value = "";
        }

        // Backspace key handling
        if (e.key === "Backspace") {
            if (currentInput.value === "" && prevInput) {
                prevInput.focus();
                prevInput.value = ""; 
                currentInput.disabled = true; 
            }

            if(currentInput.value === ""){
                button.classList.remove("active");
            }

        }

        if (nextInput && currentInput.value !== "") {
            nextInput.disabled = false;
            nextInput.focus();
        }

    });
});

window.addEventListener("load", () => inputs[0].focus());
