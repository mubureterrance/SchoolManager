using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace SchoolManager.Models
{
    // ===============================
    // SCHOOL-SPECIFIC USER TYPES
    // ===============================

    /// <summary>
    /// Student profile extending ApplicationUser
    /// </summary>
    public class Student : IAuditableEntity
    {
        [Key]
        public Guid StudentId { get; set; }

        public Guid UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string StudentNumber { get; set; } = string.Empty;

        public DateTime AdmissionDate { get; set; }

        public Guid? ClassId { get; set; }

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
        public DateTime CreatedDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey("ClassId")]
        public virtual Class? Class { get; set; }
        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
        public virtual ICollection<FeePayment> FeePayments { get; set; } = new List<FeePayment>();
    }

}
