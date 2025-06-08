using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    // ===============================
    // ACCOUNT SECURITY
    // ===============================

    /// <summary>
    /// Account lockout tracking
    /// </summary>
    public class AccountLockout
    {
        [Key]
        public Guid LockoutId { get; set; }

        public Guid UserId { get; set; }

        public DateTime LockoutStartDate { get; set; } = DateTime.UtcNow;

        public DateTime? LockoutEndDate { get; set; }

        [StringLength(50)]
        public string LockoutReason { get; set; } = string.Empty;

        public int FailedAttemptCount { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string? UnlockedBy { get; set; }

        public DateTime? UnlockedDate { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
