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
    
});

function initImageSlider() {
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
    let slideInterval = setInterval(nextSlide, 5000);
    
    function showSlide(index) {
        // Remove active class from all slides and dots
        slides.forEach(slide => slide.classList.remove('active'));
        dots.forEach(dot => dot.classList.remove('active'));
        
        // Add active class to current slide and dot
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
    
    // Add event listeners
    if (prevBtn) {
        prevBtn.addEventListener('click', function() {
            clearInterval(slideInterval);
            prevSlide();
            slideInterval = setInterval(nextSlide, 5000);
        });
    }
    
    if (nextBtn) {
        nextBtn.addEventListener('click', function() {
            clearInterval(slideInterval);
            nextSlide();
            slideInterval = setInterval(nextSlide, 5000);
        });
    }
    
    dots.forEach((dot, index) => {
        dot.addEventListener('click', function() {
            clearInterval(slideInterval);
            currentIndex = index;
            showSlide(currentIndex);
            slideInterval = setInterval(nextSlide, 5000);
        });
    });
    
    // Pause rotation when hovering over slider
    sliderContainer.addEventListener('mouseenter', function() {
        clearInterval(slideInterval);
    });
    
    sliderContainer.addEventListener('mouseleave', function() {
        slideInterval = setInterval(nextSlide, 5000);
    });
}