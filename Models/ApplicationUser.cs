using Microsoft.AspNetCore.Identity;
using SchoolManager.Enums;
using SchoolManager.Models.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolManager.Models
{
    // ===============================
    // BASE USER MODEL
    // ===============================

    /// <summary>
    /// Extended Identity User with additional properties for school management
    /// </summary>
    public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? MiddleName { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();

        [StringLength(10)]
        public Gender? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }

        [StringLength(20)]
        public string? AlternatePhoneNumber { get; set; }

        [StringLength(500)]
        public string? ProfilePictureUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [StringLength(100)]
        public string? LastModifiedBy { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public bool IsFirstLogin { get; set; } = true;

        public bool RequirePasswordChange { get; set; } = true;
        public DateTime? LastPasswordChangeDate { get; set; }

        // Navigation Properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<UserSession> UserSessions { get; set; } = new List<UserSession>();
        public virtual ICollection<UserLoginHistory> LoginHistory { get; set; } = new List<UserLoginHistory>();
        public virtual ICollection<PasswordHistory> PasswordHistory { get; set; } = new List<PasswordHistory>();
        public virtual Student? Student { get; set; }
        public virtual Staff? Staff { get; set; }
        public virtual Parent? Parent { get; set; }
    }
}
