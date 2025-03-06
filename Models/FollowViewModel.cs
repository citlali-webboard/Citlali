namespace Citlali.Models
{
    public class FollowViewModel
    {
        public User? User { get; set; }
        public bool IsCurrentUser { get; set; } = false;
        public List<BriefUser> Followers { get; set; } = new List<BriefUser>();
        public FollowingModel Following { get; set; } = new FollowingModel();
    }

    public class FollowingModel
    {
        public List<BriefUser> FollowingUsers { get; set; } = new List<BriefUser>();
        public List<Tag> FollowedTags { get; set; } = new List<Tag>();
    }
}