var numberOfNotification = 0;

function GetNumberNotification() {
    let xmlhttp = new XMLHttpRequest();
    let URL = "/Notification/count";

    console.log("URL : " + URL);

    xmlhttp.open("GET", URL, true);
    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
            var data = JSON.parse(xmlhttp.responseText);
            console.log(data);

            numberOfNotification = data.unreadNotifications;
            SetNumberNotification(numberOfNotification);
        }
    };
    xmlhttp.send();

    
}

function SetNumberNotification(number) {
    numberOfNotification = number;

    if(number != 0){
        let notificationIcon = document.getElementById("notification-icon");
        let div_count_notifications = document.createElement("div");
        div_count_notifications.classList.add("count-notifications");
        div_count_notifications.innerHTML = number;
    
        notificationIcon.appendChild(div_count_notifications);
    }
    else{
        return;
    }

}

function updateRealTimeNotification() {
    numberOfNotification++;
    SetNumberNotification(numberOfNotification);
}
 

GetNumberNotification();
