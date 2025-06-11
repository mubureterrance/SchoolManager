using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    /// <summary>
    /// Staff profile extending ApplicationUser
    /// </summary>
    public class Staff : IAuditableEntity
    {
        [Key]
        public Guid StaffId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(100)]
        public string? Position { get; set; }

        public DateTime HireDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Salary { get; set; }

        [StringLength(1000)]
        public string? Qualifications { get; set; }

        [StringLength(500)]
        public string? Specializations { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? TerminationDate { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
