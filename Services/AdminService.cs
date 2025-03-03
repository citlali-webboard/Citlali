using Citlali.Models;
using Supabase;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Citlali.Services;


public class AdminService(Client supabaseClient, Configuration configuration)
{
    private readonly Client _supabaseClient = supabaseClient;
    private readonly Configuration _configuration = configuration;
    public Supabase.Gotrue.Session CurrentSession { get;set; } = new Supabase.Gotrue.Session();

}