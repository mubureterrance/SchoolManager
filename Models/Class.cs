using System.ComponentModel.DataAnnotations;

namespace SchoolManager.Models
{
    // ===============================
    // SUPPORTING ENTITIES
    // ===============================

    /// <summary>
    /// Class/Grade information
    /// </summary>
    public class Class
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

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Staff? ClassTeacher { get; set; }
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
