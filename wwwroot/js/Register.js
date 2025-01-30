console.log('Register.js loaded');
const image = document.getElementById('profileImage');
const input = document.querySelector('input[type="file"]');


input.addEventListener('change', (e) => {
    console.log('input changed');
    image.src = URL.createObjectURL(e.target.files[0]);
});