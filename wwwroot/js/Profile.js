function openDialog() {
    document.getElementById('edit-popup').style.display = 'flex';
}

function closeDialog() {
    document.getElementById('edit-popup').style.display = 'none';
    window.location.reload();       // reload the page to reset the edit form
}

document.addEventListener("DOMContentLoaded", function () {
    let fileInput = document.getElementById("profileImage");
    let imgPreview = document.getElementById("profilePreview");


    imgPreview.addEventListener("click", function () {
        fileInput.click();
    });

    // preview image
    fileInput.addEventListener("change", function (event) {
        let file = event.target.files[0];
        if (file) {
            let reader = new FileReader();
            reader.onload = function (e) {
                imgPreview.src = e.target.result;
            };
            reader.readAsDataURL(file);
        }
    });
});

async function toggleFollow(username, follow) {
    const url = follow ? `/user/follow/${username}` : `/user/unfollow/${username}`;
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    });

    if (response.ok) {
        const result = await response.json();
        const followButton = document.getElementById('follow-button');
        const followersCountElement = document.querySelector('.followers-count');

        if (follow) {
            followButton.innerHTML = `
                <span>
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-check-circle" viewBox="0 0 16 16">
                        <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM6.97 11.03a.75.75 0 0 0 1.08 0l3.992-3.992a.75.75 0 1 0-1.08-1.08L7.5 9.439 5.53 7.47a.75.75 0 1 0-1.08 1.08l2.52 2.52z"/>
                    </svg>
                </span>
                <span>
                    Following ✅
                </span>
            `;
            followButton.setAttribute('onclick', `toggleFollow('${username}', false)`);
        } else {
            followButton.innerHTML = `
                <span>
                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-plus-circle" viewBox="0 0 16 16">
                        <path d="M8 1a7 7 0 1 1 0 14A7 7 0 0 1 8 1zm0 1a6 6 0 1 0 0 12A6 6 0 0 0 8 2zm3 5a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5H9v2.5a.5.5 0 0 1-1 0V9H5.5a.5.5 0 0 1 0-1H8V5.5a.5.5 0 0 1 1 0V8h2.5z"/>
                    </svg>
                </span>
                <span>
                    Follow
                </span>
            `;
            followButton.setAttribute('onclick', `toggleFollow('${username}', true)`);
        }

        followersCountElement.textContent = result.followersCount;
    } else {
        alert('An error occurred. Please try again.');
    }
}