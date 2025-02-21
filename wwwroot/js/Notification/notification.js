document.addEventListener("DOMContentLoaded", function () {
    // Select all notification cards
    var notificationCards = document.querySelectorAll(".notification-card");

    console.log(notificationCards);
    
    // Add click event listener to each card
    notificationCards.forEach(function (card) {
        card.addEventListener("click", function () {
            let  CardId = card.getAttribute("data-id");
            console.log(CardId);

            // Create new XMLHttpRequest object
            let xmlhttp = new XMLHttpRequest();

            //self + notification/detail/ + CardId
            let URL = window.location.href + "/detail/" + CardId;
            console.log(URL);
            xmlhttp.open("GET", URL , true);

            xmlhttp.onreadystatechange = function () {
                if (xmlhttp.readyState == 4 && xmlhttp.status == 200) { // readyState 4 means the operation is complete and status 200 means the request is successful
                    console.log(xmlhttp.responseText);
                    let data = JSON.parse(xmlhttp.responseText);
                    console.log(data);

                    // set details 
                    document.querySelector("#content-title").innerHTML = data.title;
                    document.querySelector("#content-body").innerHTML = data.message;
                    document.querySelector("#source-name").innerHTML = `${data.sourceDisplayName}(${data.sourceUsername})`
                    document.querySelector("#create-at").href = data.createdAt;
                    let source_img = document.querySelectorAll("#source-img");
                    console.log(data.sourceProfileImageUrl);
                    source_img[0].setAttribute('src', data.sourceProfileImageUrl);
                    console.log(source_img[0]); 

                    //set the card as read and cannot hover
                    card.classList.add("read");

                    //remove hidden from detail 
                    document.querySelector("#notification-detail").classList.remove("hidden");
                    
                    //set default-notification hidden
                    document.querySelector("#default-notification").classList.add("hidden");


                    //set iframe src
                    // document.querySelector("#iframe").src = ; 
                }
            }
            xmlhttp.send();

        });
    });
});