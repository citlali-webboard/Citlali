using Citlali.Models;
using Supabase;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Citlali.Services;


public class AdminService(ILogger<AdminService> logger,Client supabaseClient, Configuration configuration, EventService eventService)
{
    private readonly ILogger<AdminService> _logger = logger;
    private readonly Client _supabaseClient = supabaseClient;
    private readonly Configuration _configuration = configuration;
    private readonly EventService _eventService = eventService;
    public Supabase.Gotrue.Session CurrentSession { get;set; } = new Supabase.Gotrue.Session();


    public async Task<AdminCategoriesViewModel> GetCategoryListViewModel()
    {
        var viewModel = new AdminCategoriesViewModel();

        var tags = await _eventService.GetTags();
        viewModel.Tags = tags;

        return viewModel;
    }

    public async Task CategoryCreate(EventCategoryTag eventCategoryTag) {
        await _supabaseClient
            .From<EventCategoryTag>()
            .Insert(eventCategoryTag);
    }

    public async Task CategorySoftDelete(Guid eventCategoryTagId) {
        await _supabaseClient
            .From<EventCategoryTag>()
            .Where(x => x.EventCategoryTagId == eventCategoryTagId)
            .Set(x => x.Deleted, true)
            .Update();
    }

    public async Task<AdminLocationsViewModel> GetLocationListViewModel()
    {
        var viewModel = new AdminLocationsViewModel();

        var locations = await _eventService.GetLocationTags();
        viewModel.Locations = locations;

        return viewModel;
    }

    public async Task LocationCreate(LocationTag locationTag) {
        await _supabaseClient
            .From<LocationTag>()
            .Insert(locationTag);
    }

    public async Task LocationSoftDelete(Guid locationTagId) {
        await _supabaseClient
            .From<LocationTag>()
            .Where(x => x.LocationTagId == locationTagId)
            .Set(x => x.Deleted, true)
            .Update();
    }
}