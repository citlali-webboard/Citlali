async function toggleFollow(username, shouldFollow) {
    try {
        shouldFollow = shouldFollow === true || shouldFollow === 'true';
        
        const button = event.target;
        const userElement = button.closest('.follow-item');
        
        // Change button appearance to show processing state
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
                // If unfollowing, animate removal of the user element from the DOM
                userElement.style.transition = "opacity 0.3s, transform 0.3s";
                userElement.style.opacity = "0";
                userElement.style.transform = "translateX(20px)";
                
                setTimeout(() => {
                    userElement.remove();
                }, 300);
            } else {
                // If following, you might want to update the button or reload
                button.textContent = "Following";
                button.disabled = false;
                button.style.opacity = 1;
                button.onclick = function() { 
                    toggleFollow(username, false); 
                };
            }
        } else {
            // Restore button state on error
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
        
        // Change button appearance to show processing state
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
                // If unfollowing, animate removal of the tag element from the DOM
                tagElement.style.transition = "opacity 0.3s, transform 0.3s";
                tagElement.style.opacity = "0";
                tagElement.style.transform = "translateX(20px)";
                
                setTimeout(() => {
                    tagElement.remove();
                }, 300);
            } else {
                // If following, update the button
                button.textContent = "Following";
                button.disabled = false;
                button.style.opacity = 1;
                button.onclick = function() { 
                    toggleFollowTag(tagId, false); 
                };
            }
        } else {
            // Restore button state on error
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
    // Get the search input value
    const searchInput = document.getElementById('searchInput').value.toLowerCase();
    
    // Get all follow items
    const followItems = document.querySelectorAll('.follow-item');
    
    // Keep track of visible users and tags to show/hide section headers
    let visibleUsers = 0;
    let visibleTags = 0;
    
    // Get section headers
    const sectionHeaders = document.querySelectorAll('.follow-list > h2');
    const usersHeader = sectionHeaders[0];
    const tagsHeader = sectionHeaders[1];
    
    // Filter users and tags
    followItems.forEach(item => {
        // Check if this item is under users or tags section
        const isUserItem = item.previousElementSibling === usersHeader || 
                          (item.previousElementSibling && 
                           item.previousElementSibling.previousElementSibling === usersHeader);
        
        let shouldShow = false;
        
        if (isUserItem) {
            // User item - check display name and username
            const displayName = item.querySelector('.follow-info h2').textContent.toLowerCase();
            const username = item.querySelector('.follow-info p').textContent.toLowerCase();
            shouldShow = displayName.includes(searchInput) || username.includes(searchInput);
            
            if (shouldShow) visibleUsers++;
        } 
        else {
            // Tag item - check tag name (emoji + name)
            const tagName = item.querySelector('.follow-info h2').textContent.toLowerCase();
            shouldShow = tagName.includes(searchInput);
            
            if (shouldShow) visibleTags++;
        }
        
        // Show or hide based on search match
        item.style.display = shouldShow ? 'flex' : 'none';
    });
    
    // Show/hide section headers based on visibility
    usersHeader.style.display = visibleUsers > 0 ? 'block' : 'none';
    tagsHeader.style.display = visibleTags > 0 ? 'block' : 'none';
    
    // Handle the "no results" message
    const followList = document.getElementById('followList');
    
    // Remove any existing "no results" message
    const existingNoResults = followList.querySelector('.no-results-message');
    if (existingNoResults) {
        followList.removeChild(existingNoResults);
    }
    
    // Add "no results" message if needed
    if (searchInput && visibleUsers === 0 && visibleTags === 0) {
        const noResultsMessage = document.createElement('div');
        noResultsMessage.className = 'no-results-message';
        noResultsMessage.textContent = 'No users or tags found matching your search.';
        followList.appendChild(noResultsMessage);
    }
}

// Ensure the closeFollowPopup function is defined
function closeFollowPopup(username) {
    window.location.href = `/user/${username}`;
}

document.addEventListener('DOMContentLoaded', function() {
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        // Check if we're on the Following page by looking for multiple h2 headers in the follow-list
        const sectionHeaders = document.querySelectorAll('.follow-list > h2');
        if (sectionHeaders.length > 1) {
            // We're on the Following page with both users and tags
            searchInput.addEventListener('input', filterUsersandTags);
        } else {
            // We're on the Followers page with just users
            searchInput.addEventListener('input', filterUsers);
        }
    }
});

async function toggleUnFollow(username) {
    try {
        const button = event.target;
        const userElement = button.closest('.follow-item');
        
        // Change button appearance to show processing state
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
            // Animate removal of the user element from the DOM
            userElement.style.transition = "opacity 0.3s, transform 0.3s";
            userElement.style.opacity = "0";
            userElement.style.transform = "translateX(20px)";
            
            setTimeout(() => {
                userElement.remove();
            }, 300);
        } else {
            // Restore button state on error
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
