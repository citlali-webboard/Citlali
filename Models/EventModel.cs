using System.ComponentModel.DataAnnotations;
using Supabase.Postgrest.Models;

namespace Citlali.Models;

public class Event : BaseModel {
    public Guid EventId = new Guid();
    public Guid CreatorUserId = new Guid();
    public string EventTitle = "Sample title";
    public string EventDescription = "Sample description";
    public Guid EventCategoryTagId = new Guid();
    public Guid EventLocationTagId = new Guid();
    public int MaxParticipant = 64;
    public int Cost = 64;
    public DateTime EventDate = new DateTime();
    public DateTime PostExpiryDate = new DateTime();
    public DateTime CreatedAt = new DateTime();
}

public class EventQuestion : BaseModel {
    public Guid EventQuestionId = new Guid();
    public Guid EventId = new Guid();
    public string Question = "Question";
}

public class EventDetail {
    public Event Event = new();
    public EventQuestion[] EventQuestions = [new(), new()];
}

public class EventResponseViewModel
{
    public List<QuestionResponse> Responses { get; set; } = [];
}

public class QuestionResponse
{
    public Guid EventQuestionId { get; set; }

    [Required(ErrorMessage = "This field is required")]
    public string Answer { get; set; } = "";
}