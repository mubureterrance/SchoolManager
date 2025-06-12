using SchoolManager.Enums;

namespace SchoolManager.DTOs
{
    // ===============================
    // STUDENT PARENT DTOs
    // ===============================

    /// <summary>
    /// DTO for creating a student-parent relationship
    /// </summary>
    public class CreateStudentParentDto
    {
        public Guid StudentId { get; set; }
        public Guid ParentId { get; set; }
        public ParentRelationship RelationshipToStudent { get; set; }
        public bool IsPrimaryContact { get; set; } = false;
        public bool IsEmergencyContact { get; set; } = false;
        public bool CanPickupStudent { get; set; } = true;
    }

    /// <summary>
    /// DTO for updating student-parent relationship
    /// </summary>
    public class UpdateStudentParentDto
    {
        public ParentRelationship RelationshipToStudent { get; set; }
        public bool IsPrimaryContact { get; set; }
        public bool IsEmergencyContact { get; set; }
        public bool CanPickupStudent { get; set; }
    }

    /// <summary>
    /// DTO for student-parent relationship response
    /// </summary>
    public class StudentParentDto
    {
        public Guid StudentParentId { get; set; }
        public Guid StudentId { get; set; }
        public Guid ParentId { get; set; }
        public ParentRelationship RelationshipToStudent { get; set; }
        public bool IsPrimaryContact { get; set; }
        public bool IsEmergencyContact { get; set; }
        public bool CanPickupStudent { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }

        // Student information
        public string StudentNumber { get; set; } = string.Empty;
        public string StudentFullName { get; set; } = string.Empty;
        public string? StudentEmail { get; set; }
        public string? ClassName { get; set; }
        public bool StudentIsActive { get; set; }

        // Parent information
        public string ParentFullName { get; set; } = string.Empty;
        public string? ParentEmail { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public string? ParentOccupation { get; set; }
        public bool ParentIsActive { get; set; }
    }

    /// <summary>
    /// DTO for student with parents information
    /// </summary>
    public class StudentWithParentsDto
    {
        public Guid StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? ClassName { get; set; }
        public bool IsActive { get; set; }
        public List<StudentParentRelationshipDto> Parents { get; set; } = new List<StudentParentRelationshipDto>();
    }

    /// <summary>
    /// DTO for parent with students information
    /// </summary>
    public class ParentWithStudentsDto
    {
        public Guid ParentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Occupation { get; set; }
        public bool IsActive { get; set; }
        public List<ParentStudentRelationshipDto> Students { get; set; } = new List<ParentStudentRelationshipDto>();
    }

    /// <summary>
    /// DTO for parent relationship from student perspective
    /// </summary>
    public class StudentParentRelationshipDto
    {
        public Guid StudentParentId { get; set; }
        public Guid ParentId { get; set; }
        public string ParentFullName { get; set; } = string.Empty;
        public string? ParentEmail { get; set; }
        public string? ParentPhoneNumber { get; set; }
        public string? ParentWorkPhoneNumber { get; set; }
        public string? ParentOccupation { get; set; }
        public ParentRelationship RelationshipToStudent { get; set; }
        public bool IsPrimaryContact { get; set; }
        public bool IsEmergencyContact { get; set; }
        public bool CanPickupStudent { get; set; }
        public bool ParentIsActive { get; set; }
    }

    /// <summary>
    /// DTO for student relationship from parent perspective
    /// </summary>
    public class ParentStudentRelationshipDto
    {
        public Guid StudentParentId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string StudentFullName { get; set; } = string.Empty;
        public string? StudentEmail { get; set; }
        public string? ClassName { get; set; }
        public ParentRelationship RelationshipToStudent { get; set; }
        public bool IsPrimaryContact { get; set; }
        public bool IsEmergencyContact { get; set; }
        public bool CanPickupStudent { get; set; }
        public bool StudentIsActive { get; set; }
    }

    /// <summary>
    /// DTO for emergency contact information
    /// </summary>
    public class EmergencyContactDto
    {
        public Guid ParentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WorkPhoneNumber { get; set; }
        public string? AlternatePhoneNumber { get; set; }
        public ParentRelationship RelationshipToStudent { get; set; }
        public bool IsPrimaryContact { get; set; }
        public bool CanPickupStudent { get; set; }
        public string? WorkAddress { get; set; }
    }

    /// <summary>
    /// DTO for contact permissions
    /// </summary>
    public class ContactPermissionsDto
    {
        public Guid StudentParentId { get; set; }
        public bool IsPrimaryContact { get; set; }
        public bool IsEmergencyContact { get; set; }
        public bool CanPickupStudent { get; set; }
    }
}
