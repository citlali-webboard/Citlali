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

    let now = new Date();

    // Convert to Bangkok Time in YYYY-MM-DDTHH:MM format
    let options = {
        timeZone: "Asia/Bangkok",
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
        hour: "2-digit",
        minute: "2-digit",
        hour12: false
    };

    let formattedDateTime = new Intl.DateTimeFormat("en-US", options).format(now)
        .replace(",", "") // Remove comma
        .replace(/(\d+)\/(\d+)\/(\d+)/, "$3-$1-$2") //YYYY-MM-DD
        .replace(" ", "T"); // Add "T" separator

    console.log(formattedDateTime);

    document.getElementById("EventDate").value = formattedDateTime;
    document.getElementById("PostExpiryDate").value = formattedDateTime;

    newQuestionInput.addEventListener("keypress", function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            addQuestionBtn.click();
        }
    });

    addQuestionBtn.addEventListener("click", function () {
        let questionText = newQuestionInput.value.trim();
        
        if (questionText) {
            let newQuestionItem = document.createElement("li");
            newQuestionItem.classList.add("question-item");
            
            newQuestionItem.innerHTML = `
                <input type="hidden" name="Questions[]" value="${questionText}" />
                <a class="popup-action-button" href="javascript:void(0);" onclick="removeQuestion(this)">
                    <svg viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g id="SVGRepo_bgCarrier" stroke-width="0"></g><g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round"></g><g id="SVGRepo_iconCarrier"> <path d="M10 11V17" stroke="#000000" stroke-width="0.984" stroke-linecap="round" stroke-linejoin="round"></path> <path d="M14 11V17" stroke="#000000" stroke-width="0.984" stroke-linecap="round" stroke-linejoin="round"></path> <path d="M4 7H20" stroke="#000000" stroke-width="0.984" stroke-linecap="round" stroke-linejoin="round"></path> <path d="M6 7H12H18V18C18 19.6569 16.6569 21 15 21H9C7.34315 21 6 19.6569 6 18V7Z" stroke="#000000" stroke-width="0.984" stroke-linecap="round" stroke-linejoin="round"></path> <path d="M9 5C9 3.89543 9.89543 3 11 3H13C14.1046 3 15 3.89543 15 5V7H9V5Z" stroke="#000000" stroke-width="0.984" stroke-linecap="round" stroke-linejoin="round"></path> </g></svg>
                </a>
                <p>${escapeHtml(questionText)}</p>
            `;
            
            questionList.appendChild(newQuestionItem);
            newQuestionInput.value = '';
        }
    });
});

// Remove Question functionality
function removeQuestion(button) {
    // Get the parent list item of the clicked remove button
    let questionItem = button.closest(".question-item");
    
    // Remove the question item from the list
    questionItem.remove();
}
