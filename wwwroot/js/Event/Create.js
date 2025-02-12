document.addEventListener('DOMContentLoaded', function () {
    let addQuestionBtn = document.getElementById("add-question");
    let newQuestionInput = document.getElementById("new-question");
    let questionList = document.querySelector(".added-question ol");

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
                    <span>❌</span>
                </a>
                <p>${questionText}</p>
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
