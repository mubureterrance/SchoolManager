using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    // ===============================
    // ROLE MODELS
    // ===============================

    /// <summary>
    /// Extended Identity Role with additional properties
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>
    {
        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsSystemRole { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Navigation Properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
