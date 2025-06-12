using Microsoft.EntityFrameworkCore;
using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    /// <summary>
    /// Staff profile extending ApplicationUser
    /// </summary>
    /// <summary>
    /// Updated Staff model without direct department reference
    /// </summary>
    [Index(nameof(EmployeeNumber), IsUnique = true)]
    [Index(nameof(IsActive))]
    public class Staff : IAuditableEntity
    {
        [Key]
        public Guid StaffId { get; set; }

        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Employee number must contain only uppercase letters and numbers")]
        public string EmployeeNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Position { get; set; }

        public DateTime HireDate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Salary must be positive")]
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
        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<StaffDepartment> StaffDepartments { get; set; } = new List<StaffDepartment>();

        // Computed Properties
        //public StaffDepartment? PrimaryDepartment =>
        //    StaffDepartments?.FirstOrDefault(sd => sd.IsPrimary && sd.IsActive);

        //public IEnumerable<StaffDepartment> ActiveDepartments =>
        //    StaffDepartments?.Where(sd => sd.IsActive) ?? Enumerable.Empty<StaffDepartment>();

    }
}
