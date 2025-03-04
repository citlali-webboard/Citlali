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
  if(number > 99){ number = "99+"; }
  let notificationIcon = document.getElementById("notification-icon");
  let mobileNavToggle = document.getElementById("mobile-nav-toggle");

  // Remove existing notification badges
  let countNotificationsBadge = document.querySelectorAll(".count-notifications");
  countNotificationsBadge.forEach(badge => badge.remove());

  if (number != 0) {
    // Add badge to notification icon
    let div_count_notifications = document.createElement("div");
    div_count_notifications.classList.add("count-notifications");
    div_count_notifications.innerHTML = number;
    notificationIcon.appendChild(div_count_notifications);
    
    // Add badge to mobile hamburger menu
    let mobile_badge = document.createElement("div");
    mobile_badge.classList.add("count-notifications", "mobile-badge");
    mobile_badge.innerHTML = number;
    mobileNavToggle.appendChild(mobile_badge);
  }
}

function incrementUnreadNotification() {
  numberOfNotification += 1;
  SetNumberNotification(numberOfNotification);
}

function decrementUnreadNotification() {
  if(numberOfNotification > 0){
    numberOfNotification -= 1;
  }

  SetNumberNotification(numberOfNotification);
}

GetNumberNotification();
