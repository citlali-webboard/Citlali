// Wait for DOM to be fully loaded
document.addEventListener('DOMContentLoaded', function() {
    // Log to ensure script is running

    // Initialize participant management
    initParticipantManagement();
    
    // Set up search input event listener
    const searchInput = document.getElementById('participant-search');
    if (searchInput) {
        searchInput.addEventListener('input', function() {
            filterParticipants();
        });
    } else {
        console.error('Search input element not found!');
    }
    
    // Set up filter and sort options
    setupFilterAndSort();
});

// Initialize participant management
function initParticipantManagement() {
    // Store original order of participants for reset
    storeOriginalParticipantsOrder();
    
    // Initialize sorting to default (newest first)
    sortParticipants('date-desc');
    
    // Open/close filter and sort menus on click
    setupDropdownMenus();
}

// Store the original DOM order of participants
function storeOriginalParticipantsOrder() {
    const container = document.querySelector('.registrants-container');
    if (!container) {
        console.error('Registrants container not found!');
        return;
    }
    
    // Add data-original-index to each participant card
    const cards = container.querySelectorAll('.registrant-card');
    cards.forEach((card, index) => {
        card.setAttribute('data-original-index', index);
    });
}

// Setup dropdown menus (filter and sort)
function setupDropdownMenus() {
    // Toggle filter menu
    const filterButton = document.getElementById('filter-button');
    const filterMenu = document.getElementById('filter-menu');
    
    if (filterButton && filterMenu) {
        filterButton.addEventListener('click', function(e) {
            e.stopPropagation();
            filterMenu.style.display = filterMenu.style.display === 'block' ? 'none' : 'block';
            
            // Hide sort menu when filter menu is shown
            if (sortMenu) sortMenu.style.display = 'none';
        });
    }
    
    // Toggle sort menu
    const sortButton = document.getElementById('sort-button');
    const sortMenu = document.getElementById('sort-menu');
    
    if (sortButton && sortMenu) {
        sortButton.addEventListener('click', function(e) {
            e.stopPropagation();
            sortMenu.style.display = sortMenu.style.display === 'block' ? 'none' : 'block';
            
            // Hide filter menu when sort menu is shown
            if (filterMenu) filterMenu.style.display = 'none';
        });
    }
    
    // Close menus when clicking elsewhere on the document
    document.addEventListener('click', function() {
        if (filterMenu) filterMenu.style.display = 'none';
        if (sortMenu) sortMenu.style.display = 'none';
    });
    
    // Prevent menu closing when clicking inside menu
    if (filterMenu) {
        filterMenu.addEventListener('click', function(e) {
            e.stopPropagation();
        });
    }
    
    if (sortMenu) {
        sortMenu.addEventListener('click', function(e) {
            e.stopPropagation();
        });
    }
}

// Set up filter and sort functionality
function setupFilterAndSort() {
    // Set up filter options
    const filterOptions = document.querySelectorAll('.filter-option');
    if (filterOptions.length > 0) {
        filterOptions.forEach(option => {
            option.addEventListener('click', function() {
                // Update active state
                filterOptions.forEach(opt => opt.classList.remove('active'));
                this.classList.add('active');
                
                // Update filter button text
                const filterButton = document.getElementById('filter-button');
                if (filterButton) {
                    const filterSpan = filterButton.querySelector('span');
                    if (filterSpan) {
                        filterSpan.textContent = this.textContent === 'All statuses' ? 'All statuses' : this.textContent;
                    }
                }
                                
                // Apply filter
                filterParticipants();
                
                // Close the menu
                const filterMenu = document.getElementById('filter-menu');
                if (filterMenu) filterMenu.style.display = 'none';
            });
        });
    } else {
        console.error('No filter options found!');
    }
    
    // Set up sort options
    const sortOptions = document.querySelectorAll('.sort-option');
    if (sortOptions.length > 0) {
        sortOptions.forEach(option => {
            option.addEventListener('click', function() {
                // Update active state
                sortOptions.forEach(opt => opt.classList.remove('active'));
                this.classList.add('active');
                
                // Update sort button text
                const sortButton = document.getElementById('sort-button');
                if (sortButton) {
                    const sortSpan = sortButton.querySelector('span');
                    if (sortSpan) {
                        sortSpan.textContent = this.textContent;
                    }
                }
                
                const sortValue = this.getAttribute('data-value');
                
                // Apply sort
                sortParticipants(sortValue);
                
                // Close the menu
                const sortMenu = document.getElementById('sort-menu');
                if (sortMenu) sortMenu.style.display = 'none';
            });
        });
    } else {
        console.error('No sort options found!');
    }
}

