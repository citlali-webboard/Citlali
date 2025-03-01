async function toggleFollowTag(tagId, follow) {
    const url = follow ? `/user/followTag/${tagId}` : `/user/unfollowTag/${tagId}`;
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

    console.log(`Sending request to ${url} with follow=${follow}`);

    try {
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': csrfToken
            }
        });

        console.log(`Response status: ${response.status}`);

        if (response.ok) {
            const result = await response.json();
            const followButton = document.getElementById('followButton');

            console.log(`Result: ${JSON.stringify(result)}`);

            if (result.success) {
                if (follow) {
                    followButton.innerHTML = `
                        <span>
                            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-check-circle" viewBox="0 0 16 16">
                                <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM6.97 11.03a.75.75 0 0 0 1.08 0l3.992-3.992a.75.75 0 1 0-1.08-1.08L7.5 9.439 5.53 7.47a.75.75 0 1 0-1.08 1.08l2.52 2.52z"/>
                            </svg>
                        </span>
                        <span>
                            Unfollow
                        </span>
                    `;
                    followButton.setAttribute('onclick', `toggleFollowTag('${tagId}', false)`);
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
                    followButton.setAttribute('onclick', `toggleFollowTag('${tagId}', true)`);
                }
            } else {
                console.error("Failed to follow/unfollow tag");
                alert("Failed to follow/unfollow tag. Please try again.");
            }
        } else {
            let errorText = "An error occurred";
            try {
                errorText = await response.text();
            } catch (e) {
                console.error("Could not parse error text", e);
            }
            console.error('Error:', errorText);
            alert('An error occurred. Please try again.');
        }
    } catch (error) {
        console.error("Error:", error);
        alert(`An unexpected error occurred: ${error.message}`);
    }
}

// Set follow button event listener when the page loads
document.addEventListener("DOMContentLoaded", function() {
    const followButton = document.getElementById('followButton');
    if (followButton) {
        const tagId = followButton.getAttribute('data-tag-id');
        const isFollowing = followButton.textContent.trim() === "Unfollow";
        followButton.setAttribute('onclick', `toggleFollowTag('${tagId}', ${!isFollowing})`);
    }
});