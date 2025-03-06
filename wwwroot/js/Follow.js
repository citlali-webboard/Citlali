// Update the toggleFollow function to update counter
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
                
                // Update the following count
                updateFollowingCount(-1);
                
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
                
                // Update the following count
                updateFollowingCount(1);
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

// Update the toggleFollowTag function to update counter
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
                
                // Update the following count
                updateFollowingCount(-1);
                
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
                
                // Update the following count
                updateFollowingCount(1);
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

// Update the toggleUnFollow function to update counter
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
            
            // Update the follower count
            updateFollowerCount(-1);
            
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

// Helper function to update the follower count in the UI
function updateFollowerCount(change) {
    const followerTab = document.querySelector('.tab-button:nth-child(1)');
    if (followerTab) {
        const countSpan = followerTab.querySelector('.count');
        if (countSpan) {
            const currentCount = parseInt(countSpan.textContent);
            countSpan.textContent = Math.max(0, currentCount + change);
            
            // Check if we need to show empty state
            const followList = document.getElementById('followersList');
            if (followList && currentCount + change === 0) {
                // All followers were removed, show empty state
                const items = followList.querySelectorAll('.follow-item');
                if (items.length === 0) {
                    const emptyState = createEmptyState('follower');
                    followList.innerHTML = '';
                    followList.appendChild(emptyState);
                }
            }
        }
    }
}

// Helper function to update the following count in the UI
function updateFollowingCount(change) {
    const followingTab = document.querySelector('.tab-button:nth-child(2)');
    if (followingTab) {
        const countSpan = followingTab.querySelector('.count');
        if (countSpan) {
            const currentCount = parseInt(countSpan.textContent);
            countSpan.textContent = Math.max(0, currentCount + change);
            
            // Check if we need to show empty state
            const followingList = document.getElementById('followingList');
            if (followingList && currentCount + change === 0) {
                // All following items were removed, show empty state
                const items = followingList.querySelectorAll('.follow-item');
                if (items.length === 0) {
                    const emptyState = createEmptyState('following');
                    followingList.innerHTML = '';
                    followingList.appendChild(emptyState);
                }
            }
        }
    }
}

// Helper function to create empty state elements
function createEmptyState(type) {
    const emptyState = document.createElement('div');
    emptyState.className = 'empty-state';
    
    const emptyIcon = document.createElement('div');
    emptyIcon.className = 'empty-icon';
    
    const emptyTitle = document.createElement('h3');
    const emptyText = document.createElement('p');
    
    if (type === 'follower') {
        emptyIcon.textContent = '👤';
        emptyTitle.textContent = 'No followers yet';
        
        // Get username from profile or use generic message
        const displayName = document.querySelector('.user-profile h1')?.textContent?.trim() || 'This user';
        const isCurrentUser = document.querySelector('.follows-container').getAttribute('data-is-current-user') === 'true';
        
        emptyText.textContent = `When people follow ${isCurrentUser ? 'you' : displayName}, they'll appear here.`;
    } else {
        emptyIcon.textContent = '🔍';
        emptyTitle.textContent = 'Not following anyone';
        
        // Get username from profile or use generic message
        const displayName = document.querySelector('.user-profile h1')?.textContent?.trim() || 'This user';
        const isCurrentUser = document.querySelector('.follows-container').getAttribute('data-is-current-user') === 'true';
        
        emptyText.textContent = `${isCurrentUser ? 'You aren\'t' : displayName + ' isn\'t'} following anyone or any tags yet.`;
    }
    
    emptyState.appendChild(emptyIcon);
    emptyState.appendChild(emptyTitle);
    emptyState.appendChild(emptyText);
    
    return emptyState;
}

function filterUsersandTags() {
    const searchInput = document.getElementById('searchInput').value.toLowerCase();
    
    const followItems = document.querySelectorAll('.follow-item');
    
    let visibleUsers = 0;
    let visibleTags = 0;
    
    const sectionHeaders = document.querySelectorAll('.section-header');
    let usersHeader, tagsHeader;
    
    if (sectionHeaders.length >= 2) {
        usersHeader = sectionHeaders[0];
        tagsHeader = sectionHeaders[1];
    } else if (sectionHeaders.length === 1) {
        // Determine if the only header is for users or tags
        const headerText = sectionHeaders[0].querySelector('h2').textContent.toLowerCase();
        if (headerText.includes('people')) {
            usersHeader = sectionHeaders[0];
        } else if (headerText.includes('tag')) {
            tagsHeader = sectionHeaders[0];
        }
    }
    
    // First identify all user and tag elements and check visibility
    followItems.forEach(item => {
        const type = item.getAttribute('data-type');
        const dataName = item.getAttribute('data-name') || '';
        let shouldShow = dataName.includes(searchInput);
        
        if (type === 'tag') {
            if (shouldShow) visibleTags++;
        } else {
            if (shouldShow) visibleUsers++;
        }
        
        item.style.display = shouldShow ? 'flex' : 'none';
    });
    
    // Handle section visibility
    if (usersHeader) {
        usersHeader.style.display = (searchInput && visibleUsers === 0) ? 'none' : 'block';
    }
    
    if (tagsHeader) {
        tagsHeader.style.display = (searchInput && visibleTags === 0) ? 'none' : 'block';
    }
    
    const followList = document.getElementById('followingList');
    
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

