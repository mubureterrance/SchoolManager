using SchoolManager.Enums;
using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    /// <summary>
    /// Student-Parent relationship junction table
    /// </summary>
    public class StudentParent : IAuditableEntity
    {
        [Key]
        public Guid StudentParentId { get; set; }

        public Guid StudentId { get; set; }

        public Guid ParentId { get; set; }

        public ParentRelationship RelationshipToStudent { get; set; }

        public bool IsPrimaryContact { get; set; } = false;

        public bool IsEmergencyContact { get; set; } = false;

        public bool CanPickupStudent { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        // Navigation Properties
        public virtual Student Student { get; set; } = null!;
        public virtual Parent Parent { get; set; } = null!;
    }
}
