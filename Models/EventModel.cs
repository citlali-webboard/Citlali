using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;

namespace Citlali.Models;

[Table("EVENTS")]
public class Event : BaseModel
{
    [PrimaryKey]
    [Column("EventId")]
    public Guid EventId { get; set; } = new();

    [Column("CreatorUserId")]
    public Guid CreatorUserId { get; set; } = new();

    [Column("EventTitle")]
    public string EventTitle { get; set; } = "";

    [Column("EventDescription")]
    public string EventDescription { get; set; } = "";

    [Column("EventCategoryTagId")]
    public Guid EventCategoryTagId { get; set; } = new();

    [Column("EventLocationTagId")]
    public Guid EventLocationTagId { get; set; } = new();

    [Column("MaxParticipant")]
    public int MaxParticipant { get; set; } = 0;

    [Column("Cost")]
    public int Cost { get; set; } = 0;

    [Column("EventDate")]
    public DateTime EventDate { get; set; } = new();

    [Column("PostExpiryDate")]
    public DateTime PostExpiryDate { get; set; } = new();

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("Deleted")]
    public bool Deleted { get; set; } = false;

    [Column("Status")]
    public string Status { get; set; } = "active";
}

[Table("EVENT_QUESTION")]
public class EventQuestion : BaseModel
{
    [PrimaryKey]
    [Column("EventQuestionId")]
    public Guid EventQuestionId { get; set; } = new();

    [Column("EventId")]
    public Guid EventId { get; set; } = new();

    [Column("Question")]
    public string Question { get; set; } = "";

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}

[Table("LOCATION_TAG")]
public class EventLocationTag
{
    [PrimaryKey]
    [Column("LocationTagId")]
    public Guid EventLocationTagId {get ; set;} = new();

    [Column("LocationTagName")]
    public string EventLocationTagName {get ; set;} = "";
}

public class EventBriefCardData
{
    public Guid EventId {get ; set;} = new();
    public string EventTitle {get ; set;} = "";
    public string EventDescription {get ; set;} = "";
    public string CreatorUsername {get ; set; } = "";
    public string CreatorDisplayName {get ; set;} = "";
    public string CreatorProfileImageUrl {get ; set;} = "";
    public LocationTag LocationTag {get ; set;} = new();
    public EventCategoryTag EventCategoryTag {get ; set;} = new();
    public int CurrentParticipant {get ; set;} = 0;
    public int MaxParticipant {get ; set;} = 0;
    public int Cost {get ; set;} = 0;
    public DateTime EventDate {get ; set;} = new();
    public DateTime PostExpiryDate {get ; set;} = new();
    public DateTime CreatedAt {get ; set;} = new();

}

public class EventDetailCardData : EventBriefCardData
{
}

public class EventFormDto
{
    public Guid EventId { get; set; } = new();
    public List<QuestionViewModel> Questions { get; set; } = [];
}

public class EventDetailViewModel
{
    public bool IsUserRegistered { get; set; } = false;
    public EventDetailCardData EventDetailCardData { get; set; } = new();
    public EventFormDto EventFormDto { get; set; } = new();
}

public class JoinEventModel
{
    public Guid EventId { get; set; } = new();
    public EventFormDto EventFormDto { get; set; } = new();
}

public class QuestionViewModel
{
    public Guid EventQuestionId { get; set; }
    public string Question { get; set; } = "";
    [Required(ErrorMessage = "This field is required")]

    public string Answer { get; set; } = "";
}

public class EventExploreViewModel
{
    public EventBriefCardData[] EventBriefCardDatas = [new()];
    public Tag[] Tags = [new()];
    public int CurrentPage { get; set; } = 1;
    public int TotalPage { get; set; }
}

public class TagEventExploreViewModel : EventExploreViewModel
{
    public Guid TagId { get; set; } = new();
    public string TagName { get; set; } = "";
    public string TagEmoji { get; set; } = "";
}

[Table("EVENT_CATEGORY_TAG")]
public class EventCategoryTag : BaseModel
{
    [PrimaryKey]
    [Column("EventCategoryTagId")]
    public Guid EventCategoryTagId { get; set; } = new();

    [Column("EventCategoryTagEmoji")]
    public string EventCategoryTagEmoji { get; set; } = "";

    [Column("EventCategoryTagName")]
    public string EventCategoryTagName { get; set; } = "";

    [Column("Deleted")]
    public bool Deleted { get; set; } = false;
}

public class Tag
{
    public Guid TagId = new();
    public string TagEmoji = "";
    public string TagName = "";
}

[Table("LOCATION_TAG")]
public class LocationTag : BaseModel
{
    [PrimaryKey]
    [Column("LocationTagId")]
    public Guid LocationTagId { get; set; } = new();

    [Column("LocationTagName")]
    public string LocationTagName { get; set; } = "";

    [Column("Deleted")]
    public bool Deleted { get; set; } = false;
}

public class Location
{
    public Guid EventLocationTagId = new();
    public string EventLocationTagName = "";
}


public class CreateEventViewModel
{
    public string EventTitle { get; set; } = "";
    public string EventDescription { get; set; } = "";
    public Guid EventCategoryTagId { get; set; } = new();
    public List<Location> LocationTags { get; set; } = [];
    public Guid EventLocationTagId { get; set; } = new();
    public int MaxParticipant { get; set; } = 0;
    public int Cost { get; set; } = 0;
    public DateTime EventDate { get; set; } = new();
    public DateTime PostExpiryDate { get; set; } = new();
    public List<Tag> Tags { get; set; } = [];
    public List<string> Questions { get; set; } = [];
}

[Table("REGISTRATION")]
public class Registration : BaseModel
{
    [PrimaryKey]
    [Column("RegistrationId")]
    public Guid RegistrationId { get; set; } = new();

    [Column("UserId")]
    public Guid UserId { get; set; } = new();

    [Column("EventId")]
    public Guid EventId { get; set; } = new();

    [Column("Status")]
    public string Status { get; set; } = "pending";

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}

[Table("REGISTRATION_ANSWER")]
public class RegistrationAnswer : BaseModel
{
    [PrimaryKey]
    [Column("RegistrationAnswerId")]
    public Guid RegistrationAnswerId { get; set; } = new();

    [Column("RegistrationId")]
    public Guid RegistrationId { get; set; } = new();

    [Column("EventQuestionId")]
    public Guid EventQuestionId { get; set; } = new();

    [Column("Answer")]
    public string Answer { get; set; } = "";

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class EventManagementViewModel : EventBriefCardData 
{
    public List<QuestionViewModel> Questions { get; set; } = [];
    public List<EventManagementAnswerCollection> AnswerSet { get; set; } = [];
    public List<BriefUser> ConfirmedParticipant { get; set; } = [];
    public List<BriefUser> AwaitingConfirmationParticipant { get; set; } = [];
    public string EventStatus { get; set; } = "";
}

public class RegistrationAnswerSimplify
{
    public Guid EventQuestionId { get; set; } = new();
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
}

public class EventManagementAnswerCollection
{
    public User User { get; set; } = new();
    public string Status { get; set; } = "pending";
    public List<RegistrationAnswerSimplify> RegistrationAnswers { get; set; } = [];
}

public class EventStatusViewModel
{
    public EventDetailCardData EventDetailCardData { get; set; } = new();
    public DateTime RegistrationTime { get; set; } = new();
    public string Status { get; set; } = "";
}