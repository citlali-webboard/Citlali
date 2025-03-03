using Citlali.Models;
using Supabase;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Citlali.Services;


public class AdminService(Client supabaseClient, Configuration configuration, EventService eventService)
{
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
}