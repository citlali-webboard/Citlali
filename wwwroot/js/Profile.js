
function openPopup() {
    document.getElementById('edit-popup').style.display = 'flex';
}

function closePopup() {
    console.log('close');
    document.getElementById('edit-popup').style.display = 'none';
}

document.addEventListener("DOMContentLoaded", function () {
    let fileInput = document.getElementById("profileImage");
    let imgPreview = document.getElementById("profilePreview");


    imgPreview.addEventListener("click", function () {
        fileInput.click();
    });

    // preview image
    fileInput.addEventListener("change", function (event) {
        let file = event.target.files[0];
        if (file) {
            let reader = new FileReader();
            reader.onload = function (e) {
                imgPreview.src = e.target.result;
            };
            reader.readAsDataURL(file);
        }
    });
});

