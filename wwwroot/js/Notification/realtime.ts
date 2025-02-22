interface Notification {
  NotificationId: string;
  SourceUserId: string;
  SourceUsername: string;
  SourceDisplayName: string;
  SourceProfileImageUrl: string;
  Read: boolean;
  Title: string;
  CreatedAt: string;
}

function getCookieValue(cookieName: string) {
  return document.cookie
    .split("; ")
    .find((row) => row.startsWith(`${cookieName}=`))
    ?.split("=")[1];
}

const socketPath = "/notification/realtime";
const socket = new WebSocket(socketPath);

const accessCookieName = "AccessToken";
const refreshCookieName = "RefreshToken";
const accessCookieValue = getCookieValue(accessCookieName);
const refreshCookieValue = getCookieValue(refreshCookieName);

socket.onopen = (event) => {
  if (!accessCookieValue || !refreshCookieValue) {
    socket.close();
    return;
  }
  socket.send(`${accessCookieValue};${refreshCookieValue}`);
};

socket.onmessage = (event) => {
  const data = JSON.parse(event.data) as Notification;
  updateRealTimeNotification();
  addNotificationToast(data.Title, data.SourceDisplayName, data.SourceProfileImageUrl);
};

socket.onclose = (event) => {
};
