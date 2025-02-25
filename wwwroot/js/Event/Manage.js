function shareEvent(title, eventId) {
    if (navigator.share) {
        navigator.share({
            title: title,
            text: 'Check out this event!',
            url: window.location.origin + '/event/detail/' + eventId
        }).then(() => {
            console.log('Thanks for sharing!');
        }).catch((error) => {
            console.error('Error sharing:', error);
        });
    } else {
        const url = window.location.origin + '/event/detail/' + eventId;
        navigator.clipboard.writeText(url).then(() => {
            let button = document.getElementById('share-btn-txt');
            button.innerHTML = 'URL copied!';
            setTimeout(() => {
                button.innerHTML = 'Share';
            }, 5000);
        }).catch((error) => {
            console.error('Error copying URL:', error);
        });
    }
}

function toggleDropdown(id) {
    var element = document.getElementById(id);
    element.style.display = (element.style.display === "block") ? "none" : "block";
}

function openBroadcastPopup() {
    var popup = document.querySelector('.broadcast-popup');
    popup.style.display = 'block';
}

function closeDialog() {
    var popup = document.querySelector('.broadcast-popup');
    popup.style.display = 'none';
}