using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    // ===============================
    // TWO-FACTOR AUTHENTICATION
    // ===============================

    /// <summary>
    /// Two-factor authentication tokens
    /// </summary>
    public class TwoFactorToken
    {
        [Key]
        public Guid TokenId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [StringLength(10)]
        public string Token { get; set; } = string.Empty;

        [StringLength(20)]
        public string TokenType { get; set; } = string.Empty; // SMS, Email, Authenticator

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedDate { get; set; }

        public int AttemptCount { get; set; } = 0;

        [StringLength(45)]
        public string? IpAddress { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
    }

}
