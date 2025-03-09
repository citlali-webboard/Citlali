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


function setting_mobile() {

    var notificationCards = document.querySelectorAll(".notification-card");

    notificationCards.forEach(function (card) {
        // Remove any previous event listener
        card.removeEventListener("click", handleDesktopClick);
        card.removeEventListener("click", handleMobileClick);

        card.addEventListener("click", handleMobileClick);
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
    let xhr = new XMLHttpRequest();
    let URL = window.location.href + "/detail/" + CardId;

    xhr.open("GET", URL, true);

    xhr.onreadystatechange = function () {
        if (xhr.readyState == 4 && xhr.status == 200) {

            var data = JSON.parse(xhr.responseText);

            // Set details
            // Safe DOM manipulation without innerHTML
        const contentTitleElement = document.querySelector("#content-title");
        contentTitleElement.textContent = ""; // Clear existing content
        contentTitleElement.textContent = data.title;

        const sourceNameElement = document.querySelector("#source-name");
        sourceNameElement.textContent = ""; // Clear existing content

        // Create the bold element for display name
        const displayNameElement = document.createElement("span");
        displayNameElement.textContent = data.sourceDisplayName;

        // Add the display name element to the source name element
        sourceNameElement.appendChild(displayNameElement);

        // Add the username text
        const usernameText = document.createTextNode(` (${data.sourceUsername})`);
        sourceNameElement.appendChild(usernameText);

        // Set the href attribute safely
        document.querySelector("#create-at").href = encodeURI(data.createdAt);

            let source_img = document.querySelector("#source-img");

            source_img.src = data.sourceProfileImageUrl;

            // Decrement notification count if the card is unread
            if (Card.classList.contains("read") == false) {
                decrementUnreadNotification();
            }

            // Mark the card as read and disable hover
            Card.classList.add("read");

            // let url = window.location.href;
            let root_url = window.location.origin;
            let preview_url = root_url + data.url;

            if (data.url) {

                function createPreviewCard(data, preview_url) {
  
                    let contentContainer = document.querySelector("#notification-detail .content div");

                    let titleElement = document.createElement("p");
                    titleElement.classList.add("title-over-card");
                    titleElement.textContent = (data.title);

                    let messageElement = document.createElement("p");
                    messageElement.textContent = (data.message);

                    let linkElement = document.createElement("a");
                    linkElement.href = (preview_url);

                    let previewCardElement = document.createElement("div");
                    previewCardElement.classList.add("preview-card");

                    let appNameElement = document.createElement("p");
                    appNameElement.classList.add("app-name");
                    appNameElement.textContent = "Citlali 🩷"; // eslint-disable-line

                    let previewCardTopicElement = document.createElement("div");
                    previewCardTopicElement.classList.add("preview-card-topic");

                    let urlTitleElement = document.createElement("h1");
                    urlTitleElement.textContent = (data.urlTitle);

                    let urlDescriptionElement = document.createElement("p");
                    urlDescriptionElement.textContent = (data.urlDescription);

                    let previewLinkElement = document.createElement("a");
                    previewLinkElement.href = (preview_url);
                    previewLinkElement.target = "";

                    let imageContainerElement = document.createElement("div");
                    imageContainerElement.style.borderRadius = "8px";
                    imageContainerElement.style.overflow = "hidden";
                    imageContainerElement.style.marginTop = "10px";

                    let imageElement = document.createElement("img");
                    imageElement.src = (data.urlImage);  
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
    xhr.send();

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

function handleMobileClick() {
    let card = this;
    let cardId = card.getAttribute("data-id");
    
    // Handle toggle case when already clicked
    if (card.getAttribute("name") === "clicked") {
        card.nextElementSibling.classList.toggle("hidden");
        card.style.borderRadius = card.nextElementSibling.classList.contains("hidden") ? "8px" : "8px 8px 0 0";
        return;
    }
    
    // Check if already processing - prevent multiple clicks
    if (card.hasAttribute("data-processing")) {
        return;
    }
    
    // Set processing flag
    card.setAttribute("data-processing", "true");
    
    // Mark as read immediately to provide user feedback
    card.classList.add("read");
    decrementUnreadNotification();
    
    // Show loading indicator
    const loadingIndicator = document.createElement("div");
    loadingIndicator.className = "loading-indicator";
    loadingIndicator.textContent = "Loading...";
    card.appendChild(loadingIndicator);
    
    // Fetch notification details
    fetchNotificationDetails(cardId)
        .then(data => {
            // Remove loading indicator
            card.removeChild(loadingIndicator);
            
            // Check if a container already exists for this card
            const existingContainer = card.nextElementSibling;
            if (existingContainer && existingContainer.getAttribute("container-id") === data.id) {
                existingContainer.classList.remove("hidden");
                card.setAttribute("name", "clicked");
                card.style.borderRadius = "8px 8px 0 0";
                return;
            }
            
            // Create container for details
            const detailContainer = createDetailContainer(data);
            
            // Add to DOM
            card.after(detailContainer);
            card.setAttribute("name", "clicked");
            card.style.borderRadius = "8px 8px 0 0";
        })
        .catch(error => {
            console.error("Error fetching notification details:", error);
            card.removeChild(loadingIndicator);
            
            // Show error message
            const errorMsg = document.createElement("div");
            errorMsg.className = "error-message";
            errorMsg.textContent = "Failed to load notification details";
            errorMsg.style.color = "red";
            errorMsg.style.padding = "10px";
            card.appendChild(errorMsg);
            
            // Remove error message after 3 seconds
            setTimeout(() => {
                card.removeChild(errorMsg);
            }, 3000);
        })
        .finally(() => {
            // Remove processing flag
            card.removeAttribute("data-processing");
        });
}

// Helper function to fetch notification details
function fetchNotificationDetails(cardId) {
    return new Promise((resolve, reject) => {
        let xmlhttp = new XMLHttpRequest();
        let url = window.location.href + "/detail/" + cardId;
        
        xmlhttp.open("GET", url, true);
        xmlhttp.onreadystatechange = function() {
            if (xmlhttp.readyState === 4) {
                if (xmlhttp.status === 200) {
                    try {
                        const data = JSON.parse(xmlhttp.responseText);
                        resolve(data);
                    } catch (e) {
                        reject(new Error("Invalid JSON response"));
                    }
                } else {
                    reject(new Error(`Server returned status ${xmlhttp.status}`));
                }
            }
        };
        xmlhttp.onerror = () => reject(new Error("Network error"));
        xmlhttp.send();
    });
}

// Helper function to create detail container
function createDetailContainer(data) {
    const rootUrl = window.location.origin;
    const previewUrl = rootUrl + data.url;
    
    // Create container
    const container = document.createElement("div");
    container.classList.add("container-detail-mobile");
    container.setAttribute("container-id", data.id);
    
    // Create title box
    const boxTitle = document.createElement("div");
    boxTitle.classList.add("box-title-over-card");
    
    // Create text content
    const textBox = document.createElement("div");
    textBox.classList.add("txt-in-box");
    
    const title = document.createElement("p");
    title.classList.add("title-over-card");
    title.textContent = escapeHTML(data.title);
    
    const message = document.createElement("p");
    message.classList.add("txt-over-card");
    message.textContent = escapeHTML(data.message);
    
    textBox.appendChild(title);
    textBox.appendChild(message);
    
    // Create delete button
    const deleteButton = document.createElement("button");
    deleteButton.classList.add("delete-button-mobile");
    deleteButton.setAttribute("id", data.id);
    
    // Use createElement for SVG instead of innerHTML
    const svgElem = createSafeSvgElement();
    deleteButton.appendChild(svgElem);
    
    // Add event listener with debounce to prevent multiple clicks
    let isProcessing = false;
    deleteButton.addEventListener("click", function(e) {
        e.stopPropagation(); // Prevent event bubbling
        if (isProcessing) return;
        isProcessing = true;
        
        deleteNotificationMobile(data.id);
        setTimeout(() => { isProcessing = false; }, 500);
    });
    
    boxTitle.appendChild(textBox);
    boxTitle.appendChild(deleteButton);
    
    // Create preview link if URL exists
    if (data.url) {
        const link = document.createElement("a");
        link.classList.add("link-over-card");
        link.href = escapeHTML(previewUrl);
        
        const previewCard = createPreviewCard(data);
        link.appendChild(previewCard);
        
        container.appendChild(boxTitle);
        container.appendChild(link);
    } else {
        container.appendChild(boxTitle);
    }
    
    return container;
}

// Helper function to create preview card
function createPreviewCard(data) {
    const previewCard = document.createElement("div");
    previewCard.classList.add("preview-card-mobile");
    
    const appName = document.createElement("p");
    appName.classList.add("app-name");
    appName.textContent = "Citlali 🩷";
    
    const topicContainer = document.createElement("div");
    topicContainer.classList.add("preview-card-topic-mobile");
    
    const urlTitle = document.createElement("h1");
    urlTitle.textContent = escapeHTML(data.urlTitle);
    
    const urlDescription = document.createElement("p");
    urlDescription.textContent = escapeHTML(data.urlDescription);
    
    topicContainer.appendChild(urlTitle);
    topicContainer.appendChild(urlDescription);
    previewCard.appendChild(appName);
    previewCard.appendChild(topicContainer);
    
    // Add image if available
    if (data.urlImage) {
        const imageContainer = document.createElement("div");
        imageContainer.style.borderRadius = "8px";
        imageContainer.style.overflow = "hidden";
        imageContainer.style.marginTop = "10px";
        
        const image = document.createElement("img");
        image.src = escapeHTML(data.urlImage);
        image.style.maxWidth = "100%";
        image.style.height = "auto";
        
        imageContainer.appendChild(image);
        previewCard.appendChild(imageContainer);
    }
    
    return previewCard;
}

// Create SVG element safely without innerHTML
function createSafeSvgElement() {
    // Create SVG element
    const svg = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    svg.setAttribute("width", "20px");
    svg.setAttribute("height", "20px");
    svg.setAttribute("viewBox", "0 0 24 24");
    svg.setAttribute("version", "1.1");
    svg.setAttribute("fill", "#666");
    
    // Create groups and paths
    const g1 = document.createElementNS("http://www.w3.org/2000/svg", "g");
    g1.setAttribute("id", "SVGRepo_bgCarrier");
    g1.setAttribute("stroke-width", "0");
    
    const g2 = document.createElementNS("http://www.w3.org/2000/svg", "g");
    g2.setAttribute("id", "SVGRepo_tracerCarrier");
    g2.setAttribute("stroke-linecap", "round");
    g2.setAttribute("stroke-linejoin", "round");
    g2.setAttribute("stroke", "#CCCCCC");
    g2.setAttribute("stroke-width", "0.72");
    
    const g3 = document.createElementNS("http://www.w3.org/2000/svg", "g");
    g3.setAttribute("id", "SVGRepo_iconCarrier");
    
    const title = document.createElementNS("http://www.w3.org/2000/svg", "title");
    title.textContent = "delete_2_line";
    
    const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
    path.setAttribute("d", "M14.2792,2 C15.1401,2 15.9044,2.55086 16.1766,3.36754 L16.7208,5 L20,5 C20.5523,5 21,5.44772 21,6 C21,6.55227 20.5523,6.99998 20,7 L19.9975,7.07125 L19.1301,19.2137 C19.018,20.7837 17.7117,22 16.1378,22 L7.86224,22 C6.28832,22 4.982,20.7837 4.86986,19.2137 L4.00254,7.07125 C4.00083,7.04735 3.99998,7.02359 3.99996,7 C3.44769,6.99998 3,6.55227 3,6 C3,5.44772 3.44772,5 4,5 L7.27924,5 L7.82339,3.36754 C8.09562,2.55086 8.8599,2 9.72076,2 L14.2792,2 Z M17.9975,7 L6.00255,7 L6.86478,19.0712 C6.90216,19.5946 7.3376,20 7.86224,20 L16.1378,20 C16.6624,20 17.0978,19.5946 17.1352,19.0712 L17.9975,7 Z M10,10 C10.51285,10 10.9355092,10.386027 10.9932725,10.8833761 L11,11 L11,16 C11,16.5523 10.5523,17 10,17 C9.48715929,17 9.06449214,16.613973 9.00672766,16.1166239 L9,16 L9,11 C9,10.4477 9.44771,10 10,10 Z M14,10 C14.5523,10 15,10.4477 15,11 L15,16 C15,16.5523 14.5523,17 14,17 C13.4477,17 13,16.5523 13,16 L13,11 C13,10.4477 13.4477,10 14,10 Z M14.2792,4 L9.72076,4 L9.38743,5 L14.6126,5 L14.2792,4 Z");
    path.setAttribute("fill", "#666");
    
    // Assemble the SVG hierarchy
    g3.appendChild(title);
    g3.appendChild(path);
    
    svg.appendChild(g1);
    svg.appendChild(g2);
    svg.appendChild(g3);
    
    return svg;
}

// Make sure escapeHTML function is properly implemented
function escapeHTML(str) {
    return String(str)
    // if (!str) return "";
    // return String(str)
    //     .replace(/&/g, '&amp;')
    //     .replace(/</g, '&lt;')
    //     .replace(/>/g, '&gt;')
    //     .replace(/"/g, '&quot;')
    //     .replace(/'/g, '&#039;');
}




// function encodeURL(url) {
//     return url.replace(/[\s<>"'&]/g, function (match) {
//         return {
//             " ": "%20",
//             "<": "%3C",
//             ">": "%3E",
//             '"': "%22",
//             "'": "%27",
//             "&": "%26"
//         }[match];
//     });
// }


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