using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    // ===============================
    // SUPPORTING ENTITIES
    // ===============================

    /// <summary>
    /// Class/Grade information
    /// </summary>
    public class Class: IAuditableEntity
    {
        [Key]
        public Guid ClassId { get; set; }

        [Required]
        [StringLength(50)]
        public string ClassName { get; set; } = string.Empty;

        [StringLength(10)]
        public string? Section { get; set; }

        public int? Capacity { get; set; }

        [StringLength(20)]
        public string AcademicYear { get; set; } = string.Empty;

        public Guid? ClassTeacherId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("ClassTeacherId")]
        public virtual Staff? ClassTeacher { get; set; }
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<FeeStructure> FeeStructures { get; set; } = new List<FeeStructure>();

    }
}
