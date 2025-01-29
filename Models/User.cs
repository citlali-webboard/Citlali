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
        [Column("userId")]
        public Guid UserId { get; set; } // Linked to Supabase Auth

        [Column("email")]
        public string Email { get; set; } = "";

        [Column("profileImageURL")]
        public string ProfileImageUrl { get; set; } = Environment.GetEnvironmentVariable("DEFAULT_PROFILE_IMAGE_URL") ?? "";

        [Column("displayName")]
        public string DisplayName { get; set; } = "";

        [Column("userBio")]
        public string UserBio { get; set; } = "";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("deleted")]
        public bool Deleted { get; set; } = false;

}

public class UserRegisterDTO : User 
{
        public string Password { get; set; } = "";
        public IFormFile? ProfileImage { get; set; }
}
