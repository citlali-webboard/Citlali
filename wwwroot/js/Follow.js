async function toggleFollow(username, shouldFollow) {
    shouldFollow = shouldFollow === true || shouldFollow === 'true';

    const url = shouldFollow ? `/user/follow/${username}` : `/user/unfollow/${username}`;
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    });

    if (response.ok) {
        window.location.reload();
    } else {
        const errorText = await response.text();
        console.error('Error:', errorText);
        alert('An error occurred. Please try again.');
    }
}

async function toggleFollowTag(tagId, shouldFollow) {
    shouldFollow = shouldFollow === true || shouldFollow === 'true';

    const url = shouldFollow ? `/user/followTag/${tagId}` : `/user/unfollowTag/${tagId}`;
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    });

    if (response.ok) {
        window.location.reload();
    } else {
        const errorText = await response.text();
        console.error('Error:', errorText);
        alert('An error occurred. Please try again.');
    }
}

async function closeFollowPopup(username) {
    window.location.href = `/user/profile/${username}`;
}