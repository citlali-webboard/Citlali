var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g = Object.create((typeof Iterator === "function" ? Iterator : Object).prototype);
    return g.next = verb(0), g["throw"] = verb(1), g["return"] = verb(2), typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (g && (g = 0, op[0] && (_ = 0)), _) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var _this = this;
var apiUrl = "".concat(window.location.origin, "/search/query");
var searchForm = document.querySelector("#search-form");
var resultsNull = document.querySelector("#results-null");
var resultsLoading = document.querySelector("#results-loading");
var results = document.querySelector("#results");
function toggleResultsNull() {
    if (resultsNull)
        resultsNull.style.display = "flex";
    if (resultsLoading)
        resultsLoading.style.display = "none";
    if (results)
        results.style.display = "none";
}
function toggleResultsLoading() {
    if (resultsNull)
        resultsNull.style.display = "none";
    if (resultsLoading)
        resultsLoading.style.display = "flex";
    if (results)
        results.style.display = "none";
}
function toggleResults() {
    if (resultsNull)
        resultsNull.style.display = "none";
    if (resultsLoading)
        resultsLoading.style.display = "none";
    if (results)
        results.style.display = "flex";
}
function getResultCard(searchResult) {
    var card = document.createElement('a');
    card.href = searchResult.url;
    var listItem = document.createElement('li');
    listItem.className = 'search-result';
    card.appendChild(listItem);
    var imageDiv = document.createElement('div');
    imageDiv.className = 'search-result-image';
    listItem.appendChild(imageDiv);
    if (searchResult.imageUrl) {
        var img = document.createElement('img');
        img.src = searchResult.imageUrl;
        img.alt = "Profile picture of ".concat(searchResult.title);
        imageDiv.appendChild(img);
    }
    var textDiv = document.createElement('div');
    textDiv.className = 'search-result-text';
    listItem.appendChild(textDiv);
    var title = document.createElement('h3');
    title.textContent = searchResult.title;
    textDiv.appendChild(title);
    var description = document.createElement('p');
    description.textContent = searchResult.description;
    textDiv.appendChild(description);
    return card;
}
if (searchForm) {
    searchForm.addEventListener("submit", function (event) { return __awaiter(_this, void 0, void 0, function () {
        var formData, searchType, query, params, response, cards;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    event.preventDefault();
                    formData = new FormData(searchForm);
                    searchType = formData.get("type");
                    query = formData.get("query");
                    if (!searchType || !query) {
                        toggleResultsNull();
                        return [2 /*return*/];
                    }
                    toggleResultsLoading();
                    params = new URLSearchParams({
                        type: searchType,
                        query: query,
                    });
                    return [4 /*yield*/, fetch("".concat(apiUrl, "?").concat(params.toString()))
                            .then(function (response) { return response.json(); })
                            .then(function (data) { return data; })
                            .catch(function (error) {
                            toggleResultsNull();
                            console.error("Error querying search ".concat(error));
                            return { results: [] };
                        })];
                case 1:
                    response = _a.sent();
                    cards = response.results.map(function (result) { return getResultCard(result); });
                    if (results && cards) {
                        results.innerHTML = '';
                        cards.forEach(function (card) { return results.appendChild(card); });
                    }
                    else {
                        toggleResultsNull();
                    }
                    toggleResults();
                    return [2 /*return*/];
            }
        });
    }); });
}
