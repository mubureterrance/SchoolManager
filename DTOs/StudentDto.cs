using SchoolManager.Enums;
using System.ComponentModel.DataAnnotations;

namespace SchoolManager.DTOs
{
    public class StudentDto
    {
        public Guid StudentId { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public DateTime AdmissionDate { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string? GuardianName { get; set; }
        public string? MedicalInfo { get; set; }
        public string? EmergencyContact { get; set; }
        public bool IsActive { get; set; }
        public List<StudentParentDto> Parents { get; set; } = new();

    }

    public class CreateStudentDto: CreateUserRequest
    {
        [Required]
        public DateTime AdmissionDate { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        public Guid? GuardianId { get; set; }

        [StringLength(1000)]
        public string? MedicalInfo { get; set; }

        [StringLength(500)]
        public string? EmergencyContact { get; set; }

        [StringLength(100)]
        public string? BloodGroup { get; set; }

        [StringLength(500)]
        public string? Allergies { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? GraduationDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }
    }

    public class UpdateStudentDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        public Guid ClassId { get; set; }

        public Guid? GuardianId { get; set; }

        [StringLength(1000)]
        public string? MedicalInfo { get; set; }

        [StringLength(500)]
        public string? EmergencyContact { get; set; }

        public bool IsActive { get; set; }
    }

    public class StudentAttendanceDto
    {
        public Guid AttendanceId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string ClassName { get; set; } = string.Empty;
    }

    public class StudentGradeDto
    {
        public Guid ResultId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int MarksObtained { get; set; }
        public int MaxMarks { get; set; }
        public string Grade { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
    }

    public class StudentParentDto
    {
        public string? FullName { get; set; }
        public ParentRelationship? Relationship { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; } = null;
        public bool IsPrimaryContact { get; set; }
    }
}
