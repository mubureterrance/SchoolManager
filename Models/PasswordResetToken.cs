using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    /// <summary>
    /// Password reset tokens
    /// </summary>
    public class PasswordResetToken
    {
        [Key]
        public Guid TokenId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string Token { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedDate { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
