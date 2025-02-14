
const searchForm = document.querySelector<HTMLFormElement>("#search-form");

if (searchForm) {
    searchForm.addEventListener("submit", function(event){
        event.preventDefault();
    });
}