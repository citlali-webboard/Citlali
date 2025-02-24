var isMobile = window.matchMedia("(max-width: 768px)").matches; // Check if the screen is less than 768px

window.matchMedia("(max-width: 768px)").addEventListener("change", function (e) {
    if (e.matches) { // e.matches is true if the media query is true (less than 768px)
    
        isMobile = true;
        setting_mobile();
    } else {

        isMobile = false;
        setting_desktop();
    }
});

function setting_desktop() {
    var notificationCards = document.querySelectorAll(".notification-card");

    notificationCards.forEach(function (card) {
        // Clear any previous event listener
        card.removeEventListener("click", handleDesktopClick);
        card.removeEventListener("click", handleMobileClick);

        //check have name "clicked" or not
        if (card.getAttribute("name") == "clicked") {
            card.nextSibling.classList.add("hidden");
        }

        //set default notification
        document.querySelector("#notification-detail").classList.add("hidden");
        document.querySelector("#default-notification").classList.remove("hidden");

        // Add click event listener to each card
        card.addEventListener("click", handleDesktopClick);
    });
}

function handleDeleteClick() {
    deleteNotification();
}

function handleDesktopClick() {
    let Card = this;
    let CardId = Card.getAttribute("data-id");

    // Create new XMLHttpRequest object
    let xmlhttp = new XMLHttpRequest();
    let URL = window.location.href + "/detail/" + CardId;

    xmlhttp.open("GET", URL, true);

    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {

            var data = JSON.parse(xmlhttp.responseText);

            // Set details
            document.querySelector("#content-title").innerHTML = data.title;
            // document.querySelector("#content-body").innerHTML = data.message;
            document.querySelector("#source-name").innerHTML = `<span style="font-weight: bold;">${data.sourceDisplayName}</span> (${data.sourceUsername})`;
            document.querySelector("#create-at").href = data.createdAt;

            let source_img = document.querySelector("#source-img");

            source_img.src = data.sourceProfileImageUrl;

            // Decrement notification count if the card is unread
            if (Card.classList.contains("read") == false) {
                decrementUnreadNotification();
            }

            // Mark the card as read and disable hover
            Card.classList.add("read");

            let url = window.location.href;
            let root_url = window.location.origin;
            let preview_url = root_url + data.url;

            if (data.url) {

                function createPreviewCard(data, preview_url) {
  
                    let contentContainer = document.querySelector("#notification-detail .content div");

                    let titleElement = document.createElement("p");
                    titleElement.classList.add("title-over-card");
                    titleElement.textContent = escapeHTML(data.title);

                    let messageElement = document.createElement("p");
                    messageElement.textContent = escapeHTML(data.message);

                    let linkElement = document.createElement("a");
                    linkElement.href = escapeHTML(preview_url);

                    let previewCardElement = document.createElement("div");
                    previewCardElement.classList.add("preview-card");

                    let appNameElement = document.createElement("p");
                    appNameElement.classList.add("app-name");
                    appNameElement.textContent = "Citlali 🩷";

                    let previewCardTopicElement = document.createElement("div");
                    previewCardTopicElement.classList.add("preview-card-topic");

                    let urlTitleElement = document.createElement("h1");
                    urlTitleElement.textContent = escapeHTML(data.urlTitle);

                    let urlDescriptionElement = document.createElement("p");
                    urlDescriptionElement.textContent = escapeHTML(data.urlDescription);

                    let previewLinkElement = document.createElement("a");
                    previewLinkElement.href = escapeHTML(preview_url);
                    previewLinkElement.target = "";

                    let imageContainerElement = document.createElement("div");
                    imageContainerElement.style.borderRadius = "8px";
                    imageContainerElement.style.overflow = "hidden";
                    imageContainerElement.style.marginTop = "10px";

                    let imageElement = document.createElement("img");
                    imageElement.src = escapeHTML(data.urlImage);  // ใส่ URL รูปภาพ
                    imageElement.style.maxWidth = "100%";
                    imageElement.style.height = "auto";

                    if (data.urlImage) {
                        imageContainerElement.appendChild(imageElement);  
                    }

                    previewCardElement.appendChild(appNameElement);
                    previewCardElement.appendChild(previewCardTopicElement);
                    previewCardTopicElement.appendChild(urlTitleElement);
                    previewCardTopicElement.appendChild(urlDescriptionElement);
                    previewLinkElement.appendChild(imageContainerElement);
                    previewCardElement.appendChild(previewLinkElement);
                    linkElement.appendChild(previewCardElement);

                    contentContainer.innerHTML = "";
                    contentContainer.appendChild(titleElement);
                    contentContainer.appendChild(messageElement);
                    contentContainer.appendChild(linkElement);

                    var delete_btn = document.querySelector("#delete-btn");
                    delete_btn.setAttribute("data-id", data.id);
                
                    delete_btn.removeEventListener("click", handleDeleteClick);
                    delete_btn.addEventListener("click", handleDeleteClick);
                }

                createPreviewCard(data, preview_url);

            } else {
                function createNoPreviewCard(data) {
                    let contentContainer = document.querySelector("#notification-detail .content div");
                
                    let titleElement = document.createElement("h2");
                    titleElement.textContent = escapeHTML(data.title);
                
                    let messageElement = document.createElement("p");
                    messageElement.textContent = escapeHTML(data.message);
                
                    let imageElement = null;
                    if (data.imageUrl) {
                        imageElement = document.createElement("img");
                        imageElement.src = escapeHTML(data.imageUrl);  
                        imageElement.style.width = "100%";
                        imageElement.style.height = "auto";
                    }

                    contentContainer.innerHTML = "";  
                    contentContainer.appendChild(titleElement);
                    contentContainer.appendChild(messageElement);
                    
                    if (imageElement) {
                        contentContainer.appendChild(imageElement);
                    }

                    var delete_btn = document.querySelector("#delete-btn");
                    delete_btn.setAttribute("data-id", data.id);
                
                    delete_btn.removeEventListener("click", handleDeleteClick);
                    delete_btn.addEventListener("click", handleDeleteClick);

                }
                
                createNoPreviewCard(data);
            }

            // Show details and hide default notification
            document.querySelector("#notification-detail").classList.remove("hidden");
            document.querySelector("#default-notification").classList.add("hidden");
        }
    };
    xmlhttp.send();

}

