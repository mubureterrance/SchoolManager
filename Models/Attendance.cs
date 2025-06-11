using SchoolManager.Enums;
using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class Attendance: IAuditableEntity
    {
        [Key]
        public Guid AttendanceId { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        [Required]
        public DateTime AttendanceDate { get; set; }

        [Required]
        [StringLength(10)]
        public AttendanceStatus Status { get; set; } // Present, Absent, Late

        [StringLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Navigation Properties
        public virtual Student Student { get; set; } = null!;

        public virtual Class? Class { get; set; } = null!;
    }
}
