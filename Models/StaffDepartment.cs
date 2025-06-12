using Microsoft.EntityFrameworkCore;
using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    /// <summary>
    /// Junction table for Staff-Department many-to-many relationship
    /// </summary>
    public class StaffDepartment : IAuditableEntity
    {
        [Key]
        public Guid StaffDepartmentId { get; set; }

        public Guid StaffId { get; set; }
        public Guid DepartmentId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool IsPrimary { get; set; } = false; // Primary department affiliation
        public bool IsActive { get; set; } = true;

        [StringLength(200)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(StaffId))]
        public virtual Staff Staff { get; set; } = null!;

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department Department { get; set; } = null!;
    }
}
