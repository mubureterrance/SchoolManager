using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class ExamResult : IAuditableEntity
    {
        [Key]
        public Guid ResultId { get; set; }

        [Required]
        public Guid ExamId { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid SubjectId { get; set; }

        [Required]
        public int MarksObtained { get; set; }

        [Required]
        public int MaxMarks { get; set; }

        [StringLength(5)]
        public string? Grade { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("ExamId")]
        public virtual Examination Exam { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("SubjectId")]
        public virtual Subject Subject { get; set; } = null!;
    }
}
