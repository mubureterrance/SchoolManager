using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    /// <summary>
    /// Parent/Guardian profile extending ApplicationUser
    /// </summary>
    public class Parent : IAuditableEntity
    {
        [Key]
        public Guid ParentId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(20)]
        public string Relationship { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Occupation { get; set; }

        [StringLength(500)]
        public string? WorkAddress { get; set; }

        [StringLength(20)]
        public string? WorkPhoneNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AnnualIncome { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
