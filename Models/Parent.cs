using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    /// <summary>
    /// Parent/Guardian profile extending ApplicationUser
    /// </summary>
    public class Parent
    {
        [Key]
        public Guid ParentId { get; set; }

        public Guid UserId { get; set; }

        [StringLength(50)]
        public string? Occupation { get; set; }

        [StringLength(500)]
        public string? WorkAddress { get; set; }

        [StringLength(20)]
        public string? WorkPhoneNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AnnualIncome { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
    }
}
