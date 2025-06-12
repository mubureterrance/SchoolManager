using Microsoft.EntityFrameworkCore;
using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    /// <summary>
    /// Department entity
    /// </summary>
    [Index(nameof(Code), IsUnique = true)]
    public class Department: IAuditableEntity
    {
        [Key]
        public Guid DepartmentId { get; set; }

        [Required]
        [StringLength(10)]
        public string Code { get; set; } = string.Empty; // e.g., "MATH", "ENG", "SCI"

        [Required, StringLength(100)]
        public string Name { get; set; } = null!;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [ForeignKey("HeadOfDepartment")]
        public Guid HeadOfDepartmentId { get; set; }

        [ForeignKey(nameof(HeadOfDepartmentId))]
        public virtual Staff HeadOfDepartment { get; set; } = null!;

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? PhoneExtension { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
        public virtual ICollection<StaffDepartment> StaffDepartments { get; set; } = new List<StaffDepartment>();
    }

}
