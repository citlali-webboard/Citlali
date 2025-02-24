document.addEventListener("DOMContentLoaded", function() {
    const followButton = document.getElementById("followButton");
    if (followButton) {
        followButton.addEventListener("click", async function() {
            const button = this;
            const tagId = button.getAttribute("data-tag-id");
            const isFollowing = button.textContent.trim() === "Unfollow";
            const url = isFollowing ? `/user/unfollowTag/${tagId}` : `/user/followTag/${tagId}`;
            const csrfToken = document.getElementById("csrfToken").value;

            try {
                const response = await fetch(url, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "X-CSRF-TOKEN": csrfToken
                    }
                });

                if (response.ok) {
                    button.textContent = isFollowing ? "Follow" : "Unfollow";
                } else {
                    console.error("Failed to follow/unfollow tag");
                }
            } catch (error) {
                console.error("Error:", error);
            }
        });
    }
});