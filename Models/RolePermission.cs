using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    /// <summary>
    /// Role-Permission junction table
    /// </summary>
    public class RolePermission
    {
        [Key]
        public Guid RolePermissionId { get; set; }

        public Guid RoleId { get; set; }

        public Guid PermissionId { get; set; }

        public DateTime GrantedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? GrantedBy { get; set; }

        // Navigation Properties
        public virtual ApplicationRole Role { get; set; } = null!;
        public virtual Permission Permission { get; set; } = null!;
    }
}
