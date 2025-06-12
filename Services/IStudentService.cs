using SchoolManager.DTOs;

namespace SchoolManager.Services
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentDto>> GetAllStudentsAsync(Guid? classId = null, bool? isActive = null);
        Task<StudentDto?> GetStudentByIdAsync(Guid studentId);
        Task<StudentDto?> GetStudentByNumberAsync(string studentNumber);
        Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto);
        Task<bool> UpdateStudentAsync(Guid studentId, UpdateStudentDto updateStudentDto);
        Task<bool> DeleteStudentAsync(Guid studentId);
        Task<bool> DeactivateStudentAsync(Guid studentId);
        Task<IEnumerable<StudentAttendanceDto>> GetStudentAttendanceAsync(Guid studentId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<IEnumerable<StudentGradeDto>> GetStudentGradesAsync(Guid studentId, string? academicYear = null);
        Task<bool> TransferStudentAsync(Guid studentId, Guid newClassId);
        Task<int> GetStudentCountByClassAsync(Guid classId);
        Task<IEnumerable<StudentDto>> SearchStudentsAsync(string searchTerm);
    }
}
