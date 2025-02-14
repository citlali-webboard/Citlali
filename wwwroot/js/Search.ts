const apiUrl = `${window.location.origin}/search/query`;

const searchForm = document.querySelector<HTMLFormElement>("#search-form");
const resultsNull = document.querySelector<HTMLElement>("#results-null");
const resultsLoading = document.querySelector<HTMLElement>("#results-loading");
const results = document.querySelector<HTMLElement>("#results");

interface SearchResult {
  url: string;
  title: string;
  description: string;
  imageUrl?: string;
}

interface SearchResponse {
  results: SearchResult[];
}

function toggleResultsNull() {
  if (resultsNull) resultsNull.style.display = "flex";
  if (resultsLoading) resultsLoading.style.display = "none";
  if (results) results.style.display = "none";
}

function toggleResultsLoading() {
  if (resultsNull) resultsNull.style.display = "none";
  if (resultsLoading) resultsLoading.style.display = "flex";
  if (results) results.style.display = "none";
}

function toggleResults() {
  if (resultsNull) resultsNull.style.display = "none";
  if (resultsLoading) resultsLoading.style.display = "none";
  if (results) results.style.display = "flex";
}

function getResultCard(searchResult: SearchResult) {
  const card = document.createElement('a');
  card.href = searchResult.url;
  
  const listItem = document.createElement('li');
  listItem.className = 'search-result';
  card.appendChild(listItem);

  if (searchResult.imageUrl)  {
    const imageDiv = document.createElement('div');
    imageDiv.className = 'search-result-image';
    listItem.appendChild(imageDiv);

    const img = document.createElement('img');
    img.src = searchResult.imageUrl;
    img.alt = `Profile picture of ${searchResult.title}`;
    imageDiv.appendChild(img);
  }
  
  const textDiv = document.createElement('div');
  textDiv.className = 'search-result-text';
  listItem.appendChild(textDiv);
  
  const title = document.createElement('h3');
  title.textContent = searchResult.title;
  textDiv.appendChild(title);
  
  const description = document.createElement('p');
  description.textContent = searchResult.description;
  textDiv.appendChild(description);

  return card;
}

if (searchForm) {
  searchForm.addEventListener("submit", async (event) => {
    event.preventDefault();

    const formData = new FormData(searchForm);
    const query = formData.get("query") as string;

    if (!query) {
      toggleResultsNull();
      return;
    }
    toggleResultsLoading();

    const params = new URLSearchParams({
      query: query,
    });

    const response = await fetch(`${apiUrl}?${params.toString()}`)
      .then((response) => response.json())
      .then((data: SearchResponse) => data)
      .catch((error) => {
        toggleResultsNull();
        console.error(`Error querying search ${error}`);
        return { results: [] };
      });
    
      const cards = response.results.map((result: SearchResult) => getResultCard(result))
      if (results && cards) {
        results.innerHTML = '';
        cards.forEach(card => results.appendChild(card));
      } else {
        toggleResultsNull();
      }
      toggleResults();
  });
}
