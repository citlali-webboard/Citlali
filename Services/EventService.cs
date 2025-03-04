using Supabase;
using Citlali.Models;
using Microsoft.AspNetCore.Http.HttpResults;

// using Supabase.Gotrue;

namespace Citlali.Services;

public class EventService(Client supabaseClient, UserService userService, NotificationService notificationService, MailService mailService, Configuration configuration)
{
    private readonly Client _supabaseClient = supabaseClient;
    private readonly UserService _userService = userService;
    private readonly NotificationService _notificationService = notificationService;
    private readonly MailService _mailService = mailService;
    private readonly Configuration _configuration = configuration;
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
        var supabaseUser = _userService.CurrentSession.User ?? throw new Exception("User not authenticated");
        Guid userId = Guid.Parse(supabaseUser.Id ?? throw new Exception("User ID not found"));

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
            FirstComeFirstServed = createEventViewModel.FirstComeFirstServed,
        };
        await _supabaseClient
            .From<Event>()
            .Insert(modelEvent);

        if (createEventViewModel.Questions.Count > 0)
        {
            List<EventQuestion> eventQuestions = createEventViewModel.Questions.ConvertAll(question => new EventQuestion
            {
                EventQuestionId = Guid.NewGuid(),
                EventId = eventId,
                Question = question,
            });

            await _supabaseClient
                .From<EventQuestion>()
                .Insert(eventQuestions);
        }

        return modelEvent;
    }

    public async Task<bool> EditEvent(Guid eventId, CreateEventViewModel createEventViewModel)
    {
        var supabaseUser = _userService.CurrentSession.User ?? throw new UnauthorizedAccessException("User not authenticated");
        Guid userId = Guid.Parse(supabaseUser.Id ?? throw new UnauthorizedAccessException("User ID not found"));

        var eventToEdit = await GetEventById(eventId) ?? throw new KeyNotFoundException("Event not found");

        if (eventToEdit.CreatorUserId != userId)
            throw new UnauthorizedAccessException("User not authorized to edit this event");

        var currentParticipant = await GetRegistrationCountByEventId(eventId);
        if (createEventViewModel.MaxParticipant < currentParticipant)
            throw new MaximumParticipantExceedException();

        await _supabaseClient
            .From<Event>()
            .Where(row => row.EventId == eventId)
            .Set(row => row.EventTitle, createEventViewModel.EventTitle)
            .Set(row => row.EventDescription, createEventViewModel.EventDescription)
            .Set(row => row.EventCategoryTagId, createEventViewModel.EventCategoryTagId)
            .Set(row => row.EventLocationTagId, createEventViewModel.EventLocationTagId)
            .Set(row => row.MaxParticipant, createEventViewModel.MaxParticipant)
            .Set(row => row.Cost, createEventViewModel.Cost)
            .Set(row => row.EventDate, createEventViewModel.EventDate)
            .Set(row => row.PostExpiryDate, createEventViewModel.PostExpiryDate)
            .Update();

        return true;
    }

    public async Task<bool> CloseEventWhenMaxParticipant(Guid eventId)
    {
        var eventToClose = await GetEventById(eventId) ?? throw new KeyNotFoundException("Event not found");

        await _supabaseClient
            .From<Event>()
            .Where(row => row.EventId == eventId)
            .Set(row => row.Status, "closed")
            .Update();

        return true;

    }

    public async Task<bool> DeleteEvent(Guid eventId)
    {
        var supabaseUser = _userService.CurrentSession.User;
        if (supabaseUser == null)
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var eventToDelete = await GetEventById(eventId);
        if (eventToDelete == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        if (eventToDelete.CreatorUserId.ToString() != supabaseUser.Id)
        {
            throw new UnauthorizedAccessException("User not authorized to delete this event");
        }

        await _supabaseClient
            .From<Event>()
            .Where(row => row.EventId == eventId)
            .Set(row => row.Deleted, true)
            .Update();

        return true;
    }

    public async Task<bool> SetEventStatus(Guid eventId, string status)
    {
        var supabaseUser = _userService.CurrentSession.User
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var eventToArchive = await GetEventById(eventId)
            ?? throw new KeyNotFoundException("Event not found");

        if (eventToArchive.CreatorUserId.ToString() != supabaseUser.Id)
        {
            throw new UnauthorizedAccessException("User not authorized to archive this event");
        }

        await _supabaseClient
            .From<Event>()
            .Where(row => row.EventId == eventId)
            .Set(row => row.Status, status)
            .Update();

        return true;
    }

    public async Task<bool> ArchiveEvent(Guid eventId)
    {
        return await SetEventStatus(eventId, "archived");
    }

    public async Task<bool> OpenEvent(Guid eventId)
    {
        return await SetEventStatus(eventId, "active");
    }

    public async Task<bool> CloseEvent(Guid eventId)
    {
        return await SetEventStatus(eventId, "closed");
    }

    public async Task<Guid> GetCreatorEventIdByEventId(Guid eventId)
    {
        var response = await _supabaseClient
            .From<Event>()
            .Select("CreatorUserId")
            .Filter("EventId", Supabase.Postgrest.Constants.Operator.Equals, eventId.ToString())
            .Single();

        if (response == null)
        {
            throw new KeyNotFoundException("Event not found");
        }

        return response.CreatorUserId;
    }

    public async Task<bool> InviteUser(Guid eventId, Guid userId)
    {
        var supabaseUser = _userService.CurrentSession.User
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var eventToInviteTask = GetEventById(eventId);
        var registrationTask = GetRegistrationByEventIdAndUserId(eventId, userId);
        var targetUserTask = _userService.GetUserByUserId(userId);
        var invitedRegistrantCountTask = _supabaseClient
            .From<Registration>()
            .Filter("EventId", Supabase.Postgrest.Constants.Operator.Equals, eventId.ToString())
            .Filter("Status", Supabase.Postgrest.Constants.Operator.In, new[] { "awaiting-confirmation", "confirmed" })
            .Count(Supabase.Postgrest.Constants.CountType.Exact);

        var eventToInvite = await eventToInviteTask ?? throw new KeyNotFoundException("Event not found");
        if (eventToInvite.CreatorUserId.ToString() != supabaseUser.Id)
        {
            throw new UnauthorizedAccessException("User not authorized to invite to this event");
        }
        var invitedRegistrantCount = await invitedRegistrantCountTask;

        if (invitedRegistrantCount >= eventToInvite.MaxParticipant)
        {
            throw new MaximumInvitationExceedException();
        }

        var registration = await registrationTask ?? throw new KeyNotFoundException("Registration not found");

        await _supabaseClient
            .From<Registration>()
            .Where(row => row.RegistrationId == registration.RegistrationId)
            .Set(row => row.Status, "awaiting-confirmation")
            .Set(row => row.UpdatedAt, DateTime.UtcNow)
            .Update();

        var notificationTitle = "You have been invited to an event! 🎉";
        var notificationBody = $"Congratulations! Your request to join the event {eventToInvite.EventTitle} has been reviewed and accepted! To confirm or reject the invitation, please visit the event page.";
        var absoluteUrl = $"/event/detail/{eventId}";
        var mailModel = new MailNotificationViewModel
        {
            Title = notificationTitle,
            Body = notificationBody,
            Url = $"{_configuration.App.Url}{absoluteUrl}"
        };

        var targetUser = await targetUserTask ?? throw new KeyNotFoundException("Can't query target user");
        var notificaionTask = _notificationService.CreateNotification(userId, notificationTitle, notificationBody, absoluteUrl);
        _mailService.SendNotificationEmail(mailModel, targetUser.Email);

        return true;
    }

    public async Task<bool> RejectUser(Guid eventId, Guid userId)
    {
        var supabaseUser = _userService.CurrentSession.User
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var eventToInvite = await GetEventById(eventId)
            ?? throw new KeyNotFoundException("Event not found");

        if (eventToInvite.CreatorUserId.ToString() != supabaseUser.Id)
        {
            throw new UnauthorizedAccessException("User not authorized to reject to this event");
        }

        var registration = await GetRegistrationByEventIdAndUserId(eventId, userId)
            ?? throw new KeyNotFoundException("Registration not found");

        await _supabaseClient
            .From<Registration>()
            .Where(row => row.RegistrationId == registration.RegistrationId)
            .Set(row => row.Status, "rejected")
            .Set(row => row.UpdatedAt, DateTime.UtcNow)
            .Update();

        var notificationTitle = "Your request has been rejected. ❌";
        var notificationBody = $"We regret to inform you that your request to join the event {eventToInvite.EventTitle} has been rejected. We appreciate your understanding, but you can always try again!";

        await _notificationService.CreateNotification(userId, notificationTitle, notificationBody, $"/event/detail/{eventId}");

        return true;
    }

    public async Task<List<Event>> GetEventsByUserId(Guid userId)
    {
        var response = await _supabaseClient
            .From<Event>()
            .Filter("CreatorUserId", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Filter("Deleted", Supabase.Postgrest.Constants.Operator.Equals, "false")
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

    public async Task<EventDetailViewModel> GetEventDetailPage(Guid eventId)
    {
        var currentUser = _userService.CurrentSession.User;
        if (currentUser == null)
            return await GetEventDetail(eventId, null);

        var userId = Guid.Parse(currentUser.Id ?? "");

        return await GetEventDetail(eventId, userId);
    }


    public async Task<EventDetailViewModel> GetEventDetail(Guid id, Guid? userId)
    {
        var citlaliEvent = await GetEventById(id) ?? throw new Exception("Event not found");
        var tag = await GetTagById(citlaliEvent.EventCategoryTagId);
        var location = await GetLocationTagById(citlaliEvent.EventLocationTagId);
        var creator = await _userService.GetUserByUserId(citlaliEvent.CreatorUserId) ?? throw new Exception("Creator not found");

        bool isOwner = userId.HasValue && userId.Value == citlaliEvent.CreatorUserId;
        bool isRegistered = userId.HasValue && await IsUserRegistered(id, userId.Value);
        bool isClosed = citlaliEvent.Status == "closed";

        if (isOwner)
        {
            throw new JoinOwnerException(); // Redirect to "manage" page
        }
        if (isRegistered)
        {
            throw new UserAlreadyRegisteredException(); // Redirect to "status" page
        }
        if (isRegistered && isClosed)
            throw new UserAlreadyRegisteredException();

        var currentParticipant = await GetRegistrationCountByEventId(citlaliEvent.EventId);

        var eventDetailCardData = new EventDetailCardData
        {
            EventId = citlaliEvent.EventId,
            EventTitle = citlaliEvent.EventTitle,
            EventDescription = citlaliEvent.EventDescription,
            EventCategoryTag = tag ?? new(),
            LocationTag = location ?? new(),
            CurrentParticipant = currentParticipant,
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

        return new EventDetailViewModel
        {
            IsClosed = isClosed,
            IsUserRegistered = isRegistered,
            EventDetailCardData = eventDetailCardData,
            EventFormDto = eventFormDto
        };
    }

    //JoinEvent
    public async Task<Registration> JoinEvent(JoinEventModel joinEventModel)
    {
        var supabaseUser = _userService.CurrentSession.User ?? throw new Exception("User not authenticated");
        Guid userId = Guid.Parse(supabaseUser.Id ?? throw new Exception("User ID not found"));
        Guid EventId = joinEventModel.EventId;
        var citlaliEvent = await GetEventById(EventId) ?? throw new EventNotFoundException();
        if (citlaliEvent.Status == "closed")
            throw new EventClosedException();

        var eventQuestionsTask = _supabaseClient
            .From<EventQuestion>()
            .Where(x => x.EventId == joinEventModel.EventId)
            .Get();

        if (await IsUserRegistered(EventId, userId))
        {
            throw new UserAlreadyRegisteredException();
        }

        var Event = await GetEventById(EventId) ?? throw new KeyNotFoundException("Event not found");

        if (userId == Event.CreatorUserId)
        {
            throw new JoinOwnerException();
        }

        var newRegistration = new Registration
        {
            RegistrationId = Guid.NewGuid(),
            EventId = EventId,
            UserId = userId,
        };

        await _supabaseClient
            .From<Registration>()
            .Insert(newRegistration);

        var eventQuestions = await eventQuestionsTask;
        if (eventQuestions.Models.Count > 0)
        {
            var QuestionsList = joinEventModel.EventFormDto.Questions
                .Where(qvm => eventQuestions.Models.Any(eq => eq.EventQuestionId == qvm.EventQuestionId))
                .ToList();

            List<RegistrationAnswer> newRegistrationAnswers = QuestionsList.ConvertAll(question => new RegistrationAnswer
            {
                RegistrationAnswerId = Guid.NewGuid(),
                RegistrationId = newRegistration.RegistrationId,
                EventQuestionId = question.EventQuestionId,
                Answer = question.Answer
            });

            await _supabaseClient
                .From<RegistrationAnswer>()
                .Insert(newRegistrationAnswers);
        }

        var notificationTitle = "New joining request! 🙋🏻";
        var notificationBody = $"A user has requested to join your event {Event.EventTitle}. Please review their request on event management page.";

        await _notificationService.CreateNotification(Event.CreatorUserId, notificationTitle, notificationBody, $"/event/detail/{EventId}");

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
            .Select("*")
            .Where(row => row.Deleted == false)
            .Where(row => row.PostExpiryDate > DateTime.Now)
            .Where(row => row.Status == "active")
            .Count(Supabase.Postgrest.Constants.CountType.Exact);

        return count;
    }

    public async Task<List<Event>> GetPaginatedEvents(int start, int end, string sortBy = "newest")
    {
        var query = _supabaseClient
            .From<Event>()
            .Select("*")
            .Where(row => row.Deleted == false)
            .Where(row => row.PostExpiryDate > DateTime.Now)
            .Where(row => row.Status == "active");

        // Apply different sorting based on the sortBy parameter
        switch (sortBy)
        {
            case "date":
                query = query.Order("EventDate", Supabase.Postgrest.Constants.Ordering.Ascending);
                break;

            case "newest":
            default:
                query = query.Order("CreatedAt", Supabase.Postgrest.Constants.Ordering.Descending);
                break;

            case "popularity":
                // Get all events with their IDs to sort by popularity
                var events = await _supabaseClient
                    .From<Event>()
                    .Select("*")
                    .Where(row => row.Deleted == false)
                    .Where(row => row.PostExpiryDate > DateTime.Now)
                    .Where(row => row.Status == "active")
                    .Get();

                // Create a dictionary to store events with their participant counts
                var eventWithParticipants = new List<(Event Event, int Count)>();

                // Get participant count for each event concurrently
                var tasks = events.Models.Select(async e =>
                {
                    var count = await GetRegistrationCountByEventId(e.EventId);
                    return (e, count);
                });
                eventWithParticipants = (await Task.WhenAll(tasks)).ToList();

                // Sort events by participant count and take the requested range
                var sortedEvents = eventWithParticipants
                    .OrderByDescending(x => x.Count)
                    .Select(x => x.Event)
                    .Skip(start)
                    .Take(end - start + 1)
                    .ToList();

                return sortedEvents;
        }

        var response = await query.Get();

        int count = response.Models.Count;
        int actualEnd = Math.Min(end, count - 1);

        if (start >= count || start > actualEnd)
            return [];

        return [.. response.Models.Skip(start).Take(actualEnd - start + 1)];
    }

    // public async Task<List<Event>> GetPaginatedEventsDumb(int from, int to, string sortBy)
    // {
    //     var response = await _supabaseClient
    //         .From<Event>()
    //         .Select("*")
    //         .Where(row => row.Deleted == false)
    //         .Where(row => row.PostExpiryDate > DateTime.Now)
    //         .Where(row => row.Status == "active")
    //         .Order(row => row.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
    //         .Get();

    //     int count = response.Models.Count;
    //     int actualTo = Math.Min(to, count - 1);

    //     if (from >= count || from > actualTo)
    //         return [];

    //     return [.. response.Models.Skip(from).Take(actualTo - from + 1)];
    // }

    public async Task<List<Event>> GetPaginatedEventsSlow(int from, int to)
    {
        var models = await _supabaseClient
            .From<Event>()
            .Select(row => new object[] { row.EventId })
            .Where(row => row.Deleted == false)
            .Where(row => row.PostExpiryDate > DateTime.Now)
            .Where(row => row.Status == "active")
            .Order(row => row.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();

        int count = models.Models.Count;
        int actualTo = Math.Min(to, count - 1);

        if (from >= count || from > actualTo)
            return [];

        var idModelsToFetch = models.Models.Skip(from).Take(actualTo - from + 1);
        var idsToFetch = models.Models.ConvertAll(x => x.EventId.ToString());

        var events = await _supabaseClient
            .From<Event>()
            .Select("*")
            .Filter(x => x.EventId, Supabase.Postgrest.Constants.Operator.In, idsToFetch)
            .Get();
        return events.Models;
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
        var currentParticipant = await GetRegistrationCountByEventId(citlaliEvent.EventId);

        return new EventBriefCardData
        {
            EventId = citlaliEvent.EventId,
            EventTitle = citlaliEvent.EventTitle,
            EventDescription = citlaliEvent.EventDescription,
            CreatorDisplayName = creator.DisplayName,
            CreatorProfileImageUrl = creator.ProfileImageUrl,
            LocationTag = locationTag,
            EventCategoryTag = categoryTag,
            CurrentParticipant = currentParticipant,
            MaxParticipant = citlaliEvent.MaxParticipant,
            Cost = citlaliEvent.Cost,
            EventDate = citlaliEvent.EventDate,
            PostExpiryDate = citlaliEvent.PostExpiryDate,
            CreatedAt = citlaliEvent.CreatedAt,
        };
    }

    public async Task<EventDetailCardData> EventToDetailCard(Event citlaliEvent)
    {
        var creatorTask = _userService.GetUserByUserId(citlaliEvent.CreatorUserId);
        var locationTagTask = GetLocationTagById(citlaliEvent.EventLocationTagId);
        var categoryTagTask = GetTagById(citlaliEvent.EventCategoryTagId);

        await Task.WhenAll(creatorTask, locationTagTask, categoryTagTask);

        var creator = await creatorTask ?? throw new Exception("Creator not found");
        var locationTag = await locationTagTask ?? throw new Exception("Location not found");
        var categoryTag = await categoryTagTask ?? throw new Exception("Category not found");
        var currentParticipant = await GetRegistrationCountByEventId(citlaliEvent.EventId);

        return new EventDetailCardData
        {
            EventId = citlaliEvent.EventId,
            EventTitle = citlaliEvent.EventTitle,
            EventDescription = citlaliEvent.EventDescription,
            CreatorUsername = creator.Username,
            CreatorDisplayName = creator.DisplayName,
            CreatorProfileImageUrl = creator.ProfileImageUrl,
            LocationTag = locationTag,
            EventCategoryTag = categoryTag,
            CurrentParticipant = currentParticipant,
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

    public async Task<List<User>> GetRegistrantsByEventId(Guid eventId)
    {
        var response = await _supabaseClient
            .From<Registration>()
            .Where(row => row.EventId == eventId)     // Please add a filter here to get only the registrants whol have been accepted
            .Select("UserId")
            .Get();

        var eventRegistrants = response.Models;

        var registrants = new List<User>();
        foreach (var registrant in eventRegistrants)
        {
            var user = await _userService.GetUserByUserId(registrant.UserId);
            if (user != null)
            {
                registrants.Add(user);
            }
        }

        return registrants;
    }

    public async Task<int> GetRegistrationCountByEventId(Guid eventId)
    {
        var response = await _supabaseClient
            .Rpc("count_participants", new Dictionary<string, object>
            {
                { "event_uuid", eventId }
            });


        return response.Content != null ? int.Parse(response.Content) : 0;
    }

    public async Task<int> GetEventCountByTagId(Guid tagId)
    {
        var response = await _supabaseClient
            .From<Event>()
            .Filter("EventCategoryTagId", Supabase.Postgrest.Constants.Operator.Equals, tagId.ToString())
            .Filter("Deleted", Supabase.Postgrest.Constants.Operator.Equals, "false")
            .Filter(row => row.PostExpiryDate, Supabase.Postgrest.Constants.Operator.GreaterThan, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"))
            .Filter(row => row.Status, Supabase.Postgrest.Constants.Operator.Equals, "active")
            .Count(Supabase.Postgrest.Constants.CountType.Exact);

        return response;
    }

    public async Task<int> GetTagFollowersCountByTagId(Guid tagId)
    {
        var response = await _supabaseClient
            .From<UserFollowedCategory>()
            .Filter("EventCategoryTagId", Supabase.Postgrest.Constants.Operator.Equals, tagId.ToString())
            .Count(Supabase.Postgrest.Constants.CountType.Exact);

        return response;
    }

    public async Task<bool> UpdateEventStatus(Guid eventId)
    {
        var response = await _supabaseClient
             .Rpc("update_event_status", new Dictionary<string, object>
             {
                { "event_uuid", eventId }
             });

        return true;
    }


    public async Task<bool> IsUserRegistered(Guid eventId, Guid userId)
    {
        var response = await _supabaseClient
            .From<Registration>()
            .Select("*")
            .Filter("EventId", Supabase.Postgrest.Constants.Operator.Equals, eventId.ToString())
            .Filter("UserId", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Get();

        return response.Models.Count != 0;
    }

    //GetEventsByTagId
    public async Task<List<Event>> GetEventsByTagId(Guid tagId)
    {
        var response = await _supabaseClient
            .From<Event>()
            .Select("*")
            .Filter("EventCategoryTagId", Supabase.Postgrest.Constants.Operator.Equals, tagId.ToString())
            .Filter("Deleted", Supabase.Postgrest.Constants.Operator.Equals, "false")
            .Filter(row => row.PostExpiryDate, Supabase.Postgrest.Constants.Operator.GreaterThan, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"))
            .Filter(row => row.Status, Supabase.Postgrest.Constants.Operator.Equals, "active")
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

    public async Task<EventManagementViewModel> GetEventManagement(Guid eventId)
    {
        var currentUser = _userService.CurrentSession.User
                        ?? throw new UnauthorizedAccessException("User not authenticated");
        var userId = Guid.Parse(currentUser.Id ?? throw new UnauthorizedAccessException("User not found"));

        var userTask = _userService.GetUserByUserId(userId);
        var eventTask = GetEventById(eventId);

        await Task.WhenAll(userTask, eventTask);

        var user = userTask.Result ?? throw new KeyNotFoundException("User not found");
        var ev = eventTask.Result ?? throw new KeyNotFoundException("Event not found");

        if (ev.CreatorUserId.ToString() != currentUser.Id)
        {
            throw new UnauthorizedAccessException("User not authorized to access this event");
        }

        var locationTagTask = GetLocationTagById(ev.EventLocationTagId);
        var eventCategoryTagTask = GetTagById(ev.EventCategoryTagId);
        var currentParticipantCountTask = GetRegistrationCountByEventId(ev.EventId);
        var eventQuestionsTask = _supabaseClient
            .From<EventQuestion>()
            .Select("*")
            .Filter("EventId", Supabase.Postgrest.Constants.Operator.Equals, ev.EventId.ToString())
            .Get();

        var registrationsTask = _supabaseClient
            .From<Registration>()
            .Select("*")
            .Filter("EventId", Supabase.Postgrest.Constants.Operator.Equals, ev.EventId.ToString())
            .Get();

        await Task.WhenAll(locationTagTask, eventCategoryTagTask, currentParticipantCountTask,
                        eventQuestionsTask, registrationsTask);

        var locationTag = locationTagTask.Result ?? new LocationTag();
        var eventCategoryTag = eventCategoryTagTask.Result ?? new EventCategoryTag();
        var eventQuestions = eventQuestionsTask.Result.Models;
        var registrations = registrationsTask.Result.Models;

        var questionLookup = eventQuestions.ToDictionary(q => q.EventQuestionId, q => q.Question);

        var registrationIds = registrations.Select(r => r.RegistrationId).ToList();

        var userIds = registrations.Select(r => r.UserId).Distinct().ToList();
        var usersTask = Task.WhenAll(userIds.Select(uid => _userService.GetUserByUserId(uid)));

        var answersTask = _supabaseClient
            .From<RegistrationAnswer>()
            .Select("*")
            .Filter(x => x.RegistrationAnswerId, Supabase.Postgrest.Constants.Operator.In,
                registrationIds.Select(id => id.ToString()).ToList())
            .Get();

        await Task.WhenAll(usersTask, answersTask);

        var allUsers = usersTask.Result.Where(u => u != null).ToDictionary(u => u?.UserId ?? Guid.Empty);
        var allAnswers = answersTask.Result.Models;
        var answersByRegistration = allAnswers
            .GroupBy(a => a.RegistrationId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var answerSet = new List<EventManagementAnswerCollection>();
        var confirmedParticipant = new List<BriefUser>();
        var awaitingConfirmationParticipant = new List<BriefUser>();
        var rejectedConfirmationParticipant = new List<BriefUser>();

        foreach (var registration in registrations)
        {
            if (!allUsers.TryGetValue(registration.UserId, out var registrant))
                continue;

            var briefUser = new BriefUser
            {
                UserId = registrant?.UserId ?? throw new GetUserException(),
                Username = registrant.Username,
                ProfileImageUrl = registrant.ProfileImageUrl,
                DisplayName = registrant.DisplayName
            };

            switch (registration.Status)
            {
                case "confirmed":
                    confirmedParticipant.Add(briefUser);
                    break;
                case "awaiting-confirmation":
                    awaitingConfirmationParticipant.Add(briefUser);
                    break;
                case "rejected-invitation":
                    rejectedConfirmationParticipant.Add(briefUser);
                    break;
            }

            var registrationAnswers = new List<RegistrationAnswerSimplify>();
            if (answersByRegistration.TryGetValue(registration.RegistrationId, out var answers))
            {
                registrationAnswers = answers.Select(answer => new RegistrationAnswerSimplify
                {
                    EventQuestionId = answer.EventQuestionId,
                    Question = questionLookup.GetValueOrDefault(answer.EventQuestionId, ""),
                    Answer = answer.Answer
                }).ToList();
            }

            answerSet.Add(new EventManagementAnswerCollection
            {
                User = registrant,
                Status = registration.Status,
                RegistrationAnswers = registrationAnswers
            });
        }

        return new EventManagementViewModel
        {
            EventId = ev.EventId,
            EventTitle = ev.EventTitle,
            EventDescription = ev.EventDescription,
            CreatorUsername = user.Username,
            CreatorDisplayName = user.DisplayName,
            CreatorProfileImageUrl = user.ProfileImageUrl,
            LocationTag = locationTag,
            EventCategoryTag = eventCategoryTag,
            CurrentParticipant = currentParticipantCountTask.Result,
            MaxParticipant = ev.MaxParticipant,
            Cost = ev.Cost,
            EventDate = ev.EventDate,
            PostExpiryDate = ev.PostExpiryDate,
            EventStatus = ev.Status,
            CreatedAt = ev.CreatedAt,
            Questions = questionLookup.Select(q => new QuestionViewModel
            {
                EventQuestionId = q.Key,
                Question = q.Value,
                Answer = ""
            }).ToList(),
            AnswerSet = answerSet,
            ConfirmedParticipant = confirmedParticipant,
            AwaitingConfirmationParticipant = awaitingConfirmationParticipant,
            RejectedConfirmationParticipant = rejectedConfirmationParticipant
        };
    }

    public async Task<EditEventViewModel> GetEventEditPage(Guid eventId)
    {
        var currentUser = _userService.CurrentSession.User
                        ?? throw new UnauthorizedAccessException("User not authenticated");
        var userId = Guid.Parse(currentUser.Id
                        ?? throw new UnauthorizedAccessException("User ID not found"));

        var eventTask = GetEventById(eventId);
        var ev = await eventTask ?? throw new KeyNotFoundException("Event not found");

        if (ev.CreatorUserId != userId)
            throw new UnauthorizedAccessException("User not authorized to edit this event");

        var tasks = new Task[]
        {
            Task.CompletedTask,
            GetLocationTagById(ev.EventLocationTagId),
            GetTagById(ev.EventCategoryTagId),
            GetLocationTags(),
            GetTags(),
            GetQuestionsByEventId(eventId),
            GetRegistrationCountByEventId(eventId)

        };

        await Task.WhenAll(tasks);

        var locationTag = await (Task<LocationTag?>)tasks[1] ?? new LocationTag();
        var categoryTag = await (Task<EventCategoryTag?>)tasks[2] ?? new EventCategoryTag();
        var locationTags = await (Task<List<Location>>)tasks[3];
        var categoryTags = await (Task<List<Tag>>)tasks[4];
        var questions = await (Task<List<QuestionViewModel>>)tasks[5];
        var currentParticipant = await (Task<int>)tasks[6];

        return new EditEventViewModel
        {
            EventId = ev.EventId,
            EventTitle = ev.EventTitle,
            EventDescription = ev.EventDescription,
            EventCategoryTag = categoryTag,
            EventLocationTag = locationTag,
            MaxParticipant = ev.MaxParticipant,
            CurrentParticipant = currentParticipant,
            Cost = ev.Cost,
            EventDate = ev.EventDate,
            PostExpiryDate = ev.PostExpiryDate,
            LocationTagsList = locationTags,
            EventCategoryTagsList = categoryTags,
            Questions = questions
        };
    }

    public async Task<Registration?> GetRegistrationByEventIdAndUserId(Guid eventId, Guid userId)
    {
        var response = await _supabaseClient
            .From<Registration>()
            .Select("*")
            .Filter("EventId", Supabase.Postgrest.Constants.Operator.Equals, eventId.ToString())
            .Filter("UserId", Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Single();

        return response ?? null;
    }

    public async Task<EventStatusViewModel> GetEventStatus(Guid eventId)
    {
        var currentUser = _userService.CurrentSession.User
                        ?? throw new UnauthorizedAccessException("User not authenticated");
        var user = _userService.GetUserByUserId(Guid.Parse(currentUser.Id ?? throw new UnauthorizedAccessException("User not found")));
        var ev = await GetEventById(eventId)
                ?? throw new KeyNotFoundException("Event not found");

        if (ev.CreatorUserId.ToString() == currentUser.Id)
            throw new JoinOwnerException("Owner cannot join their own event.");

        if (!await IsUserRegistered(eventId, Guid.Parse(currentUser.Id)))
            throw new UserHasNotRegisteredException("User has not registered to this event.");

        var registration = await GetRegistrationByEventIdAndUserId(eventId, Guid.Parse(currentUser.Id))
                ?? throw new KeyNotFoundException("Registration not found");

        return new EventStatusViewModel
        {
            Status = registration.Status,
            RegistrationTime = registration.CreatedAt,
            EventDetailCardData = await EventToDetailCard(ev),
        };

    }

    //CancelRegistration
    public async Task<bool> CancelRegistration(Guid eventId)
    {
        var supabaseUser = _userService.CurrentSession.User
                        ?? throw new UnauthorizedAccessException("User not authenticated");
        if (supabaseUser == null)
            throw new UnauthorizedAccessException("User not authenticated");

        var userId = Guid.Parse(supabaseUser.Id ?? throw new UnauthorizedAccessException("User ID not found"));

        var registration = await GetRegistrationByEventIdAndUserId(eventId, userId)
                ?? throw new KeyNotFoundException("Registration not found");

        if (registration.Status == "confirmed") // if user has been confirmed
            throw new Exception("Cannot cancel registration after confirmation");

        await _supabaseClient
            .From<Registration>()
            .Where(row => row.RegistrationId == registration.RegistrationId)
            .Delete();


        return true;
    }

    //RejectedInvitation
    public async Task<bool> RejectedInvitation(Guid eventId)
    {
        var supabaseUser = _userService.CurrentSession.User
                        ?? throw new UnauthorizedAccessException("User not authenticated");
        if (supabaseUser == null)
            throw new UnauthorizedAccessException("User not authenticated");

        var userId = Guid.Parse(supabaseUser.Id ?? throw new UnauthorizedAccessException("User ID not found"));

        var registration = await GetRegistrationByEventIdAndUserId(eventId, userId)
                ?? throw new KeyNotFoundException("Registration not found");

        if (registration.Status != "awaiting-confirmation") // if user has been confirmed
            throw new Exception("You cannot reject this invitation");

        await _supabaseClient
            .From<Registration>()
            .Where(row => row.RegistrationId == registration.RegistrationId)
            .Set(row => row.Status, "rejected-invitation")
            .Set(row => row.UpdatedAt, DateTime.UtcNow)
            .Update();

        var CreatorUserId = await GetCreatorEventIdByEventId(eventId);

        var citlaliEvent = await GetEventById(eventId) ?? throw new KeyNotFoundException("Event not found");

        var notificationTitle = "Invitation Rejected ❌";
        var notificationBody = $"A user has rejected your invitation to the event {citlaliEvent.EventTitle}. You may invite other registrant from event management page.";

        await _notificationService.CreateNotification(CreatorUserId, notificationTitle, notificationBody, $"/event/detail/{eventId}");


        return true;
    }

    //ConfirmRegistration
    public async Task<bool> ConfirmRegistration(Guid eventId)
    {
        var supabaseUser = _userService.CurrentSession.User
                        ?? throw new UnauthorizedAccessException("User not authenticated");
        if (supabaseUser == null)
            throw new UnauthorizedAccessException("User not authenticated");

        var userId = Guid.Parse(supabaseUser.Id ?? throw new UnauthorizedAccessException("User ID not found"));

        var registration = await GetRegistrationByEventIdAndUserId(eventId, userId)
                ?? throw new KeyNotFoundException("Registration not found");

        if (registration.Status != "awaiting-confirmation")
            throw new Exception("You cannot confirm this registration");

        await _supabaseClient
            .From<Registration>()
            .Where(row => row.RegistrationId == registration.RegistrationId)
            .Set(row => row.Status, "confirmed")
            .Set(row => row.UpdatedAt, DateTime.UtcNow)
            .Update();

        var CreatorUserId = await GetCreatorEventIdByEventId(eventId);

        var citlaliEvent = await GetEventById(eventId) ?? throw new KeyNotFoundException("Event not found");

        var notificationTitle = "Invitation Confirmed ✅";
        var notificationBody = $"Congratulations! A user has confirmed your invitation to the event {citlaliEvent.EventTitle}. You may view the confirmed registrants from event management page.";

        await _notificationService.CreateNotification(CreatorUserId, notificationTitle, notificationBody, $"/event/detail/{eventId}");

        await UpdateEventStatus(eventId);

        return true;
    }


    //GetRegistrantsConfirmedByEventId
    public async Task<List<Registration>> GetRegistrantsConfirmedByEventId(Guid eventId)
    {
        var response = await _supabaseClient
            .From<Registration>()
            .Select("UserId")
            .Filter("EventId", Supabase.Postgrest.Constants.Operator.Equals, eventId.ToString())
            .Filter("Status", Supabase.Postgrest.Constants.Operator.Equals, "confirmed")
            .Get();

        return response.Models;
    }


    //Broadcast
    public async Task<bool> Broadcast(Guid eventId, string title, string message)
    {
        var supabaseUser = _userService.CurrentSession.User
                        ?? throw new UnauthorizedAccessException("User not authenticated");
        var userId = Guid.Parse(supabaseUser.Id ?? throw new GetUserException());

        var Event = await GetEventById(eventId) ?? throw new KeyNotFoundException("Event not found");

        if (Event.CreatorUserId.ToString() != userId.ToString())
            throw new UnauthorizedAccessException("User not authorized to broadcast this event");

        var registrants = await GetRegistrantsConfirmedByEventId(eventId);

        if (registrants != null && registrants.Count > 0)
        {
            var notificationTasks = registrants.Select(registrant =>
                _notificationService.CreateNotification(registrant.UserId, title, message, $"/event/detail/{eventId}")
            );

            await Task.WhenAll(notificationTasks);
        }

        return true;

    }

    public async Task<RegistrationHistoryData> GetHistory()
    {
        var currentUser = _userService.CurrentSession.User;
        if (currentUser == null)
            throw new UnauthorizedAccessException("User not authenticated.");
        var userId = Guid.Parse(currentUser.Id ?? throw new UnauthorizedAccessException("User not authenticated."));

        var registrationHistory = await _supabaseClient
            .From<Registration>()
            .Filter(row => row.UserId, Supabase.Postgrest.Constants.Operator.Equals, userId.ToString())
            .Order("CreatedAt", Supabase.Postgrest.Constants.Ordering.Descending)
            .Get();

        var historyList = new RegistrationHistoryData();

        foreach (var registration in registrationHistory.Models)
        {
            var citlaliEvent = await GetEventById(registration.EventId);
            if (citlaliEvent == null)
                continue;

            var creatorTask = _userService.GetUserByUserId(citlaliEvent.CreatorUserId);
            var locationTagTask = GetLocationTagById(citlaliEvent.EventLocationTagId);
            var eventCategoryTagTask = GetTagById(citlaliEvent.EventCategoryTagId);

            await Task.WhenAll(creatorTask, locationTagTask, eventCategoryTagTask);

            var creator = await creatorTask;
            var locationTag = await locationTagTask;
            var eventCategoryTag = await eventCategoryTagTask;

            if (creator == null || locationTag == null || eventCategoryTag == null)
                continue;

            var historyCard = new RegistrationHistoryCardModel
            {
                EventId = registration.EventId,
                EventTitle = citlaliEvent.EventTitle,
                EventDescription = citlaliEvent.EventDescription,
                CreatorUsername = creator.Username,
                CreatorDisplayName = creator.DisplayName,
                CreatorProfileImageUrl = creator.ProfileImageUrl,
                LocationTag = locationTag,
                EventCategoryTag = eventCategoryTag,
                Status = registration.Status,
                RegistrationTime = registration.CreatedAt
            };

            historyList.RegistrationHistoryCardModels.Add(historyCard);
        }

        return historyList;
    }

    public async Task<List<EventBriefCardData>> GetTrendingEvents()
    {
        try
        {
            // Get events created in the last week that are active
            var allActiveEvents = await _supabaseClient
                .From<Event>()
                .Select("*")
                .Filter("Deleted", Supabase.Postgrest.Constants.Operator.Equals, "false")
                .Filter("CreatedAt", Supabase.Postgrest.Constants.Operator.GreaterThan, DateTime.Now.AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ssZ"))
                .Filter(row => row.PostExpiryDate, Supabase.Postgrest.Constants.Operator.GreaterThan, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"))
                .Filter(row => row.Status, Supabase.Postgrest.Constants.Operator.Equals, "active")
                .Get();

            if (allActiveEvents?.Models == null || allActiveEvents.Models.Count == 0)
                return new List<EventBriefCardData>();

            // Get participant count for each event concurrently
            var eventWithParticipants = new List<(Event Event, int Count)>();
            var tasks = allActiveEvents.Models.Select(async e =>
            {
                var count = await GetRegistrationCountByEventId(e.EventId);
                return (e, count);
            });

            eventWithParticipants = (await Task.WhenAll(tasks)).ToList();

            // Sort events by participant count and take top 5
            var sortedEvents = eventWithParticipants
                .OrderByDescending(x => x.Count)
                .Take(5)
                .Select(x => x.Event)
                .ToList();

            if (sortedEvents.Count == 0)
                return new List<EventBriefCardData>();

            // Convert to brief card data
            var eventBriefCardDataTasks = sortedEvents.Select(EventToBriefCard);
            var eventBriefCardData = await Task.WhenAll(eventBriefCardDataTasks);

            return eventBriefCardData.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting trending events: {ex.Message}");
            return new List<EventBriefCardData>();
        }
    }

    public async Task<List<PopularTag>> GetPopularTags()
    {
        // Get all active events
        var allActiveEvents = await _supabaseClient
                    .From<Event>()
                    .Select("*")
                    .Filter("Deleted", Supabase.Postgrest.Constants.Operator.Equals, "false")
                    .Filter(row => row.PostExpiryDate, Supabase.Postgrest.Constants.Operator.GreaterThan, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"))
                    .Filter(row => row.Status, Supabase.Postgrest.Constants.Operator.Equals, "active")
                    .Get();

        // Count occurrences of each category tag
        var tagCounts = new Dictionary<Guid, int>();
        foreach (var e in allActiveEvents.Models)
        {
            if (tagCounts.TryGetValue(e.EventCategoryTagId, out var count))
            {
                tagCounts[e.EventCategoryTagId] = count + 1;
            }
            else
            {
                tagCounts[e.EventCategoryTagId] = 1;
            }
        }

        // Get the top 5 most popular tag IDs
        var popularTagsId = tagCounts.OrderByDescending(tc => tc.Value)
                                    .Take(5)
                                    .Select(tc => tc.Key)
                                    .ToList();

        // Fetch all popular tags individually
        var popularTags = new List<PopularTag>();
        foreach (var tagId in popularTagsId)
        {
            var tagResponse = await _supabaseClient
                .From<EventCategoryTag>()
                .Select("*")
                .Filter(tag => tag.EventCategoryTagId, Supabase.Postgrest.Constants.Operator.Equals, tagId.ToString())
                .Single();

            if (tagResponse != null)
            {
                popularTags.Add(new PopularTag
                {
                    EventCategoryTagId = tagResponse.EventCategoryTagId,
                    EventCategoryTagName = tagResponse.EventCategoryTagName,
                    EventCategoryTagEmoji = tagResponse.EventCategoryTagEmoji,
                    EventCount = tagCounts[tagResponse.EventCategoryTagId]
                });
            }
        }

        return popularTags;
    }


    public async Task<List<Event>> GetEventsByLocationId(Guid locationId)
    {
        try
        {
            var response = await _supabaseClient
                .From<Event>()
                .Where(e => e.EventLocationTagId == locationId)
                .Filter("Deleted", Supabase.Postgrest.Constants.Operator.Equals, "false")
                .Filter(row => row.PostExpiryDate, Supabase.Postgrest.Constants.Operator.GreaterThan, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"))
                .Filter(row => row.Status, Supabase.Postgrest.Constants.Operator.Equals, "active")
                .Get();

            return response.Models;
        }
        catch (Exception)
        {
            return new List<Event>();
        }
    }

    public async Task<int> GetEventCountByLocationId(Guid locationId)
    {
        var response = await _supabaseClient
            .From<Event>()
            .Filter("EventLocationTagId", Supabase.Postgrest.Constants.Operator.Equals, locationId.ToString())
            .Filter("Deleted", Supabase.Postgrest.Constants.Operator.Equals, "false")
            .Filter(row => row.PostExpiryDate, Supabase.Postgrest.Constants.Operator.GreaterThan, DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"))
            .Filter(row => row.Status, Supabase.Postgrest.Constants.Operator.Equals, "active")
            .Count(Supabase.Postgrest.Constants.CountType.Exact);

        return response;
    }

    // bypass confirmation 
    public async Task<bool> BypassConfirmation(Guid eventId, Guid userId)
    {
        var registration = await GetRegistrationByEventIdAndUserId(eventId, userId)
                ?? throw new KeyNotFoundException("Registration not found");

        await _supabaseClient
            .From<Registration>()
            .Where(row => row.RegistrationId == registration.RegistrationId)
            .Set(row => row.Status, "confirmed")
            .Set(row => row.UpdatedAt, DateTime.UtcNow)
            .Update();

        await UpdateEventStatus(eventId);

        return true;
    }

}

public class UserAlreadyRegisteredException : Exception
{
    public UserAlreadyRegisteredException() : base("User already register to this event.") { }

    public UserAlreadyRegisteredException(string message) : base(message) { }

    public UserAlreadyRegisteredException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class UserHasNotRegisteredException : Exception
{
    public UserHasNotRegisteredException() : base("User has not registered to this event.") { }

    public UserHasNotRegisteredException(string message) : base(message) { }

    public UserHasNotRegisteredException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class JoinOwnerException : Exception
{
    public JoinOwnerException() : base("Owner cannot join their own event.") { }

    public JoinOwnerException(string message) : base(message) { }

    public JoinOwnerException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class MaximumInvitationExceedException : Exception
{
    public MaximumInvitationExceedException() : base("Maximum invitation has been exceeded.") { }

    public MaximumInvitationExceedException(string message) : base(message) { }

    public MaximumInvitationExceedException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class EventClosedException : Exception
{
    public EventClosedException() : base("Event have been closed.") { }

    public EventClosedException(string message) : base(message) { }

    public EventClosedException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class MaximumParticipantExceedException : Exception
{
    public MaximumParticipantExceedException() : base("Maximum participant has been exceeded.") { }

    public MaximumParticipantExceedException(string message) : base(message) { }

    public MaximumParticipantExceedException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class EventNotFoundException : Exception
{
    public EventNotFoundException() : base("Event not found!") { }

    public EventNotFoundException(string message) : base(message) { }

    public EventNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}