function getCookieValue(cookieName) {
    var _a;
    return (_a = document.cookie
        .split("; ")
        .find(function (row) { return row.startsWith("".concat(cookieName, "=")); })) === null || _a === void 0 ? void 0 : _a.split("=")[1];
}
var socketPath = "/notification/realtime";
var socket = new WebSocket(socketPath);
var accessCookieName = "AccessToken";
var refreshCookieName = "RefreshToken";
var accessCookieValue = getCookieValue(accessCookieName);
var refreshCookieValue = getCookieValue(refreshCookieName);
socket.onopen = function (event) {
    console.log(event);
    socket.send("".concat(accessCookieValue, ";").concat(refreshCookieValue));
};
socket.onmessage = function (event) {
    console.log(event);
    var data = JSON.parse(event.data);
    console.log(data);
};
socket.onclose = function (event) {
    console.log(event);
};
