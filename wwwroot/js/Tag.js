document.addEventListener("DOMContentLoaded", async function() {
    const followButton = document.getElementById("followButton");
    if (followButton) {
        const tagId = followButton.getAttribute("data-tag-id");
        const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]').value;

        try {
            const response = await fetch(`/user/isFollowingTag/${tagId}`, {
                method: "GET",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": csrfToken
                }
            });

            if (response.ok) {
                const result = await response.json();
                followButton.textContent = result.isFollowing ? "Unfollow" : "Follow";
            } else {
                console.error('Error fetching follow status');
            }
        } catch (error) {
            console.error("Error:", error);
        }

        followButton.addEventListener("click", async function() {
            const isFollowing = followButton.textContent.trim() === "Unfollow";
            const url = isFollowing ? `/user/unfollowTag/${tagId}` : `/user/followTag/${tagId}`;

            try {
                const response = await fetch(url, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "RequestVerificationToken": csrfToken
                    }
                });

                if (response.ok) {
                    const result = await response.json();
                    if (result.success) {
                        followButton.textContent = isFollowing ? "Follow" : "Unfollow";
                    } else {
                        console.error("Failed to follow/unfollow tag");
                    }
                } else {
                    const errorText = await response.text();
                    console.error('Error:', errorText);
                    alert('An error occurred. Please try again.');
                }
            } catch (error) {
                console.error("Error:", error);
            }
        });
    }
});