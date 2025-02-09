using Supabase;
using DotNetEnv;
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
            Questions = createEventViewModel.Questions
        };

        await _supabaseClient
            .From<Event>()
            .Insert(model);

        return model;
    }
    
    
    }