function setting_mobile() {

    var notificationCards = document.querySelectorAll(".notification-card");

    notificationCards.forEach(function (card) {
        // Remove any previous event listener
        card.removeEventListener("click", handleDesktopClick);
        card.removeEventListener("click", handleMobileClick);

        card.addEventListener("click", handleMobileClick);
    });
}

function handleMobileClick() {
    let Card = this;
    let CardId = Card.getAttribute("data-id");


    if (Card.getAttribute("name") == "clicked") {
        Card.nextElementSibling.classList.toggle("hidden");
        return;
    }

    Card.classList.add("read");

    // Create new XMLHttpRequest object
    let xmlhttp = new XMLHttpRequest();
    let URL = window.location.href + "/detail/" + CardId;
    xmlhttp.open("GET", URL, true);

    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
            var data = JSON.parse(xmlhttp.responseText);

            let url = window.location.href;
            let root_url = window.location.origin;
            let preview_url = root_url + data.url;
            
            Card.setAttribute("name", "clicked");
            decrementUnreadNotification();

            let container_detail_mobile = document.createElement("div");
            container_detail_mobile.classList.add("container-detail-mobile");
            container_detail_mobile.setAttribute("container-id", data.id);

            let boxtitleElement = document.createElement("div");
            boxtitleElement.classList.add("box-title-over-card");

            let titleElement = document.createElement("p");
            titleElement.classList.add("title-over-card");
            titleElement.textContent = escapeHTML(data.title);

            let messageElement = document.createElement("p");
            messageElement.classList.add("txt-over-card");
            messageElement.textContent = escapeHTML(data.message);

            let linkElement = document.createElement("a");
            linkElement.classList.add("link-over-card");
            linkElement.href = escapeHTML(preview_url);

            let previewCardElement = document.createElement("div");
            previewCardElement.classList.add("preview-card-mobile");

            let appNameElement = document.createElement("p");
            appNameElement.classList.add("app-name");
            appNameElement.textContent = "Citlali 🩷";

            let previewCardTopicElement = document.createElement("div");
            previewCardTopicElement.classList.add("preview-card-topic-mobile");

            let urlTitleElement = document.createElement("h1");
            urlTitleElement.textContent = escapeHTML(data.urlTitle);

            let urlDescriptionElement = document.createElement("p");
            urlDescriptionElement.textContent = escapeHTML(data.urlDescription);

            let imageContainerElement = document.createElement("div");
            imageContainerElement.style.borderRadius = "8px";
            imageContainerElement.style.overflow = "hidden";
            imageContainerElement.style.marginTop = "10px";

            let imageElement = document.createElement("img");
            imageElement.src = escapeHTML(data.urlImage);  
            imageElement.style.maxWidth = "100%";
            imageElement.style.height = "auto";

            previewCardTopicElement.appendChild(urlTitleElement);
            previewCardTopicElement.appendChild(urlDescriptionElement);
            previewCardElement.appendChild(appNameElement);
            previewCardElement.appendChild(previewCardTopicElement);

            if (data.urlImage) {
                imageContainerElement.appendChild(imageElement);
                previewCardElement.appendChild(imageContainerElement);
            }

            let deleteButton = document.createElement("button");
            deleteButton.classList.add("delete-button-mobile");
            deleteButton.textContent = "Delete";
            deleteButton.setAttribute("id", data.id);
            deleteButton.addEventListener("click", function () {
                deleteNotificationMobile(data.id);
            });

            linkElement.appendChild(previewCardElement);

            boxtitleElement.appendChild(titleElement);
            boxtitleElement.appendChild(messageElement);

            container_detail_mobile.appendChild(boxtitleElement);
            // container_detail_mobile.appendChild(titleElement);
            // container_detail_mobile.appendChild(messageElement);
            container_detail_mobile.appendChild(linkElement);

            container_detail_mobile.appendChild(deleteButton);
            // Add the new content after the card
            Card.after(container_detail_mobile);

            var delete_btn = document.querySelector("#delete-btn");
            delete_btn.setAttribute("data-id", data.id);
        
            delete_btn.removeEventListener("click", handleDeleteClick);
            delete_btn.addEventListener("click", handleDeleteClick);
        }
    };

    xmlhttp.send();
}


