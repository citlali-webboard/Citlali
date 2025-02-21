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
        console.log("11111");
        // set container-detail-mobile to hidden 
        // if (card.nextSibling != null && card.nextSibling.classList.contains("container-detail-mobile")){
        //     card.nextSibling.classList.add("hidden");
        // }

        //check have name "clicked" or not
        if (card.getAttribute("name") == "clicked") {
            card.nextSibling.classList.add("hidden");
        }

        console.log("222222");
        //set default notification 
        document.querySelector("#notification-detail").classList.add("hidden");
        document.querySelector("#default-notification").classList.remove("hidden");

        console.log("3333333");
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

            
            // Mark the card as read and disable hover
            Card.classList.add("read");

            let contentContainer = document.querySelector("#notification-detail .content div");

            if (data.url) {
                // ถ้า notification มี URL ให้แสดงเป็น card คล้าย GitHub Link Embed
                contentContainer.innerHTML = `
                <p>${data.title}</p>
                <p>${data.message}</p>
                <div class="preview-card">
                    <p class="app-name">Citlali 🩷</p>
                    <div class="preview-card-topic">
                        <h1>${data.urlTitle}</h1>
                        <p>${data.urlDescription}</p>
                    </div>
                    
                    <a class="preview-link" href="${data.url}">${data.url}</a>
                    
                    <a href="${data.url}" target="">
                        <div style="border-radius: 8px; overflow: hidden; margin-top: 10px;">
                            <img src="${data.urlImage}" style="max-width: 100%; height: auto;">
                        </div>
                    </a>
                </div>

                `;
            } else {
                // ถ้าไม่มี URL แสดงเป็นข้อความปกติ
                contentContainer.innerHTML = `
                    <h2>${data.title}</h2>
                    <p>${data.message}</p>
                    ${data.imageUrl ? `<img src="${data.imageUrl}" width="100%" height="auto" />` : ""}
                `;
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
    console.log("mobile");

    let Card = this;
    let CardId = Card.getAttribute("data-id");
    console.log(CardId);

    // Create new XMLHttpRequest object
    let xmlhttp = new XMLHttpRequest();
    let URL = window.location.href + "/detail/" + CardId;
    xmlhttp.open("GET", URL, true);

    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
            var data = JSON.parse(xmlhttp.responseText);
            console.log(data);
        }

        let url = window.location.href;
        let root_url = url.split("/Notification")[0];
        let preview_url = root_url + data.url;
        console.log(preview_url);

        //if the card is not contain the name "clicked"
        if (Card.getAttribute("name") != "clicked") {
            Card.setAttribute("name", "clicked");
            // create new div below the card
            let container_detail_mobile = document.createElement("div");
            container_detail_mobile.classList.add("container-detail-mobile");

            let title_detail_mobile = document.createElement("div");
            title_detail_mobile.innerHTML = data.title;
            title_detail_mobile.classList.add("title-detail-mobile");

            let message_detail_mobile = document.createElement("div");
            message_detail_mobile.innerHTML = data.message;
            message_detail_mobile.classList.add("message-detail-mobile");

            let preview_url_detail_mobile = document.createElement("a");
            preview_url_detail_mobile.innerHTML = "Preview";
            preview_url_detail_mobile.href = preview_url;
            preview_url_detail_mobile.classList.add("preview-url-detail-mobile");

            container_detail_mobile.appendChild(title_detail_mobile);
            container_detail_mobile.appendChild(message_detail_mobile);
            container_detail_mobile.appendChild(preview_url_detail_mobile);



            Card.after(container_detail_mobile);

        }
        else{
            // hidden the div below the card
            console.log("hidden");
            Card.nextElementSibling.classList.toggle("hidden");
        }
    };
    xmlhttp.send();
}


if (isMobile) {
    setting_mobile();
} else {
    setting_desktop();
}
