using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    /// <summary>
    /// User-specific permissions (overrides)
    /// </summary>
    public class UserPermission : IAuditableEntity
    {
        [Key]
        public Guid UserPermissionId { get; set; }

        public Guid UserId { get; set; }

        public Guid PermissionId { get; set; }

        public bool IsGranted { get; set; } = true; // false for explicitly denied permissions

        public DateTime GrantedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? GrantedBy { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}
