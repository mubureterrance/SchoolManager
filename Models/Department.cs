using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class Department: IAuditableEntity
    {
        [Key]
        public Guid DepartmentId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } = null!;

        [Required]
        public Guid HeadOfDepartmentId { get; set; }

        [ForeignKey("HeadOfDepartmentId")]
        public virtual Staff HeadOfDepartment { get; set; } = null!;

        [StringLength(100)]
        public string? Email { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    }

}
