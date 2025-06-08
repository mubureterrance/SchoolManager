using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    // ===============================
    // AUTHENTICATION HISTORY
    // ===============================

    /// <summary>
    /// Login history tracking
    /// </summary>
    public class UserLoginHistory
    {
        [Key]
        public Guid LoginHistoryId { get; set; }

        public Guid UserId { get; set; }

        public DateTime LoginDate { get; set; } = DateTime.UtcNow;

        public DateTime? LogoutDate { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(500)]
        public string? UserAgent { get; set; }

        [StringLength(100)]
        public string? DeviceInfo { get; set; }

        [StringLength(50)]
        public string LoginResult { get; set; } = string.Empty; // Success, Failed, Locked, etc.

        [StringLength(500)]
        public string? FailureReason { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
