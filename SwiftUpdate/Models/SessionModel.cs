using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwiftUpdate.Models
{
    public class SessionModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SessionId { get; set; }

        [Required]
        [StringLength(255)]
        public string SessionGuid { get; set; } // Unique identifier for the session

        [Required]
        public int UserId { get; set; } // Associated user ID

        [Required]
        public DateTime ExpiryTime { get; set; } // Session expiration time

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Timestamp when session was created

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now; // Timestamp when session was last updated


        // Navigation property for the associated user
        [ForeignKey("UserId")]
        public virtual UserModel User { get; set; }
    }
}
