namespace Citlali.Models;

public class SearchQueryDto
{
    public string Query { get; set; } = "";
}

public class SearchResult
{
    public string Url { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string ImageUrl { get; set; } = "";
}

public class SearchResponse
{
    public List<SearchResult> Results { get; set; } = [];
}