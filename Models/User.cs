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
        public Guid UserId { get; set; } // Linked to Supabase Auth

        [Column("Email")]
        public string Email { get; set; } = "";

        [Column("ProfileImageURL")]
        public string ProfileImageUrl { get; set; } = Environment.GetEnvironmentVariable("DEFAULT_PROFILE_IMAGE_URL") ?? "";

        [Column("DisplayName")]
        public string DisplayName { get; set; } = "";

        [Column("UserBio")]
        public string UserBio { get; set; } = "";

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("Deleted")]
        public bool Deleted { get; set; } = false;

}

public class UserRegisterDTO : User 
{
        public string Password { get; set; } = "";
        public IFormFile? ProfileImage { get; set; }
}
