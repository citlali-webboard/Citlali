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
            let URL = window.location.href + "detail/" + CardId;
            console.log(URL);
            xmlhttp.open("GET", URL , true);

            xmlhttp.onreadystatechange = function () {
                if (xmlhttp.readyState == 4 && xmlhttp.status == 200) { // readyState 4 means the operation is complete and status 200 means the request is successful
                    let data = JSON.parse(xmlhttp.responseText);
                    console.log(data);
                }
            }
            xmlhttp.send();

        });
    });
});