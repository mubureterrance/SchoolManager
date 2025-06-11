using SchoolManager.Enums;
using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class Examination : IAuditableEntity
    {
        [Key]
        public Guid ExamId { get; set; }

        [Required]
        [StringLength(100)]
        public string ExamName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string ExamType { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public ExamStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        // Navigation Properties
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; } = null!;

        public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    }
}
