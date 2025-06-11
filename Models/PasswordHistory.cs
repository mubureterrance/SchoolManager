using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    // ===============================
    // PASSWORD MANAGEMENT
    // ===============================

    /// <summary>
    /// Password history for enforcing password policies
    /// </summary>
    public class PasswordHistory : IAuditableEntity
    {
        [Key]
        public Guid PasswordHistoryId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [StringLength(500)]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
