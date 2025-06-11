using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class Subject : IAuditableEntity
    {
        [Key]
        public Guid SubjectId { get; set; }

        [Required]
        [StringLength(100)]
        public string SubjectName { get; set; } = null!;

        [Required]
        [StringLength(10)]
        public string SubjectCode { get; set; } = null!;

        public int Credits { get; set; }

        public Guid DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
    }
}
