var isMobile = window.matchMedia("(max-width: 768px)").matches; // Check if the screen is less than 768px

window.matchMedia("(max-width: 768px)").addEventListener("change", function (e) {
    if (e.matches) { // e.matches is true if the media query is true (less than 768px)
        console.log("less than 768px");
        isMobile = true;
        setting_mobile();
    } else {
        console.log("more than 768px");
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

function handleDesktopClick() {
    let Card = this;
    let CardId = Card.getAttribute("data-id");
    console.log(CardId);

    // Create new XMLHttpRequest object
    let xmlhttp = new XMLHttpRequest();
    let URL = window.location.href + "/detail/" + CardId;
    console.log(URL);
    xmlhttp.open("GET", URL, true);

    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
            console.log(xmlhttp.responseText);
            var data = JSON.parse(xmlhttp.responseText);
            console.log(data);

            // Set details
            document.querySelector("#content-title").innerHTML = data.title;
            // document.querySelector("#content-body").innerHTML = data.message;
            document.querySelector("#source-name").innerHTML = `${data.sourceDisplayName}(${data.sourceUsername})`;
            document.querySelector("#create-at").href = data.createdAt;

            let source_img = document.querySelector("#source-img");

            source_img.src = data.sourceProfileImageUrl;

            // Decrement notification count if the card is unread
            if (Card.classList.contains("read") == false) {
                console.log("decrement");
                decrementUnreadNotification();
            }

            // Mark the card as read and disable hover
            Card.classList.add("read");

            let contentContainer = document.querySelector("#notification-detail .content div");

            let url = window.location.href;
            let root_url = url.split("/Notification")[0];
            let preview_url = root_url + data.url;

            if (data.url) {

                let preview_card = `
                    <p>${escapeHTML(data.title)}</p>
                    <p>${escapeHTML(data.message)}</p>
                    <a href="${preview_url}">
                        <div class="preview-card">
                            <p class="app-name">Citlali 🩷</p>
                            <div class="preview-card-topic">
                                <h1>${escapeHTML(data.urlTitle)}</h1>
                                <p>${escapeHTML(data.urlDescription)}</p>
                            </div>
                            
                            <a href="${preview_url}" target="">
                                <div style="border-radius: 8px; overflow: hidden; margin-top: 10px;">
                                    <img src="${data.urlImage}" style="max-width: 100%; height: auto;">
                                </div>
                            </a>
                        </div>
                    </a>
                `;

                contentContainer.innerHTML = preview_card;

            } else {
                // ถ้าไม่มี URL แสดงเป็นข้อความปกติ
                no_preview_card = `
                    <h2>${escapeHTML(data.title)}</h2>
                    <p>${escapeHTML(data.message)}</p>
                    ${data.imageUrl ? `<img src="${data.imageUrl}" width="100%" height="auto" />` : ""}
                `;

                contentContainer.innerHTML = no_preview_card;
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
    console.log(CardId);

    if(Card.getAttribute("name") == "clicked"){
        Card.nextElementSibling.classList.toggle("hidden");
        return;
    }

    // Create new XMLHttpRequest object
    let xmlhttp = new XMLHttpRequest();
    let URL = window.location.href + "/detail/" + CardId;
    xmlhttp.open("GET", URL, true);

    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
            var data = JSON.parse(xmlhttp.responseText);
            console.log(data);

            let url = window.location.href;
            let root_url = url.split("/Notification")[0];
            let preview_url = root_url + data.url;
            console.log(preview_url);
            
            Card.setAttribute("name", "clicked");
            decrementUnreadNotification();

            let container_detail_mobile = document.createElement("div");
            container_detail_mobile.classList.add("container-detail-mobile");

            if (data.url) {

                let preview_card_mobile = `
                    <p class="txt-over-card">${escapeHTML(data.title)}</p>
                    <p class="txt-over-card">${escapeHTML(data.message)}</p>
                    <a href="${preview_url}">
                        <div class="preview-card-mobile">
                            <p class="app-name">Citlali 🩷</p>
                            <div class="preview-card-topic-mobile">
                                <h1>${escapeHTML(data.urlTitle)}</h1>
                                <p>${escapeHTML(data.urlDescription)}</p>
                            </div>
                            
                            <a href="${preview_url}" target="">
                                <div style="border-radius: 8px; overflow: hidden; margin-top: 10px;">
                                    <img src="${data.urlImage}" style="max-width: 100%; height: auto;">
                                </div>
                            </a>
                        </div>
                    </a>
                `;

                container_detail_mobile.innerHTML = preview_card_mobile;

            } else {
                // ถ้าไม่มี URL แสดงเป็นข้อความปกติ
                let no_preview_card_mobile = `
                    <h2>${escapeHTML(data.title)}</h2>
                    <p>${escapeHTML(data.message)}</p>
                    ${data.imageUrl ? `<img src="${data.imageUrl}" width="100%" height="auto" />` : ""}
                `;

                container_detail_mobile.innerHTML = no_preview_card_mobile;
            }

            Card.after(container_detail_mobile);
            
        }
    };
    xmlhttp.send();

}

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
