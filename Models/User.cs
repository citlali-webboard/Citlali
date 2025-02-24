using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Citlali.Models;

[Table("USERS")]
public class User : BaseModel
{
        [PrimaryKey]
        [Column("UserId")]
        public Guid UserId { get; set; }

        [Column("Email")]
        public string Email { get; set; } = "";

        [Column("Username")]
        public string Username { get; set; } = "";

        [Column("ProfileImageURL")]
        public string ProfileImageUrl { get; set; } = "";

        [Column("DisplayName")]
        public string DisplayName { get; set; } = "";

        [Column("UserBio")]
        public string UserBio { get; set; } = "";

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Deleted")]
        public bool Deleted { get; set; } = false;

}

[Table("USER_FOLLOWED_CATEGORY")]
public class UserFollowedCategory : BaseModel
{
        [PrimaryKey]
        [Column("UserFollowedTagId")]
        public Guid UserFollowedTagId { get; set; } = Guid.NewGuid();

        [Column("UserId")]
        public Guid UserId { get; set; }

        [Column("EventCategoryTagId")]
        public Guid EventCategoryTagId { get; set; }
}

[Table("USER_FOLLOWED")]
public class UserFollowed : BaseModel
{
    [PrimaryKey]
    [Column("FollowingId")]
    public Guid FollowingId { get; set; } = Guid.NewGuid();

    [Column("FollowerUserId")]
    public Guid FollowerUserId { get; set; }

    [Column("FollowedUserId")]
    public Guid FollowedUserId { get; set; }
}

public class UserOnboardingDto : User
{
        public IFormFile? ProfileImage { get; set; }
        public List<Guid> SelectedTags { get; set; } = new List<Guid>();
}

public class UserViewModel : User
{
        public int FollowingCount { get; set; }
        public int FollowersCount { get; set; }
        public bool IsCurrentUser { get; set; }
        public List<EventBriefCardData> UserEvents { get; set; } = new List<EventBriefCardData>();
}

public class BriefUser 
{
        public Guid UserId { get; set; } = new();
        public string Username { get; set; } = "";
        public string ProfileImageUrl { get; set; } = "";
        public string DisplayName { get; set; } = "";
}