// Main function to filter and sort participants
function filterParticipants() {
    const searchInput = document.getElementById('participant-search');
    const searchTerm = searchInput ? searchInput.value.toLowerCase() : '';
    
    const activeFilterOption = document.querySelector('.filter-option.active');
    const statusFilter = activeFilterOption ? activeFilterOption.getAttribute('data-value') : 'all';
    
    const container = document.querySelector('.registrants-container');
    const noResults = document.getElementById('no-participants-found');
    
    if (!container) {
        console.error('Registrants container not found!');
        return;
    }
        
    const cards = container.querySelectorAll('.registrant-card');
    let visibleCount = 0;
    
    cards.forEach(card => {
        const name = card.getAttribute('data-name') || '';
        const status = card.getAttribute('data-status') || '';
        
        // Match search term and status filter
        const matchesSearch = name.toLowerCase().includes(searchTerm);
        const matchesStatus = statusFilter === 'all' || status === statusFilter;
        
        // Show or hide based on filters
        if (matchesSearch && matchesStatus) {
            card.classList.remove('hidden');
            visibleCount++;
            
            // Highlight search match if there's a search term
            if (searchTerm) {
                card.classList.add('highlight-search');
            } else {
                card.classList.remove('highlight-search');
            }
        } else {
            card.classList.add('hidden');
            card.classList.remove('highlight-search');
        }
    });
        
    // Show "no results" message if needed
    if (noResults) {
        if (visibleCount === 0) {
            noResults.classList.remove('hidden');
        } else {
            noResults.classList.add('hidden');
        }
    }
    
    // Re-sort visible cards
    const activeSortOption = document.querySelector('.sort-option.active');
    if (activeSortOption) {
        sortParticipants(activeSortOption.getAttribute('data-value'));
    }
}

// Sort participants based on selected criteria
function sortParticipants(sortBy) {
    const activeSortOption = document.querySelector('.sort-option.active');
    sortBy = sortBy || (activeSortOption ? activeSortOption.getAttribute('data-value') : 'date-desc');
    
    const container = document.querySelector('.registrants-container');
    
    if (!container) {
        console.error('Registrants container not found!');
        return;
    }
    
    
    const cards = Array.from(container.querySelectorAll('.registrant-card:not(.hidden)'));
    
    // Sort cards based on selected criteria
    cards.sort((a, b) => {
        switch (sortBy) {
            case 'name-asc':
                return (a.getAttribute('data-name') || '').localeCompare(b.getAttribute('data-name') || '');
            case 'name-desc':
                return (b.getAttribute('data-name') || '').localeCompare(a.getAttribute('data-name') || '');
            case 'date-asc':
                return (a.getAttribute('data-date') || '').localeCompare(b.getAttribute('data-date') || '');
            case 'date-desc':
                return (b.getAttribute('data-date') || '').localeCompare(a.getAttribute('data-date') || '');
            default:
                // Default to original order
                const aIndex = parseInt(a.getAttribute('data-original-index') || '0');
                const bIndex = parseInt(b.getAttribute('data-original-index') || '0');
                return aIndex - bIndex;
        }
    });
    
    // Reorder the DOM elements
    cards.forEach(card => {
        container.appendChild(card);
    });
}

// Toggle dropdown for participant details
function toggleDropdown(id) {
    const details = document.getElementById(id);
    if (!details) {
        console.error(`Details element with ID ${id} not found!`);
        return;
    }
    
    const header = details.previousElementSibling;
    const icon = header.querySelector('.dropdown-icon');
    
    if (details.style.display === 'block') {
        details.style.display = 'none';
        if (icon) icon.style.transform = 'rotate(0deg)';
    } else {
        details.style.display = 'block';
        if (icon) icon.style.transform = 'rotate(180deg)';
    }
}

// For the broadcast popup
function openBroadcastPopup() {
    const popup = document.querySelector('.broadcast-popup');
    if (popup) popup.style.display = 'flex';
}

function closeDialog() {
    const popup = document.querySelector('.broadcast-popup');
    if (popup) popup.style.display = 'none';
}

// Copy to clipboard function for sharing
function shareEvent(eventTitle, eventId) {
    const url = `${window.location.origin}/event/detail/${eventId}`;
    
    if (navigator.clipboard) {
        navigator.clipboard.writeText(url)
            .then(() => {
                const shareBtn = document.getElementById('share-btn-txt');
                if (shareBtn) {
                    const originalText = shareBtn.textContent;
                    shareBtn.textContent = 'Copied!';
                    
                    setTimeout(() => {
                        shareBtn.textContent = originalText;
                    }, 2000);
                }
            })
            .catch(err => {
                console.error('Could not copy text: ', err);
            });
    } else {
        // Fallback for browsers that don't support clipboard API
        const textArea = document.createElement('textarea');
        textArea.value = url;
        document.body.appendChild(textArea);
        textArea.select();
        document.execCommand('copy');
        document.body.removeChild(textArea);
        
        const shareBtn = document.getElementById('share-btn-txt');
        if (shareBtn) {
            const originalText = shareBtn.textContent;
            shareBtn.textContent = 'Copied!';
            
            setTimeout(() => {
                shareBtn.textContent = originalText;
            }, 2000);
        }
    }
}