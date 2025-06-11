using SchoolManager.Enums;
using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    public class FeePayment : IAuditableEntity
    {
        [Key]
        public Guid PaymentId { get; set; }

        [Required]
        public Guid StudentId { get; set; }

        [Required]
        public Guid FeeStructureId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountPaid { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [StringLength(50)]
        public PaymentMethod Method { get; set; }

        [StringLength(100)]
        public string? TransactionId { get; set; }

        [Required]
        [StringLength(20)]
        public PaymentStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Navigation Properties
        public virtual Student Student { get; set; } = null!;
        public virtual FeeStructure FeeStructure { get; set; } = null!;

    }
}
