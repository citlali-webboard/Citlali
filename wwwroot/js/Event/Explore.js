document.addEventListener('DOMContentLoaded', function() {
    const tagsContainer = document.getElementById('tags-container');
    const toggleBtn = document.getElementById('tags-toggle-btn');
    const toggleText = toggleBtn.querySelector('.tags-toggle-text');
    
    if (toggleBtn) {
        toggleBtn.addEventListener('click', function() {
            tagsContainer.classList.toggle('expanded');
            toggleBtn.classList.toggle('expanded');
            
            if (tagsContainer.classList.contains('expanded')) {
                toggleText.textContent = 'See less';
            } else {
                toggleText.textContent = 'See more';
            }
        });
    }

    const locationContainer = document.getElementById('location-container');
    const locationToggleBtn = document.getElementById('location-toggle-btn');
    const locationToggleText = locationToggleBtn.querySelector('.location-toggle-text');
    if (locationToggleBtn) {
        locationToggleBtn.addEventListener('click', function() {
            locationContainer.classList.toggle('expanded');
            locationToggleBtn.classList.toggle('expanded');
            
            if (locationContainer.classList.contains('expanded')) {
                locationToggleText.textContent = 'See less';
            } else {
                locationToggleText.textContent = 'See more';
            }
        });
    }

    // Sort buttons functionality
    const sortButtons = document.querySelectorAll('.sort-btn');
    sortButtons.forEach(button => {
        button.addEventListener('click', function() {
            const sortBy = this.getAttribute('data-sort');
            const currentUrl = new URL(window.location.href);
            
            // Update the sortBy parameter
            currentUrl.searchParams.set('sortBy', sortBy);
            
            // Keep the current page if it exists
            const currentPage = currentUrl.searchParams.get('page');
            if (currentPage) {
                currentUrl.searchParams.set('page', currentPage);
            }
            
            // Navigate to the updated URL
            window.location.href = currentUrl.toString();
        });
    });
});