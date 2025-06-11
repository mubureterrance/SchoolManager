using Microsoft.AspNetCore.Identity;
using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    /// <summary>
    /// Junction table for User-Role many-to-many relationship
    /// </summary>
    public class UserRole : IdentityUserRole<Guid>, IAuditableEntity
    {
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? AssignedBy { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? ExpiryDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ApplicationRole Role { get; set; } = null!;
    }
}
