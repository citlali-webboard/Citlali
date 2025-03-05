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
    if(window.confirm("Are you sure you want to delete this notification?")){
        deleteNotification();
    }

    return;
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
        Card.style.borderRadius = "8px"; // Reset to default
        Card.removeAttribute("name");
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
            Card.style.borderRadius = "8px 8px 0 0";
            decrementUnreadNotification();
 
            let container_detail_mobile = document.createElement("div");
            container_detail_mobile.classList.add("container-detail-mobile");
            container_detail_mobile.setAttribute("container-id", data.id);

            let boxtitleElement = document.createElement("div");
            boxtitleElement.classList.add("box-title-over-card");

            let textBox = document.createElement("div");
            textBox.classList.add("txt-in-box");
            
            let titleElement = document.createElement("p");
            titleElement.classList.add("title-over-card");
            titleElement.textContent = escapeHTML(data.title);
            
            let messageElement = document.createElement("p");
            messageElement.classList.add("txt-over-card");
            messageElement.textContent = escapeHTML(data.message);
            
            textBox.appendChild(titleElement);
            textBox.appendChild(messageElement);

            let deleteButton = document.createElement("button");
            deleteButton.classList.add("delete-button-mobile");
            deleteButton.setAttribute("id", data.id);
            deleteButton.addEventListener("click", function () {
                deleteNotificationMobile(data.id);
            });

            // Add FluentIcon inside the delete button
            deleteButton.innerHTML = `
                <svg width="20px" height="20px" viewBox="0 0 24.00 24.00" version="1.1" 
                    xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" 
                    fill="#666" transform="rotate(0)matrix(1, 0, 0, 1, 0, 0)" stroke="#666">
                    <g id="SVGRepo_bgCarrier" stroke-width="0"></g>
                    <g id="SVGRepo_tracerCarrier" stroke-linecap="round" stroke-linejoin="round" stroke="#CCCCCC" stroke-width="0.72"></g>
                    <g id="SVGRepo_iconCarrier">
                        <title>delete_2_line</title>
                        <g id="页面-1" stroke-width="0.00024000000000000003" fill="none" fill-rule="evenodd">
                            <g id="System" transform="translate(-576.000000, -192.000000)" fill-rule="nonzero">
                                <g id="delete_2_line" transform="translate(576.000000, 192.000000)">
                                    <path d="M24,0 L24,24 L0,24 L0,0 L24,0 Z M12.5934901,23.257841 L12.5819402,23.2595131 L12.5108777,23.2950439 L12.4918791,23.2987469 
                                        L12.4918791,23.2987469 L12.4767152,23.2950439 L12.4056548,23.2595131 C12.3958229,23.2563662 12.3870493,23.2590235 12.3821421,23.2649074 
                                        L12.3780323,23.275831 L12.360941,23.7031097 L12.3658947,23.7234994 L12.3769048,23.7357139 L12.4804777,23.8096931 
                                        L12.4953491,23.8136134 L12.4953491,23.8136134 L12.5071152,23.8096931 L12.6106902,23.7357139 L12.6232938,23.7196733 
                                        L12.6232938,23.7196733 L12.6266527,23.7031097 L12.609561,23.275831 C12.6075724,23.2657013 12.6010112,23.2592993 12.5934901,23.257841 
                                        L12.5934901,23.257841 Z" fill-rule="nonzero"></path>
                                    <path d="M14.2792,2 C15.1401,2 15.9044,2.55086 16.1766,3.36754 L16.7208,5 L20,5 C20.5523,5 21,5.44772 21,6 C21,6.55227 20.5523,6.99998 
                                        20,7 L19.9975,7.07125 L19.9975,7.07125 L19.1301,19.2137 C19.018,20.7837 17.7117,22 16.1378,22 L7.86224,22 C6.28832,22 4.982,20.7837 
                                        4.86986,19.2137 L4.00254,7.07125 C4.00083,7.04735 3.99998,7.02359 3.99996,7 C3.44769,6.99998 3,6.55227 3,6 C3,5.44772 3.44772,5 4,5 
                                        L7.27924,5 L7.82339,3.36754 C8.09562,2.55086 8.8599,2 9.72076,2 L14.2792,2 Z M17.9975,7 L6.00255,7 L6.86478,19.0712 
                                        C6.90216,19.5946 7.3376,20 7.86224,20 L16.1378,20 C16.6624,20 17.0978,19.5946 17.1352,19.0712 L17.9975,7 Z M10,10 
                                        C10.51285,10 10.9355092,10.386027 10.9932725,10.8833761 L11,11 L11,16 C11,16.5523 10.5523,17 10,17 C9.48715929,17 9.06449214,16.613973 
                                        9.00672766,16.1166239 L9,16 L9,11 C9,10.4477 9.44771,10 10,10 Z M14,10 C14.5523,10 15,10.4477 15,11 L15,16 C15,16.5523 14.5523,17 
                                        14,17 C13.4477,17 13,16.5523 13,16 L13,11 C13,10.4477 13.4477,10 14,10 Z M14.2792,4 L9.72076,4 L9.38743,5 L14.6126,5 L14.2792,4 Z" 
                                        fill="#666"></path>
                                </g>
                            </g>
                        </g>
                    </g>
                </svg>
            `;

            boxtitleElement.appendChild(textBox);
            boxtitleElement.appendChild(deleteButton);

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

            previewCardTopicElement.appendChild(urlTitleElement);
            previewCardTopicElement.appendChild(urlDescriptionElement);
            previewCardElement.appendChild(appNameElement);
            previewCardElement.appendChild(previewCardTopicElement);

            let imageContainerElement = document.createElement("div");
            imageContainerElement.style.borderRadius = "8px";
            imageContainerElement.style.overflow = "hidden";
            imageContainerElement.style.marginTop = "10px";

            if (data.urlImage) {
                let imageElement = document.createElement("img");
                imageElement.src = escapeHTML(data.urlImage);
                imageElement.style.maxWidth = "100%";
                imageElement.style.height = "auto";

                imageContainerElement.appendChild(imageElement);
                previewCardElement.appendChild(imageContainerElement);
            }

            linkElement.appendChild(previewCardElement);
            container_detail_mobile.appendChild(boxtitleElement);
            container_detail_mobile.appendChild(linkElement);

            // Add the new content after the card
            Card.after(container_detail_mobile);

        }
    };

    xmlhttp.send();
}


//set delete-all-btn when load page
document.addEventListener("DOMContentLoaded", function () {
    let deleteAllBtn = document.querySelector("#delete-all-btn");
    deleteAllBtn.addEventListener("click", deleteAllNotification);

    let readAllBtn = document.querySelector("#read-all-btn");
    readAllBtn.addEventListener("click", readAllNotification);
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
    if(window.confirm("Are you sure you want to delete all notifications?")){
        let URL = window.location.href + "/deleteAll";
        let xmlhttp = new XMLHttpRequest();
        xmlhttp.open("POST", URL, true);
        xmlhttp.onreadystatechange = function () {
            if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
                location.reload();
            }
        };
        xmlhttp.send();
    }
    return;
}

function deleteNotificationMobile(id){

    if(window.confirm("Are you sure you want to delete this notification?")){
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

    return;

}

function readAllNotification(){
    if(window.confirm("Are you sure you want to read all notifications?")){
        let URL = window.location.href + "/readAll";
        let xmlhttp = new XMLHttpRequest();
        xmlhttp.open("POST", URL, true);
        xmlhttp.onreadystatechange = function () {
            if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
                location.reload();
            }
        };
        xmlhttp.send();
    }
    return;
}