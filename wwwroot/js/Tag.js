document.addEventListener('DOMContentLoaded', function() {
    // Tags container toggle functionality
    const tagsContainer = document.getElementById('tags-container');
    const toggleBtn = document.getElementById('tags-toggle-btn');
    
    if (toggleBtn && tagsContainer) {
        toggleBtn.addEventListener('click', function() {
            tagsContainer.classList.toggle('expanded');
            toggleBtn.classList.toggle('expanded');
            
            if (tagsContainer.classList.contains('expanded')) {
                toggleBtn.querySelector('.tags-toggle-text').textContent = 'See less';
            } else {
                toggleBtn.querySelector('.tags-toggle-text').textContent = 'See more';
            }
        });
    }

    // Locations container toggle functionality
    const locationContainer = document.getElementById('location-container');
    const locationToggleBtn = document.getElementById('location-toggle-btn');
    
    if (locationToggleBtn && locationContainer) {
        locationToggleBtn.addEventListener('click', function() {
            locationContainer.classList.toggle('expanded');
            locationToggleBtn.classList.toggle('expanded');
            
            if (locationContainer.classList.contains('expanded')) {
                locationToggleBtn.querySelector('.location-toggle-text').textContent = 'See less';
            } else {
                locationToggleBtn.querySelector('.location-toggle-text').textContent = 'See more';
            }
        });
    }

    // Sort buttons functionality
    const sortButtons = document.querySelectorAll('.sort-btn');
    
    sortButtons.forEach(button => {
        button.addEventListener('click', function() {
            const sortBy = this.getAttribute('data-sort');
            window.location.href = updateUrlParameter(window.location.href, 'sortBy', sortBy);
        });
    });

    // Get the current URL and parameters
    const url = new URL(window.location.href);
    const sortBy = url.searchParams.get('sortBy');
    
    // Define valid sort options
    const validSortOptions = ['newest', 'date', 'popularity'];
    
    // Check if sortBy parameter exists and is invalid
    if (sortBy && !validSortOptions.includes(sortBy)) {
        // Replace with default 'newest' and update URL without reloading the page
        url.searchParams.set('sortBy', 'newest');
        window.history.replaceState({}, '', url.toString());
    }
});

// Helper function to toggle tag following
function toggleFollowTag(tagId, shouldFollow) {
    const csrfToken = document.querySelector('input[name="__RequestVerificationToken"]').value;
    const followButton = document.getElementById('followButton');

    shouldFollow = shouldFollow === 'true' || shouldFollow === true;
    const url = shouldFollow ? `/user/followTag/${tagId}` : `/user/unfollowTag/${tagId}`;
    
    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': csrfToken
        },
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            // Update UI
            if (shouldFollow) {
                followButton.innerHTML = `<span class="icon-container"><svg style="width: 16px; fill: var(--neutral-foreground-rest);" focusable="false" viewBox="0 0 16 16" aria-hidden="true"><path d="M2 8a6 6 0 1 1 12 0A6 6 0 0 1 2 8Zm6-7a7 7 0 1 0 0 14A7 7 0 0 0 8 1Zm2.85 5.85a.5.5 0 0 0-.7-.7l-2.9 2.9-1.4-1.4a.5.5 0 1 0-.7.7L6.9 10.1c.2.2.5.2.7 0l3.25-3.25Z"></path></svg></span><span>Following</span>`;
                followButton.classList.add('following');
            } else {
                followButton.innerHTML = `<span class="icon-container"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="12" y1="5" x2="12" y2="19"></line><line x1="5" y1="12" x2="19" y2="12"></line></svg></span><span>Follow</span>`;
                followButton.classList.remove('following');
            }
            
            // Update follower count
            const followerCountElem = document.querySelector('.follower-count');
            if (followerCountElem) {
                const count = parseInt(followerCountElem.textContent);
                followerCountElem.textContent = `${shouldFollow ? count + 1 : count - 1} followers`;
            }
            
            // Update button onclick attribute
            followButton.setAttribute('onclick', `toggleFollowTag('${tagId}', '${!shouldFollow}')`);
        }
    })
    .catch(error => {
        console.error('Error:', error);
    });
}

// Helper function to update URL parameters
function updateUrlParameter(url, param, value) {
    const urlObj = new URL(url);
    urlObj.searchParams.set(param, value);
    // Only set valid sort options for sortBy parameter
    if (param === 'sortBy') {
        const validSortOptions = ['newest', 'date', 'popularity'];
        if (!validSortOptions.includes(value)) {
            value = 'newest';
        }
    }
    return urlObj.toString();
}