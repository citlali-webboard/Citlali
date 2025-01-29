using System;
using System.Collections.Generic;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace Citlali.Models;

[Table("users")]
public class User : BaseModel
{
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("userId")]
        public Guid UserId { get; set; } // Linked to Supabase Auth

        [Column("email")]
        public string Email { get; set; } = "";

        [Column("displayName")]
        public string DisplayName { get; set; } = "";

        [Column("profileImageURL")]
        public string ProfileImageUrl { get; set; } = "";

        [Column("userBio")]
        public string UserBio { get; set; } = "";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public IFormFile? ProfileImage { get; set; } // Used for handling file upload in form
}
