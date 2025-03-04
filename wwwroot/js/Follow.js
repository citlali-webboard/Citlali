async function toggleFollow(username, shouldFollow) {
    try {
        shouldFollow = shouldFollow === true || shouldFollow === 'true';
        
        const button = event.target;
        const userElement = button.closest('.follow-item');
        
        const originalText = button.textContent;
        button.textContent = shouldFollow ? "Following..." : "Unfollowing...";
        button.disabled = true;
        button.style.opacity = 0.7;

        const url = shouldFollow ? `/user/follow/${username}` : `/user/unfollow/${username}`;
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        
        if (!token) {
            throw new Error("CSRF token not found");
        }
        
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        });

        if (response.ok) {
            if (!shouldFollow) {
                userElement.style.transition = "opacity 0.3s, transform 0.3s";
                userElement.style.opacity = "0";
                userElement.style.transform = "translateX(20px)";
                
                setTimeout(() => {
                    userElement.remove();
                }, 300);
            } else {
                button.textContent = "Following";
                button.disabled = false;
                button.style.opacity = 1;
                button.onclick = function() { 
                    toggleFollow(username, false); 
                };
            }
        } else {
            button.textContent = originalText;
            button.disabled = false;
            button.style.opacity = 1;
            
            const errorText = await response.text();
            console.error('Error:', errorText);
            alert('Failed to ' + (shouldFollow ? 'follow' : 'unfollow') + ' user. Please try again.');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred while processing your request.');
    }
}

async function toggleFollowTag(tagId, shouldFollow) {
    try {
        shouldFollow = shouldFollow === true || shouldFollow === 'true';
        
        const button = event.target;
        const tagElement = button.closest('.follow-item');
        
        const originalText = button.textContent;
        button.textContent = shouldFollow ? "Following..." : "Unfollowing...";
        button.disabled = true;
        button.style.opacity = 0.7;

        const url = shouldFollow ? `/user/followTag/${tagId}` : `/user/unfollowTag/${tagId}`;
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        
        if (!token) {
            throw new Error("CSRF token not found");
        }
        
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        });

        if (response.ok) {
            if (!shouldFollow) {
                tagElement.style.transition = "opacity 0.3s, transform 0.3s";
                tagElement.style.opacity = "0";
                tagElement.style.transform = "translateX(20px)";
                
                setTimeout(() => {
                    tagElement.remove();
                }, 300);
            } else {
                button.textContent = "Following";
                button.disabled = false;
                button.style.opacity = 1;
                button.onclick = function() { 
                    toggleFollowTag(tagId, false); 
                };
            }
        } else {
            button.textContent = originalText;
            button.disabled = false;
            button.style.opacity = 1;
            
            const errorText = await response.text();
            console.error('Error:', errorText);
            alert('Failed to ' + (shouldFollow ? 'follow' : 'unfollow') + ' tag. Please try again.');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred while processing your request.');
    }
}

function filterUsersandTags() {
    const searchInput = document.getElementById('searchInput').value.toLowerCase();
    
    const followItems = document.querySelectorAll('.follow-item');
    
    let visibleUsers = 0;
    let visibleTags = 0;
    
    const sectionHeaders = document.querySelectorAll('.follow-list > h2');
    const usersHeader = sectionHeaders[0];
    const tagsHeader = sectionHeaders[1];
    
    followItems.forEach(item => {
        const isUserItem = item.previousElementSibling === usersHeader || 
                          (item.previousElementSibling && 
                           item.previousElementSibling.previousElementSibling === usersHeader);
        
        let shouldShow = false;
        
        if (isUserItem) {
            const displayName = item.querySelector('.follow-info h2').textContent.toLowerCase();
            const username = item.querySelector('.follow-info p').textContent.toLowerCase();
            shouldShow = displayName.includes(searchInput) || username.includes(searchInput);
            
            if (shouldShow) visibleUsers++;
        } 
        else {
            const tagName = item.querySelector('.follow-info h2').textContent.toLowerCase();
            shouldShow = tagName.includes(searchInput);
            
            if (shouldShow) visibleTags++;
        }
        
        item.style.display = shouldShow ? 'flex' : 'none';
    });
    
    usersHeader.style.display = visibleUsers > 0 ? 'block' : 'none';
    tagsHeader.style.display = visibleTags > 0 ? 'block' : 'none';
    
    const followList = document.getElementById('followList');
    
    const existingNoResults = followList.querySelector('.no-results-message');
    if (existingNoResults) {
        followList.removeChild(existingNoResults);
    }
    
    if (searchInput && visibleUsers === 0 && visibleTags === 0) {
        const noResultsMessage = document.createElement('div');
        noResultsMessage.className = 'no-results-message';
        noResultsMessage.textContent = 'No users or tags found matching your search.';
        followList.appendChild(noResultsMessage);
    }
}

function closeFollowPopup(username) {
    window.location.href = `/user/${username}`;
}

document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        const sectionHeaders = document.querySelectorAll('.follow-list > h2');
        if (sectionHeaders.length > 1) {
            searchInput.addEventListener('input', filterUsersandTags);
        } else {
            searchInput.addEventListener('input', filterUsers);
        }
    }
});

async function toggleUnFollow(username) {
    try {
        const button = event.target;
        const userElement = button.closest('.follow-item');
        
        const originalText = button.textContent;
        button.textContent = "Removing...";
        button.disabled = true;
        button.style.opacity = 0.7;
        
        const url = `/user/removeFollower/${username}`;
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        
        if (!token) {
            throw new Error("CSRF token not found");
        }
        
        const response = await fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            }
        });

        if (response.ok) {
            userElement.style.transition = "opacity 0.3s, transform 0.3s";
            userElement.style.opacity = "0";
            userElement.style.transform = "translateX(20px)";
            
            setTimeout(() => {
                userElement.remove();
            }, 300);
        } else {
            button.textContent = originalText;
            button.disabled = false;
            button.style.opacity = 1;
            
            const errorText = await response.text();
            console.error('Error:', errorText);
            alert('Failed to remove follower. Please try again.');
        }
    } catch (error) {
        console.error('Error:', error);
        alert('An error occurred while processing your request.');
    }
}
