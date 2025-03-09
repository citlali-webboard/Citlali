document.addEventListener('DOMContentLoaded', function() {

    initImageSlider();

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

function initImageSlider() {
    const INTERVAL = 3500;  // ms
    const sliderContainer = document.querySelector('.image-slider-container');
    if (!sliderContainer) return;
    
    const slides = sliderContainer.querySelectorAll('.slide');
    const dots = sliderContainer.querySelectorAll('.dot');
    const prevBtn = sliderContainer.querySelector('.prev-btn');
    const nextBtn = sliderContainer.querySelector('.next-btn');
    
    if (slides.length <= 1) return;
    
    let currentIndex = 0;
    const totalSlides = slides.length;
    
    // Set up auto rotation
    let slideInterval = setInterval(nextSlide, INTERVAL);
    
    function showSlide(index) {
        slides.forEach(slide => slide.classList.remove('active'));
        dots.forEach(dot => dot.classList.remove('active'));
        
        slides[index].classList.add('active');
        dots[index].classList.add('active');
    }
    
    function nextSlide() {
        currentIndex = (currentIndex + 1) % totalSlides;
        showSlide(currentIndex);
    }
    
    function prevSlide() {
        currentIndex = (currentIndex - 1 + totalSlides) % totalSlides;
        showSlide(currentIndex);
    }
    
    if (prevBtn) {
        prevBtn.addEventListener('click', function() {
            prevSlide();
            clearInterval(slideInterval);
        });
    }
    
    if (nextBtn) {
        nextBtn.addEventListener('click', function() {
            nextSlide();
            clearInterval(slideInterval);
        });
    }
    
    dots.forEach((dot, index) => {
        dot.addEventListener('click', function() {
            currentIndex = index;
            showSlide(currentIndex);
            clearInterval(slideInterval);
        });
    });
    
    sliderContainer.addEventListener('mouseenter', function() {
        clearInterval(slideInterval);
    });
    
    sliderContainer.addEventListener('mouseleave', function() {
        slideInterval = setInterval(nextSlide, INTERVAL);
    });
    
    let touchStartX = 0;
    
    sliderContainer.addEventListener('touchstart', function(e) {
        touchStartX = e.changedTouches[0].screenX;
        clearInterval(slideInterval);
    }, {passive: true});
    
    sliderContainer.addEventListener('touchend', function(e) {
        const touchEndX = e.changedTouches[0].screenX;
        const diff = touchEndX - touchStartX;
        
        if (diff > 50) {
            // Swipe right - show previous slide
            prevSlide();
        } else if (diff < -50) {
            // Swipe left - show next slide
            nextSlide();
        }
        
        // Restart the timer
        slideInterval = setInterval(nextSlide, INTERVAL);
    }, {passive: true});
}

// Helper function to update URL parameters (can be used by other functions)
function updateUrlParameter(url, param, value) {
    const urlObj = new URL(url);
    
    // Only set valid sort options for sortBy parameter
    if (param === 'sortBy') {
        const validSortOptions = ['newest', 'date', 'popularity'];
        if (!validSortOptions.includes(value)) {
            value = 'newest';
        }
    }
    
    urlObj.searchParams.set(param, value);
    return urlObj.toString();
}