using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;

namespace Citlali.Models;

public class Event : BaseModel {
    public Guid EventId = new();
    public Guid CreatorUserId = new();
    public string EventTitle = "Sample title";
    public string EventDescription = "Sample description";
    public Guid EventCategoryTagId = new();
    public Guid EventLocationTagId = new();
    public int MaxParticipant = 64;
    public int Cost = 64;
    public DateTime EventDate = new();
    public DateTime PostExpiryDate = new();
    public DateTime CreatedAt = new();
}

public class EventQuestion : BaseModel {
    public Guid EventQuestionId = new();
    public Guid EventId = new();
    public string Question = "Question";
}

public class EventCategoryTag {
    public Guid EventCategoryTagId = new();
    public string EventCategoryTagName = "FOOD!!!!";
    public string EventCategoryTagEmoji = "😋";
}

public class EventLocationTag {
    public Guid EventLocationTagId = new();
    public string EventLocationTagName = "Lat Krabang";
}

public class EventDetailViewModel {
    public Guid EventId = new();
    public string EventTitle = "Basketball? Anyone?";
    public string EventDescription = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
    public string CreatorDisplayName = "John Basketball";
    public EventLocationTag LocationTag = new();
    public EventCategoryTag EventCategoryTag = new();
    public int CurrentParticipant = 32;
    public int MaxParticipant = 64;
    public int Cost = 64;
    public DateTime EventDate = new(2024, 12, 31);
    public DateTime PostExpiryDate = new(2024, 12, 30);
    public DateTime CreatedAt = new(2024, 12, 3);

    public List<QuestionViewModel> Questions { get; set; } = [new()];
}

public class QuestionViewModel
{
    public Guid EventQuestionId { get; set; }
    public string Question { get; set; } = "Question Question";
    [Required(ErrorMessage = "This field is required")]
    public string Answer { get; set; } = "Answer Answer";
}