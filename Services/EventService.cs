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
                    Answer = "answer answer"
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
        eventDetailViewModel.EventDetailCardData.CreatorDisplayName = creator.DisplayName;
        eventDetailViewModel.EventDetailCardData.CreatorProfileImageUrl = creator.ProfileImageUrl;

        var questions = await GetQuestionsByEventId(Event.EventId);
        eventDetailViewModel.EventFormDto.Questions = questions ?? [];

        return eventDetailViewModel;
    }
}
