using System.Text.Json;
using Citlali.Controllers;
using Citlali.Models;
using Supabase.Postgrest;

namespace Citlali.Services;

public class SearchService(Supabase.Client supabaseClient, Configuration configuration, ILogger<SearchService> logger)
{
    private readonly Supabase.Client _supabaseClient = supabaseClient;
    private readonly Configuration _configuration = configuration;
    private readonly ILogger<SearchService> _logger = logger;

    public async Task<List<SearchResult>> QueryDisplayName(string query)
    {
        try
        {
            var databaseResult = await _supabaseClient
                .From<User>()
                .Select(x => new object[] { x.DisplayName, x.Username, x.UserBio, x.ProfileImageUrl, x.Deleted })
                .Filter(x => x.Deleted, Constants.Operator.Equals, "FALSE")
                .Filter(x => x.DisplayName , Constants.Operator.WFTS, new FullTextSearchConfig(query, null))
                .Get();

            if (databaseResult == null)
            {
                return [];
            }

            List<SearchResult> searchResults = databaseResult.Models.ConvertAll(model => new SearchResult
            {
                Url = $"/user/{model.Username}",
                Title = model.DisplayName,
                Description = model.UserBio,
                ImageUrl = model.ProfileImageUrl
            });

            return searchResults;
        }
        catch (Exception e)
        {
            var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
            string msgError = errorJson.GetProperty("msg").GetString() ?? "";
            _logger.LogError("Search error occurred: {ErrorMessage}", msgError);

            throw new Exception(msgError);
        }
    }

    public async Task<List<SearchResult>> QueryUsername(string query)
    {
        try
        {
            var databaseResult = await _supabaseClient
                .From<User>()
                .Select(x => new object[] { x.DisplayName, x.Username, x.UserBio, x.ProfileImageUrl, x.Deleted })
                .Filter(x => x.Deleted, Constants.Operator.Equals, "FALSE")
                .Filter(x => x.Username , Constants.Operator.WFTS, new FullTextSearchConfig(query, null))
                .Get();

            if (databaseResult == null)
            {
                return [];
            }

            List<SearchResult> searchResults = databaseResult.Models.ConvertAll(model => new SearchResult
            {
                Url = $"/user/{model.Username}",
                Title = model.DisplayName,
                Description = model.UserBio,
                ImageUrl = model.ProfileImageUrl
            });

            return searchResults;
        }
        catch (Exception e)
        {
            var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
            string msgError = errorJson.GetProperty("msg").GetString() ?? "";
            _logger.LogError("Search error occurred: {ErrorMessage}", msgError);

            throw new Exception(msgError);
        }
    }
    
    public async Task<List<SearchResult>> QueryEvent(string query)
    {
        try
        {
            var databaseResult = await _supabaseClient
                .From<Event>()
                .Select(x => new object[] { x.EventTitle, x.EventDescription, x.EventId, x.Deleted })
                .Filter(x => x.Deleted, Constants.Operator.Equals, "FALSE")
                .Filter(x => x.EventTitle, Constants.Operator.WFTS, new FullTextSearchConfig(query, null))
                .Get();

            if (databaseResult == null)
            {
                return [];
            }

            List<SearchResult> searchResults = databaseResult.Models.ConvertAll(model => new SearchResult
            {
                Url = $"/event/detail/{model.EventId}",
                Title = model.EventTitle,
                Description = model.EventDescription
            });

            return searchResults;
        }
        catch (Exception e)
        {
            var errorJson = JsonSerializer.Deserialize<JsonElement>(e.Message);
            string msgError = errorJson.GetProperty("msg").GetString() ?? "";
            _logger.LogError("Search error occurred: {ErrorMessage}", msgError);

            throw new Exception(msgError);
        }
    }
}