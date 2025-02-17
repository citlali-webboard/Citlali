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

        List<EventQuestion> eventQuestions = createEventViewModel.Questions.ConvertAll(question => new EventQuestion
        {
            EventQuestionId = Guid.NewGuid(),
            EventId = eventId,
            Question = question,
        });

        await _supabaseClient
            .From<Event>()
            .Insert(modelEvent);

        await _supabaseClient
            .From<EventQuestion>()
            .Insert(eventQuestions);

        return modelEvent;
    }

    public async Task<List<Event>> GetEventsByUserId(Guid userId)
    {
        var response = await _supabaseClient
            .From<Event>()
            .Filter("CreatorUserId", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Order("CreatedAt", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();

        return response.Models;
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

        return response ?? null;
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
        var citlaliEvent = await GetEventById(id) ?? throw new Exception("Event not found");
        var tag = await GetTagById(citlaliEvent.EventCategoryTagId);
        var location = await GetLocationTagById(citlaliEvent.EventLocationTagId);
        var creator = await _userService.GetUserByUserId(citlaliEvent.CreatorUserId) ?? throw new Exception("Creator not found");

        var eventDetailCardData = new EventDetailCardData
        {
            EventId = citlaliEvent.EventId,
            EventTitle = citlaliEvent.EventTitle,
            EventDescription = citlaliEvent.EventDescription,
            EventCategoryTag = tag ?? new(),
            LocationTag = location ?? new(),
            MaxParticipant = citlaliEvent.MaxParticipant,
            Cost = citlaliEvent.Cost,
            EventDate = citlaliEvent.EventDate,
            PostExpiryDate = citlaliEvent.PostExpiryDate,
            CreatedAt = citlaliEvent.CreatedAt,
            CreatorUsername = creator.Username,
            CreatorDisplayName = creator.DisplayName,
            CreatorProfileImageUrl = creator.ProfileImageUrl
        };
        var eventFormDto = new EventFormDto
        {
            Questions = await GetQuestionsByEventId(citlaliEvent.EventId) ?? [],
            EventId = citlaliEvent.EventId
        };

        var eventDetailViewModel = new EventDetailViewModel
        {
            EventDetailCardData = eventDetailCardData,
            EventFormDto = eventFormDto
        };
        return eventDetailViewModel;
    }

    //JoinEvent
    public async Task<Registration> JoinEvent(JoinEventModel joinEventModel)
    {
        var supabaseUser = _supabaseClient.Auth.CurrentUser ?? throw new Exception("User not authenticated");
        Guid userId = Guid.Parse(supabaseUser.Id ?? "");
        Guid EventID = joinEventModel.EventId;
        var questionsList = joinEventModel.EventFormDto.Questions;

        var newRegistration = new Registration
        {
            RegistrationId = Guid.NewGuid(),
            EventId = EventID,
            UserId = userId,
        };

        await _supabaseClient
            .From<Registration>()
            .Insert(newRegistration);

        List<RegistrationAnswer> newRegistrationAnswers = questionsList.ConvertAll(question => new RegistrationAnswer
        {
            RegistrationAnswerId = Guid.NewGuid(),
            RegistrationId = newRegistration.RegistrationId,
            EventQuestionId = question.EventQuestionId,
            Answer = question.Answer
        });

        await _supabaseClient
            .From<RegistrationAnswer>()
            .Insert(newRegistrationAnswers);

        return newRegistration;
    }

    public async Task<List<Event>> GetAllEvents()
    {
        var response = await _supabaseClient
            .From<Event>()
            .Filter(row => row.Deleted, Supabase.Postgrest.Constants.Operator.Equals, "false")
            .Order("CreatedAt", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();

        return response.Models;
    }

    public async Task<int> GetEventsExactCount()
    {
        var count = await _supabaseClient
            .From<Event>()
            .Filter(row => row.Deleted, Supabase.Postgrest.Constants.Operator.Equals, "false")
            .Count(Supabase.Postgrest.Constants.CountType.Exact);

        return count;
    }

    public async Task<List<Event>> GetPaginatedEvents(int from, int to)
    {
        var response = await _supabaseClient
            .From<Event>()
            .Filter(row => row.Deleted, Supabase.Postgrest.Constants.Operator.Equals, "false")
            .Order("CreatedAt", Supabase.Postgrest.Constants.Ordering.Descending)
            .Range(from, to)
            .Get();

        return response.Models;
    }

    public async Task<EventBriefCardData> EventToBriefCard(Event citlaliEvent)
    {
        var creatorTask = _userService.GetUserByUserId(citlaliEvent.CreatorUserId);
        var locationTagTask = GetLocationTagById(citlaliEvent.EventLocationTagId);
        var categoryTagTask = GetTagById(citlaliEvent.EventCategoryTagId);

        await Task.WhenAll(creatorTask, locationTagTask, categoryTagTask);

        var creator = await creatorTask ?? throw new Exception("Creator not found");
        var locationTag = await locationTagTask ?? throw new Exception("Location not found");
        var categoryTag = await categoryTagTask ?? throw new Exception("Category not found");

        return new EventBriefCardData
            {
                EventId = citlaliEvent.EventId,
                EventTitle = citlaliEvent.EventTitle,
                EventDescription = citlaliEvent.EventDescription,
                CreatorDisplayName = creator.DisplayName,
                CreatorProfileImageUrl = creator.ProfileImageUrl,
                LocationTag = locationTag,
                EventCategoryTag = categoryTag,
                CurrentParticipant = 0,
                MaxParticipant = citlaliEvent.MaxParticipant,
                Cost = citlaliEvent.Cost,
                EventDate = citlaliEvent.EventDate,
                PostExpiryDate = citlaliEvent.PostExpiryDate,
                CreatedAt = citlaliEvent.CreatedAt,
            };
    }
    
    public async Task<EventBriefCardData[]> EventsToBriefCardArray(List<Event> citlaliEvents)
    {
        List<Task<EventBriefCardData>> briefCardDataTasks = citlaliEvents.ConvertAll(EventToBriefCard);
        var briefCardsData = await Task.WhenAll(briefCardDataTasks);
        return briefCardsData;
    }
}
