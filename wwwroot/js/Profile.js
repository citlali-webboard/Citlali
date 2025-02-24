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
        const followButton = document.getElementById('follow-button');
        const followersCountElement = document.querySelector('.data h3.followers-count');
        let followersCount = parseInt(followersCountElement.textContent);

        if (follow) {
            followButton.innerHTML = `
                <span>
                    Following ✅
                </span>
            `;
            followButton.setAttribute('onclick', `toggleFollow('${username}', false)`);
            followersCountElement.textContent = followersCount + 1;
        } else {
            followButton.innerHTML = `
                <span>
                    Follow
                </span>
            `;
            followButton.setAttribute('onclick', `toggleFollow('${username}', true)`);
            followersCountElement.textContent = followersCount - 1;
        }
    } else {
        alert('An error occurred. Please try again.');
    }
}