//set delete-all-btn when load page
document.addEventListener("DOMContentLoaded", function () {
    let deleteAllBtn = document.querySelector("#delete-all-btn");
    deleteAllBtn.addEventListener("click", deleteAllNotification);
});


if (isMobile) {
    setting_mobile();
} else {
    setting_desktop();
}

// ✅ ป้องกัน XSS โดย Escape HTML
function escapeHTML(str) {
    return str.replace(/[&<>"']/g, function (match) {
        return {
            "&": "&amp;",
            "<": "&lt;",
            ">": "&gt;",
            '"': "&quot;",
            "'": "&#039;"
        }[match];
    });
}

function encodeURL(url) {
    return url.replace(/[\s<>"'&]/g, function (match) {
        return {
            " ": "%20",
            "<": "%3C",
            ">": "%3E",
            '"': "%22",
            "'": "%27",
            "&": "%26"
        }[match];
    });
}


function deleteNotification(){
    let id = document.querySelector("#delete-btn").getAttribute("data-id");
    let URL = window.location.href + "/delete/" + id;
    let xmlhttp = new XMLHttpRequest();
    xmlhttp.open("GET", URL, true);
    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
            //set card hidden select card from data-id 
            let card = document.querySelector(`[data-id="${id}"]`);
            card.classList.add("hidden");

            //set to default notification
            document.querySelector("#notification-detail").classList.add("hidden");
            document.querySelector("#default-notification").classList.remove("hidden");
        }
    };
    xmlhttp.send();
}

function deleteAllNotification(){
    let URL = window.location.href + "/deleteAll";
    let xmlhttp = new XMLHttpRequest();
    xmlhttp.open("POST", URL, true);
    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
            location.reload();
        }
    };
    xmlhttp.send();
    console.log("delete all");
}

function deleteNotificationMobile(id){
    let URL = window.location.href + "/delete/" + id;
    let xmlhttp = new XMLHttpRequest();
    xmlhttp.open("GET", URL, true);
    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
            //set card hidden select card from data-id 
            let card = document.querySelector(`[data-id="${id}"]`);
            let containerDetail = document.querySelector(`[container-id="${id}"]`);
            
            card.classList.add("hidden");
            containerDetail.classList.add("hidden");


        }
    };
    xmlhttp.send();
}