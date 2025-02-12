const inputs = document.querySelectorAll(".input-field input");
const button = document.querySelector("button");
const Otp = document.getElementById("Otp");

let OtpValue = "";

function reset() { 
    OtpValue = "";  
    Otp.value = "";  
    button.classList.remove("active");

    inputs.forEach((input, index) => {
        input.value = "";
        input.disabled = index !== 0; 
    });

    inputs[0].focus();
}


inputs.forEach((input, index) => {
    input.addEventListener("keydown", (e) => {
        if (e.key === "Enter" && OtpValue.length !== 6) {
            e.preventDefault();
        }
    });

    input.addEventListener("keyup", (e) => {

        const currentInput = input;
        const nextInput = inputs[index + 1];
        const prevInput = inputs[index - 1];

        if (currentInput.value.length === 6) {
            const value = currentInput.value.slice(0, 6); 
            for (let i = 0; i < value.length && i < inputs.length; i++) {
                inputs[i].value = value[i];
                inputs[i].disabled = false;
            }
            inputs[5].focus(); 
            OtpValue = value;
            Otp.value = OtpValue;
            
            if (OtpValue.length === 6) {
                button.classList.add("active");
                button.click();
                //wait for 1 second
                setTimeout(() => {
                    reset();
                }, 1000);
            }
            
            return;
        }

        if (currentInput.value.length > 1) {
            currentInput.value = currentInput.value[0];
        }

        // Update OTP value
        OtpValue = Array.from(inputs).map((inp) => inp.value).join("");
        Otp.value = OtpValue;

        if (nextInput && currentInput.value !== "") {
            nextInput.disabled = false;
            nextInput.focus();
        }

        if (e.key === "Backspace") {
            if (currentInput.value === "" && prevInput) {
                prevInput.focus();
                prevInput.value = ""; 
                currentInput.disabled = true; 
            }
            OtpValue = OtpValue.slice(0, -1);
            button.classList.remove("active");
        }

        // Activate button when OTP is complete
        if (OtpValue.length === 6) {
            button.classList.add("active");
        } else {
            button.classList.remove("active");
        }

        if (OtpValue.length === 6) {
            button.classList.add("active");
            button.click();
            //wait for 1 second
            setTimeout(() => {
                reset();
            }, 1000);
        }

    });
});

window.addEventListener("load", () => inputs[0].focus());
