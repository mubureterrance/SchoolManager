using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    // ===============================
    // SESSION MANAGEMENT
    // ===============================

    /// <summary>
    /// User session management for tracking active sessions
    /// </summary>
    public class UserSession
    {
        [Key]
        public Guid SessionId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string SessionToken { get; set; } = string.Empty;

        [StringLength(500)]
        public string? RefreshToken { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime ExpiryDate { get; set; }

        public DateTime? RefreshTokenExpiryDate { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(100)]
        public string? DeviceInfo { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastActivityDate { get; set; }

        public DateTime? LogoutDate { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
