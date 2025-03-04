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
    
    // First identify all user and tag elements and check visibility
    followItems.forEach(item => {
        // Better method to determine if item is a user or tag
        // Check if the item is before tagsHeader in the DOM tree
        const itemIndex = Array.from(item.parentNode.children).indexOf(item);
        const tagsHeaderIndex = Array.from(item.parentNode.children).indexOf(tagsHeader);
        const isUserItem = itemIndex < tagsHeaderIndex;
        
        let shouldShow = false;
        
        if (isUserItem) {
            // For user items
            const displayName = item.querySelector('.follow-info h2')?.textContent.toLowerCase() || '';
            const username = item.querySelector('.follow-info p')?.textContent.toLowerCase() || '';
            shouldShow = displayName.includes(searchInput) || username.includes(searchInput);
            
            if (shouldShow) visibleUsers++;
        } 
        else {
            // For tag items
            const tagName = item.querySelector('div h2')?.textContent.toLowerCase() || '';
            shouldShow = tagName.includes(searchInput);
            
            if (shouldShow) visibleTags++;
        }
        
        item.style.display = shouldShow ? 'flex' : 'none';
    });
    
    // Handle section visibility
    // If searching for something, show relevant sections
    if (searchInput) {
        // Show/hide based on results
        usersHeader.style.display = visibleUsers > 0 ? 'block' : 'none';
        tagsHeader.style.display = visibleTags > 0 ? 'block' : 'none';
    } else {
        // If not searching, show all sections
        usersHeader.style.display = 'block';
        tagsHeader.style.display = 'block';
    }
    
    const followList = document.getElementById('followList');
    
    // Remove existing "no results" message if any
    const existingNoResults = followList.querySelector('.no-results-message');
    if (existingNoResults) {
        followList.removeChild(existingNoResults);
    }
    
    // Show "no results" message if needed
    if (searchInput && visibleUsers === 0 && visibleTags === 0) {
        const noResultsMessage = document.createElement('div');
        noResultsMessage.className = 'no-results-message';
        noResultsMessage.textContent = 'No users or tags found matching your search.';
        followList.appendChild(noResultsMessage);
    }
}

// Add the filterUsers function that was missing
function filterUsers() {
    const searchInput = document.getElementById('searchInput').value.toLowerCase();
    const followItems = document.querySelectorAll('.follow-item');
    
    let visibleCount = 0;
    
    followItems.forEach(item => {
        const displayName = item.querySelector('.follow-info h2').textContent.toLowerCase();
        const username = item.querySelector('.follow-info p').textContent.toLowerCase();
        const shouldShow = displayName.includes(searchInput) || username.includes(searchInput);
        
        item.style.display = shouldShow ? 'flex' : 'none';
        if (shouldShow) visibleCount++;
    });
    
    const followList = document.getElementById('followList');
    
    // Remove existing no results message if any
    const existingNoResults = followList.querySelector('.no-results-message');
    if (existingNoResults) {
        followList.removeChild(existingNoResults);
    }
    
    // Add no results message if needed
    if (searchInput && visibleCount === 0) {
        const noResultsMessage = document.createElement('div');
        noResultsMessage.className = 'no-results-message';
        noResultsMessage.textContent = 'No users found matching your search.';
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
