// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const profileContainer = document.querySelector(".profile-container");
    const profileDropdown = document.querySelector(".profile-dropdown");
    let hideTimeout;

    profileContainer.addEventListener("mouseenter", function () {
        clearTimeout(hideTimeout);
        profileDropdown.style.display = "flex";
        setTimeout(() => {
            profileDropdown.style.opacity = "1";
        }, 10); // Slight delay to allow transition
    });

    profileContainer.addEventListener("mouseleave", function () {
        hideTimeout = setTimeout(function () {
            profileDropdown.style.opacity = "0";
            setTimeout(() => {
                profileDropdown.style.display = "none";
            }, 500); // Wait for fade-out transition before hiding
        }, 2000); // Dropdown stays visible for 2 sec before hiding
    });

    profileDropdown.addEventListener("mouseenter", function () {
        clearTimeout(hideTimeout);
    });

    profileDropdown.addEventListener("mouseleave", function () {
        hideTimeout = setTimeout(function () {
            profileDropdown.style.opacity = "0";
            setTimeout(() => {
                profileDropdown.style.display = "none";
            }, 500);
        }, 2000);
    });
});

