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
    public string EventTitle { get; set; } = "Sample title";

    [Column("EventDescription")]
    public string EventDescription { get; set; } = "Sample description";

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
    public DateTime CreatedAt { get; set; } = new();

    [Column("Deleted")]
    public bool Deleted { get; set; } = false;


}

public class EventQuestion : BaseModel
{
    public Guid EventQuestionId = new();
    public Guid EventId = new();
    public string Question = "Question";
}

public class EventLocationTag
{
    public Guid EventLocationTagId = new();
    public string EventLocationTagName = "Lat Krabang";
}

public class EventBriefCardData
{
    public Guid EventId = new();
    public string EventTitle = "Basketball? Anyone?";
    public string CreatorDisplayName = "John Basketball";
    public EventLocationTag LocationTag = new();
    public EventCategoryTag EventCategoryTag = new();
    public int CurrentParticipant = 32;
    public int MaxParticipant = 64;
    public int Cost = 64;
    public DateTime EventDate = new(2024, 12, 31);
    public DateTime PostExpiryDate = new(2024, 12, 30);
    public DateTime CreatedAt = new(2024, 12, 3);
}

public class EventDetailCardData : EventBriefCardData
{
    public string EventDescription = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
}

public class EventDetailViewModel
{
    public EventDetailCardData EventDetailCardData = new();
    public List<QuestionViewModel> Questions { get; set; } = [new()];
}

public class QuestionViewModel
{
    public Guid EventQuestionId { get; set; }
    public string Question { get; set; } = "Question Question";
    [Required(ErrorMessage = "This field is required")]
    public string Answer { get; set; } = "Answer Answer";
}

public class EventExploreViewModel
{
    public EventBriefCardData[] EventBriefCardDatas = [new()];
}

[Table("EVENT_CATEGORY_TAG")]
public class EventCategoryTag : BaseModel {
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

public class Tag{
    public Guid TagId = new();
    public string TagEmoji = "";
    public string TagName = "";
}

[Table("LOCATION_TAG")]
public class LocationTag : BaseModel {
    [PrimaryKey]
    [Column("LocationTagId")]
    public Guid EventLocationTagId { get; set; } = new();

    [Column("LocationTagName")]
    public string EventLocationTagName { get; set; } = "";

    [Column("Deleted")]
    public bool Deleted { get; set; } = false;
}

public class Location{
    public Guid EventLocationTagId = new();
    public string EventLocationTagName = "";
}


public class CreateEventViewModel {
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
}