using Supabase;
// using DotNetEnv;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Citlali.Models;
using Microsoft.AspNetCore.Mvc;

// using Supabase.Gotrue;

namespace Citlali.Services;

public class EventService(Client supabaseClient, IConfiguration configuration)
{
    private readonly Client _supabaseClient = supabaseClient;
    private readonly IConfiguration _configuration = configuration;

    // CreateEvent 

    public async Task<List<Tag>> GetTags()
    {
        var response = await _supabaseClient
            .From<EventCategoryTag>()
            .Select("*")
            .Get();  

        var tags = new List<Tag>();

        if (response != null){
            foreach (var tag in response.Models)
            {
                tags.Add(new Tag { TagId = tag.EventCategoryTagId,
                                   TagEmoji = tag.EventCategoryTagEmoji, 
                                   TagName = tag.EventCategoryTagName });
            }
        }

        return tags;   
    }

    public async Task<List<Location>> GetLocationTags()
    {
        var response = await _supabaseClient
            .From<LocationTag>()
            .Select("*")
            .Get();

        var locations = new List<Location>();
        
        if (response != null){
            foreach (var location in response.Models)
            {
                locations.Add(new Location { EventLocationTagId = location.EventLocationTagId,
                                             EventLocationTagName = location.EventLocationTagName });
            }
        }

        return locations;
    } 

    public async Task<Event> CreateEvent(CreateEventViewModel createEventViewModel)
    {
        var supabaseUser = _supabaseClient.Auth.CurrentUser;
        if (supabaseUser == null)
        {
            throw new Exception("User not authenticated");
        }

        Guid userId = Guid.Parse(supabaseUser.Id ?? "");

        var model = new Event
        {
            EventId =  Guid.NewGuid(),
            CreatorUserId = userId,
            EventTitle = createEventViewModel.EventTitle,
            EventDescription = createEventViewModel.EventDescription,
            EventCategoryTagId = createEventViewModel.EventCategoryTagId,
            EventLocationTagId = createEventViewModel.EventLocationTagId,
            MaxParticipant = createEventViewModel.MaxParticipant,
            Cost = createEventViewModel.Cost,
            EventDate = createEventViewModel.EventDate,
            PostExpiryDate = createEventViewModel.PostExpiryDate,
        };

        Console.WriteLine(model.EventLocationTagId);

        Console.WriteLine("Before Insert--------------------------------");
        await _supabaseClient
            .From<Event>()
            .Insert(model);
        Console.WriteLine("After Insert---------------------------------");

        return model;
    }

    
    }
