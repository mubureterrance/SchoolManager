using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SchoolManager.Models
{
    // ===============================
    // SCHOOL-SPECIFIC USER TYPES
    // ===============================

    /// <summary>
    /// Student profile extending ApplicationUser
    /// </summary>
    public class Student
    {
        [Key]
        public Guid StudentId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string StudentNumber { get; set; } = string.Empty;

        public DateTime AdmissionDate { get; set; }

        public Guid? CurrentClassId { get; set; }

        [StringLength(500)]
        public string? MedicalInfo { get; set; }

        [StringLength(500)]
        public string? EmergencyContact { get; set; }

        [StringLength(100)]
        public string? BloodGroup { get; set; }

        [StringLength(500)]
        public string? Allergies { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? GraduationDate { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Class? CurrentClass { get; set; }
        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
    }

}
