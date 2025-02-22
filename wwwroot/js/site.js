// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const profileContainer = document.querySelector(".profile-container");
    const profileDropdown = document.querySelector(".profile-dropdown");
    let hideTimeout;
    let isDropdownOpen = false;

    function isMobileDevice() {
        return window.matchMedia("(max-width: 768px)").matches;
    }

    // Mobile: Toggle 
    profileContainer.addEventListener("click", function (event) {
        if (isMobileDevice()) {
            event.stopPropagation(); 
            if (!isDropdownOpen) {
                showDropdown();
            } else {
                hideDropdown();
            }
        }
    });

    // Desktop: Hover
    profileContainer.addEventListener("mouseenter", function () {
        if (!isMobileDevice()) {
            showDropdown();
        }
    });

    profileContainer.addEventListener("mouseleave", function () {
        if (!isMobileDevice()) {
            startHideTimeout();
        }
    });

    profileDropdown.addEventListener("mouseenter", function () {
        clearTimeout(hideTimeout);
    });

    profileDropdown.addEventListener("mouseleave", function () {
        if (!isMobileDevice()) {
            startHideTimeout();
        }
    });

    document.addEventListener("click", function () {
        if (isMobileDevice()) {
            hideDropdown();
        }
    });

    function showDropdown() {
        clearTimeout(hideTimeout);
        profileDropdown.style.display = "flex";
        setTimeout(() => {
            profileDropdown.style.opacity = "1";
        }, 2);
        isDropdownOpen = true;
    }

    function hideDropdown() {
        hideTimeout = setTimeout(function () {
            profileDropdown.style.opacity = "0";
            setTimeout(() => {
                profileDropdown.style.display = "none";
            }, 200);
        }, isMobileDevice() ? 0 : 200); 
        isDropdownOpen = false;
    }

    function startHideTimeout() {
        hideTimeout = setTimeout(hideDropdown, 200);
    }
});







