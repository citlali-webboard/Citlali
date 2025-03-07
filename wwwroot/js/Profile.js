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

async function toggleFollow(username, shouldFollow) {
    shouldFollow = shouldFollow === true || shouldFollow === 'true';

    const url = shouldFollow ? `/user/follow/${username}` : `/user/unfollow/${username}`;
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        }
    });

    if (response.ok) {
        if (response.redirected === true) {
            window.location.href = response.url;
            return;
        }

        const result = await response.json();
        const followButton = document.getElementById('follow-button');
        const followersCountElement = document.querySelector('.followers-count');

        if (shouldFollow) {
            followButton.innerHTML = `
                <span>
                    <svg style="width: 16px; fill: var(--neutral-foreground-rest);" focusable="false" viewBox="0 0 16 16" aria-hidden="true"><path d="M2 8a6 6 0 1 1 12 0A6 6 0 0 1 2 8Zm6-7a7 7 0 1 0 0 14A7 7 0 0 0 8 1Zm2.85 5.85a.5.5 0 0 0-.7-.7l-2.9 2.9-1.4-1.4a.5.5 0 1 0-.7.7L6.9 10.1c.2.2.5.2.7 0l3.25-3.25Z"></path></svg>
                </span>
                <span>
                    Following
                </span>
            `;
            followButton.setAttribute('onclick', `toggleFollow('${username}', 'false')`);
            followButton.classList.add('following');
        } 
        else {
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
                followButton.setAttribute('onclick', `toggleFollow('${username}', 'true')`);
                followButton.classList.remove('following');
        }

        followersCountElement.textContent = result.followersCount;
    } else {
        const errorText = await response.text();
        console.error('Error:', errorText);
        alert('An error occurred. Please try again.');
    }
}