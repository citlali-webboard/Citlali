using Supabase;
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

        Console.WriteLine(tags);
        return tags;   
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

        Console.WriteLine(model);

        Console.WriteLine("Before Insert--------------------------------");
        await _supabaseClient
            .From<Event>()
            .Insert(model);
        Console.WriteLine("After Insert---------------------------------");

        return model;
    }

    public async Task<List<EventBriefCardData>> GetEventsByUserId(Guid userId)
    {
        var response = await _supabaseClient
            .From<Event>()
            .Select("*")
            .Filter("CreatorUserId", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Get();

        var events = new List<EventBriefCardData>();

        if (response != null)
        {
            foreach (var item in response.Models)
            {
                events.Add(new EventBriefCardData
                {
                    EventId = item.EventId,
                    EventTitle = item.EventTitle,
                    CreatorDisplayName = "Some Creator", // Example placeholder
                    EventCategoryTag = new EventCategoryTag { EventCategoryTagName = "Category" },
                    LocationTag = new EventLocationTag { EventLocationTagName = "Location" },
                    CurrentParticipant = 10, // Example data
                    MaxParticipant = 50, // Example data
                    Cost = item.Cost,
                    EventDate = item.EventDate,
                    PostExpiryDate = item.PostExpiryDate,
                    CreatedAt = item.CreatedAt
                });
            }
        }

        return events;
    }

    
    }
