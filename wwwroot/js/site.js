// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const profileContainer = document.querySelector(".profile-container");
    const profileDropdown = document.querySelector(".profile-dropdown");
    let hideTimeout;
    let isDropdownOpen = false;


    // Set number of notifications on navbar
    // if have a cookie named count_notifications
    if (document.cookie.indexOf("count_notifications") >= 0)
    {
        // get value of cookie name count_notifications
        let count = document.cookie.replace(/(?:(?:^|.*;\s*)count_notifications\s*\=\s*([^;]*).*$)|^.*$/, "$1");
        let notificationIcon = document.getElementById("notification-icon");
        let div_count_notifications = document.createElement("div");
        div_count_notifications.classList.add("count-notifications");
        div_count_notifications.innerHTML = count;
        
        notificationIcon.appendChild(div_count_notifications);
    }

    let notificationIcon = document.getElementById("notification-icon");
    

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

var socket = new WebSocket("ws://localhost:5112/notification/realtime");

socket.onopen = function () {
    console.log("Connected to the server");
}

socket.onmessage = function (e){
    console.log("Message received: " + e.data);
}

socket.onerror = function (error){
    console.log(error);
}

socket.onclose = function (){
    console.log("Connection closed");
}






