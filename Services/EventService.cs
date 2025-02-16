using Supabase;
using Citlali.Models;

// using Supabase.Gotrue;

namespace Citlali.Services;

public class EventService(Client supabaseClient, UserService userService)
{
    private readonly Client _supabaseClient = supabaseClient;
    private readonly UserService _userService = userService;
    // CreateEvent 

    public async Task<List<Tag>> GetTags()
    {
        var response = await _supabaseClient
            .From<EventCategoryTag>()
            .Select("*")
            .Get();

        var tags = new List<Tag>();

        if (response != null)
        {
            foreach (var tag in response.Models)
            {
                tags.Add(new Tag
                {
                    TagId = tag.EventCategoryTagId,
                    TagEmoji = tag.EventCategoryTagEmoji,
                    TagName = tag.EventCategoryTagName
                });
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

        if (response != null)
        {
            foreach (var location in response.Models)
            {
                locations.Add(new Location
                {
                    EventLocationTagId = location.LocationTagId,
                    EventLocationTagName = location.LocationTagName
                });
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

        Guid eventId = Guid.NewGuid();
        var modelEvent = new Event
        {
            EventId = eventId,
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

        List<string> questionsList = createEventViewModel.Questions;

        List<EventQuestion> eventQuestions = new();
        foreach (var question in questionsList)
        {
            eventQuestions.Add(new EventQuestion
            {
                EventQuestionId = Guid.NewGuid(),
                EventId = eventId,
                Question = question,
            });
        }

        await _supabaseClient
            .From<Event>()
            .Insert(modelEvent);

        foreach (var question in eventQuestions)
        {
            await _supabaseClient
                .From<EventQuestion>()
                .Insert(question);
        }

        return modelEvent;
    }

    public async Task<List<Event>> GetEventsByUserId(Guid userId)
    {
        var response = await _supabaseClient
            .From<Event>()
            .Select("*")
            .Filter("CreatorUserId", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Order("CreatedAt", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();

        var events = new List<Event>();

        if (response != null)
        {
            foreach (var e in response.Models)
            {
                var creator = await _userService.GetUserByUserId(e.CreatorUserId);
                var categoryTag = await GetTagById(e.EventCategoryTagId);

                events.Add(new Event
                {
                    EventId = e.EventId,
                    CreatorUserId = e.CreatorUserId,
                    EventTitle = e.EventTitle,
                    EventDescription = e.EventDescription,
                    EventCategoryTagId = e.EventCategoryTagId,
                    EventLocationTagId = e.EventLocationTagId,
                    MaxParticipant = e.MaxParticipant,
                    Cost = e.Cost,
                    EventDate = e.EventDate,
                    PostExpiryDate = e.PostExpiryDate,
                    CreatedAt = e.CreatedAt,
                    Deleted = e.Deleted,
                    CreatorProfileImageUrl = creator?.ProfileImageUrl ?? "",
                    CreatorDisplayName = creator?.DisplayName ?? "",
                    CurrentParticipant = e.CurrentParticipant,
                    EventCategoryTag = categoryTag ?? new EventCategoryTag()
                });
            }
        }
        return events;
    }

    
    // GetEventDetail
    public async Task<Event?> GetEventById(Guid eventId)
    {
        var response = await _supabaseClient
            .From<Event>()
            .Where(row => row.EventId == eventId)
            .Single();

        return response ?? null;
    }

    public async Task<EventCategoryTag?> GetTagById(Guid tagId)
    {
        var response = await _supabaseClient
            .From<EventCategoryTag>()
            .Where(row => row.EventCategoryTagId == tagId)
            .Single();

        return response ?? null;
    }

    public async Task<LocationTag?> GetLocationTagById(Guid locationTagId)
    {
        var response = await _supabaseClient
            .From<LocationTag>()
            .Where(row => row.LocationTagId == locationTagId)
            .Single();

        if (response != null)
        {

            return new LocationTag
            {
                LocationTagId = response.LocationTagId,
                LocationTagName = response.LocationTagName
            };
        }
       
        return null;
    }

    //get question by event id
    public async Task<List<QuestionViewModel>> GetQuestionsByEventId(Guid eventId)
    {
        var response = await _supabaseClient
            .From<EventQuestion>()
            .Where(row => row.EventId == eventId)
            .Get();

        var questions = new List<QuestionViewModel>();

        if (response != null)
        {
            foreach (var question in response.Models)
            {
                questions.Add(new QuestionViewModel
                {
                    EventQuestionId = question.EventQuestionId,
                    Question = question.Question,
                    Answer = ""
                });
            }
        }

        return questions;
    }

    public async Task<EventDetailViewModel> GetEventDetail(Guid id)
    {
        var Event = await GetEventById(id);
        if (Event == null)
        {
            throw new Exception("Event not found");
        }
        var tag = await GetTagById(Event.EventCategoryTagId);
        var location = await GetLocationTagById(Event.EventLocationTagId);
        var creator = await _userService.GetUserByUserId(Event.CreatorUserId) ?? throw new Exception("Creator not found");

        var eventDetailViewModel = new EventDetailViewModel();

        eventDetailViewModel.EventDetailCardData.EventId = Event.EventId;
        eventDetailViewModel.EventDetailCardData.EventTitle = Event.EventTitle;
        eventDetailViewModel.EventDetailCardData.EventDescription = Event.EventDescription;
        eventDetailViewModel.EventDetailCardData.EventCategoryTag = tag ?? new();
        eventDetailViewModel.EventDetailCardData.LocationTag = location ?? new();
        eventDetailViewModel.EventDetailCardData.MaxParticipant = Event.MaxParticipant;
        eventDetailViewModel.EventDetailCardData.Cost = Event.Cost;
        eventDetailViewModel.EventDetailCardData.EventDate = Event.EventDate;
        eventDetailViewModel.EventDetailCardData.PostExpiryDate = Event.PostExpiryDate;
        eventDetailViewModel.EventDetailCardData.CreatedAt = Event.CreatedAt;
        eventDetailViewModel.EventDetailCardData.CreatorUsername = creator.Username;
        eventDetailViewModel.EventDetailCardData.CreatorDisplayName = creator.DisplayName;
        eventDetailViewModel.EventDetailCardData.CreatorProfileImageUrl = creator.ProfileImageUrl;

        var questions = await GetQuestionsByEventId(Event.EventId);

        eventDetailViewModel.EventFormDto.Questions = questions ?? [];
        eventDetailViewModel.EventFormDto.EventId = Event.EventId;

        return eventDetailViewModel;
    }

    //JoinEvent
    public async Task<Registrantion> JoinEvent(JoinEventModel joinEventModel)
    {
        var supabaseUser = _supabaseClient.Auth.CurrentUser;

        if (supabaseUser == null)
        {
            throw new Exception("User not authenticated");
        }

        Guid userId = Guid.Parse(supabaseUser.Id ?? ""); 
        Guid EventID = joinEventModel.EventId;
        var QuestionsList = joinEventModel.EventFormDto.Questions;

        var newRegistration = new Registrantion
        {
            RegistrationId = Guid.NewGuid(),
            EventId = EventID,
            UserId = userId,
        };

        await _supabaseClient
            .From<Registrantion>()
            .Insert(newRegistration);

        Console.WriteLine("Registration created");

        foreach (var question in QuestionsList)
        {
            var newRegistrationAnswer = new RegistrationAnswer
            {
                RegistrationAnswerId = Guid.NewGuid(),
                RegistrationId = newRegistration.RegistrationId,
                EventQuestionId = question.EventQuestionId,
                Answer = question.Answer
            };

            await _supabaseClient
                .From<RegistrationAnswer>()
                .Insert(newRegistrationAnswer);
        }

        Console.WriteLine("Registration answers created");

        return newRegistration;
    }
    
    public async Task<List<Event>> GetAllEvents()
    {
        var response = await _supabaseClient
            .From<Event>()
            .Select("*")
            .Filter(row => row.Deleted, Supabase.Postgrest.Constants.Operator.Equals, "false")
            .Order("CreatedAt", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();

        var events = new List<Event>();

        if (response != null)
        {
            foreach (var e in response.Models)
            {
                events.Add(new Event
                {
                    EventId = e.EventId,
                    CreatorUserId = e.CreatorUserId,
                    EventTitle = e.EventTitle,
                    EventDescription = e.EventDescription,
                    EventCategoryTagId = e.EventCategoryTagId,
                    EventLocationTagId = e.EventLocationTagId,
                    MaxParticipant = e.MaxParticipant,
                    Cost = e.Cost,
                    EventDate = e.EventDate,
                    PostExpiryDate = e.PostExpiryDate,
                    CreatedAt = e.CreatedAt,
                    Deleted = e.Deleted
                });
            }
        }

        return events;
    }

    //GetEventsByTagId
    public async Task<List<Event>> GetEventsByTagId(Guid tagId)
    {
        var response = await _supabaseClient
            .From<Event>()
            .Select("*")
            .Filter("EventCategoryTagId", Supabase.Postgrest.Constants.Operator.Equals, tagId.ToString())
            .Order("CreatedAt", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();

        var events = new List<Event>();

        if (response != null)
        {
            foreach (var e in response.Models)
            {
                events.Add(new Event
                {
                    EventId = e.EventId,
                    CreatorUserId = e.CreatorUserId,
                    EventTitle = e.EventTitle,
                    EventDescription = e.EventDescription,
                    EventCategoryTagId = e.EventCategoryTagId,
                    EventLocationTagId = e.EventLocationTagId,
                    MaxParticipant = e.MaxParticipant,
                    Cost = e.Cost,
                    EventDate = e.EventDate,
                    PostExpiryDate = e.PostExpiryDate,
                    CreatedAt = e.CreatedAt,
                    Deleted = e.Deleted
                });
            }
        }

        return events;
    }
}
