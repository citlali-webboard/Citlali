namespace Citlali.Models
{
    public class FollowViewModel
    {
        public User? User { get; set; }
        public List<BriefUser> Users { get; set; } = new List<BriefUser>();
        public List<Tag> Tags { get; set; } = new List<Tag>();
    }
}