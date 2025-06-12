using SchoolManager.Enums;

namespace SchoolManager.DTOs
{
    // ===============================
    // PARENT DTOs
    // ===============================

    /// <summary>
    /// DTO for creating a new parent
    /// </summary>
    public class CreateParentDto
    {
        public Guid UserId { get; set; }
        public string? Occupation { get; set; }
        public string? WorkAddress { get; set; }
        public string? WorkPhoneNumber { get; set; }
        public decimal? AnnualIncome { get; set; }
    }

    /// <summary>
    /// DTO for updating parent information
    /// </summary>
    public class UpdateParentDto
    {
        public string? Occupation { get; set; }
        public string? WorkAddress { get; set; }
        public string? WorkPhoneNumber { get; set; }
        public decimal? AnnualIncome { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// DTO for parent response data
    /// </summary>
    public class ParentDto
    {
        public Guid ParentId { get; set; }
        public Guid UserId { get; set; }
        public string? Occupation { get; set; }
        public string? WorkAddress { get; set; }
        public string? WorkPhoneNumber { get; set; }
        public decimal? AnnualIncome { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // User information
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? AlternatePhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    /// <summary>
    /// DTO for parent summary information
    /// </summary>
    public class ParentSummaryDto
    {
        public Guid ParentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Occupation { get; set; }
        public bool IsActive { get; set; }
        public int NumberOfChildren { get; set; }
    }

    /// <summary>
    /// DTO for parent with their children information
    /// </summary>
    public class ParentWithChildrenDto : ParentDto
    {
        public List<ParentChildDto> Children { get; set; } = new List<ParentChildDto>();
    }

    /// <summary>
    /// DTO for child information in parent context
    /// </summary>
    public class ParentChildDto
    {
        public Guid StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public ParentRelationship RelationshipToStudent { get; set; }
        public bool IsPrimaryContact { get; set; }
        public bool IsEmergencyContact { get; set; }
        public bool CanPickupStudent { get; set; }
        public string? ClassName { get; set; }
        public bool IsActive { get; set; }
    }
}
