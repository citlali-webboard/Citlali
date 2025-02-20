using Supabase;
using Citlali.Models;
using Microsoft.AspNetCore.Http.HttpResults;

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

    public async Task<bool> InviteUser(Guid eventId, Guid userId)
    {
        var supabaseUser = _userService.CurrentSession.User
            ?? throw new UnauthorizedAccessException("User not authenticated");

        var eventToInvite = await GetEventById(eventId)
            ?? throw new KeyNotFoundException("Event not found");

        if (eventToInvite.CreatorUserId.ToString() != supabaseUser.Id)
        {
            throw new UnauthorizedAccessException("User not authorized to invite to this event");
        }

        var invitedRegistrantCount = await _supabaseClient
            .From<Registration>()
            .Filter("EventId", Supabase.Postgrest.Constants.Operator.Equals, eventId.ToString())
            .Filter("Status", Supabase.Postgrest.Constants.Operator.In, new[] { "awaiting-confirmation", "confirmed" })
            .Count(Supabase.Postgrest.Constants.CountType.Exact);

        var registration = await GetRegistrationByEventIdAndUserId(eventId, userId)
            ?? throw new KeyNotFoundException("Registration not found");

        if (invitedRegistrantCount >= eventToInvite.MaxParticipant)
        {
            throw new MaximumInvitationExceedException();
        }

        await _supabaseClient
            .From<Registration>()
            .Where(row => row.RegistrationId == registration.RegistrationId)
            .Set(row => row.Status, "awaiting-confirmation")
            .Update();

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
            .Update();

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

        if (isOwner)
        {
            throw new JoinOwnerException(); // Redirect to "manage" page
        }
        if (isRegistered)
        {
            throw new UserAlreadyRegisteredException(); // Redirect to "status" page
        }

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
        var eventQuestionsTask = _supabaseClient
            .From<EventQuestion>()
            .Where(x => x.EventId == joinEventModel.EventId)
            .Get();

        if (await IsUserRegistered(EventId, userId))
        {
            throw new UserAlreadyRegisteredException();
        }

        if (userId == (await GetEventById(EventId))?.CreatorUserId)
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

        var user = await _userService.GetUserByUserId(Guid.Parse(currentUser.Id ?? throw new UnauthorizedAccessException("User not found")));

        var ev = await GetEventById(eventId)
                ?? throw new KeyNotFoundException("Event not found");

        if (ev.CreatorUserId.ToString() != currentUser.Id)
        {
            throw new UnauthorizedAccessException("User not authorized to access this event");
        }

        var locationTagTask = GetLocationTagById(ev.EventLocationTagId);
        var eventCategoryTagTask = GetTagById(ev.EventCategoryTagId);
        var registrantsTask = GetRegistrantsByEventId(ev.EventId);

        await Task.WhenAll(locationTagTask, eventCategoryTagTask, registrantsTask);

        var locationTag = locationTagTask.Result ?? new LocationTag();
        var eventCategoryTag = eventCategoryTagTask.Result ?? new EventCategoryTag();
        var registrants = registrantsTask.Result;

        var eventQuestions = (await _supabaseClient
            .From<EventQuestion>()
            .Select("*")
            .Filter("EventId", Supabase.Postgrest.Constants.Operator.Equals, ev.EventId.ToString())
            .Get()).Models;

        var questionLookup = eventQuestions.ToDictionary(q => q.EventQuestionId, q => q.Question);

        var answerSet = new List<EventManagementAnswerCollection>();
        var ConfirmedParticipant = new List<BriefUser>();
        var AwaitingConfirmationParticipant = new List<BriefUser>();
        var RejectedConfirmationParticipant = new List<BriefUser>();

        foreach (var registrant in registrants)
        {
            var registration = await _supabaseClient
                .From<Registration>()
                .Select("*")
                .Filter("EventId", Supabase.Postgrest.Constants.Operator.Equals, ev.EventId.ToString())
                .Filter("UserId", Supabase.Postgrest.Constants.Operator.Equals, registrant.UserId.ToString())
                .Single();

            if (registration == null) continue;

            var registrationId = registration.RegistrationId;

            if (registration.Status == "confirmed")
            {
                ConfirmedParticipant.Add(new BriefUser
                {
                    UserId = registrant.UserId,
                    Username = registrant.Username,
                    ProfileImageUrl = registrant.ProfileImageUrl,
                    DisplayName = registrant.DisplayName
                });
            }
            else if (registration.Status == "awaiting-confirmation")
            {
                AwaitingConfirmationParticipant.Add(new BriefUser
                {
                    UserId = registrant.UserId,
                    Username = registrant.Username,
                    ProfileImageUrl = registrant.ProfileImageUrl,
                    DisplayName = registrant.DisplayName
                });
            }

            var answers = (await _supabaseClient
                .From<RegistrationAnswer>()
                .Select("*")
                .Filter("RegistrationId", Supabase.Postgrest.Constants.Operator.Equals, registrationId.ToString())
                .Get()).Models;

            var registrationAnswers = answers.Select(answer => new RegistrationAnswerSimplify
            {
                EventQuestionId = answer.EventQuestionId,
                Question = questionLookup.GetValueOrDefault(answer.EventQuestionId, ""),
                Answer = answer.Answer
            }).ToList();

            answerSet.Add(new EventManagementAnswerCollection
            {
                User = registrant,
                Status = registration.Status,
                RegistrationAnswers = registrationAnswers
            });
        }

        var questionList = questionLookup.Select(q => new QuestionViewModel
        {
            EventQuestionId = q.Key,
            Question = q.Value,
            Answer = ""
        }).ToList();

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
            CurrentParticipant = registrants.Count,
            MaxParticipant = ev.MaxParticipant,
            Cost = ev.Cost,
            EventDate = ev.EventDate,
            PostExpiryDate = ev.PostExpiryDate,
            EventStatus = ev.Status,
            Questions = questionList,
            AnswerSet = answerSet,
            ConfirmedParticipant = ConfirmedParticipant,
            AwaitingConfirmationParticipant = AwaitingConfirmationParticipant
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
        var userId = Guid.Parse(supabaseUser.Id);

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
        var userId = Guid.Parse(supabaseUser.Id);

        var registration = await GetRegistrationByEventIdAndUserId(eventId, userId)
                ?? throw new KeyNotFoundException("Registration not found");

        if (registration.Status != "awaiting-confirmation") // if user has been confirmed
            throw new Exception("You cannot reject this invitation");

        await _supabaseClient
            .From<Registration>()
            .Where(row => row.RegistrationId == registration.RegistrationId)
            .Set(row => row.Status, "rejected-invitation")
            .Update();


        return true;
    }

    //ConfirmRegistration
    public async Task<bool> ConfirmRegistration(Guid eventId)
    {
        var supabaseUser = _userService.CurrentSession.User
                        ?? throw new UnauthorizedAccessException("User not authenticated");
        var userId = Guid.Parse(supabaseUser.Id);

        var registration = await GetRegistrationByEventIdAndUserId(eventId, userId)
                ?? throw new KeyNotFoundException("Registration not found");

        if (registration.Status != "awaiting-confirmation")
            throw new Exception("You cannot confirm this registration");

        await _supabaseClient
            .From<Registration>()
            .Where(row => row.RegistrationId == registration.RegistrationId)
            .Set(row => row.Status, "confirmed")
            .Update();

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