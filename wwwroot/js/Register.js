document.addEventListener('DOMContentLoaded', () => {
    const image = document.getElementById('profileImage');
    const imageInput = document.querySelector('input[type="file"]');
    imageInput.addEventListener('change', (e) => {
        image.src = URL.createObjectURL(e.target.files[0]);
    });
    
    function toggleTagSelection(tagElement, tagId) {
        console.log(tagElement.classList);
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
    
});
