using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.DTOs;
using SchoolManager.Models;
using SchoolManager.Services;
using System.Security.Cryptography;
using System.Text;

namespace SchoolManager.Services.Implementations
{
    public class StudentService : IStudentService
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<StudentService> _logger;

        public StudentService(SchoolManagementDbContext context, ILogger<StudentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync(Guid? classId = null, bool? isActive = null)
        {
            try
            {
                var query = _context.Students
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .Include(s => s.StudentParents)
                        .ThenInclude(g => g.Parent)
                    .AsQueryable();

                if (classId.HasValue)
                    query = query.Where(s => s.ClassId == classId.Value);

                if (isActive.HasValue)
                    query = query.Where(s => s.IsActive == isActive.Value);

                var students = await query
                    .OrderBy(s => s.User.FirstName)
                    .ThenBy(s => s.User.LastName)
                    .Select(s => new StudentDto
                    {
                        StudentId = s.StudentId,
                        StudentNumber = s.StudentNumber,
                        FirstName = s.User.FirstName,
                        LastName = s.User.LastName,
                        Email = s.User.Email ?? "N/A",
                        PhoneNumber = s.User.PhoneNumber,
                        Address = s.User.Address,
                        AdmissionDate = s.AdmissionDate,
                        ClassName = s.Class!.ClassName ?? "N/A",
                        //GuardianName = s.StudentParents != null ? s.StudentParents.FirstOrDefault(p : null,
                        MedicalInfo = s.MedicalInfo,
                        EmergencyContact = s.EmergencyContact,
                        IsActive = s.IsActive
                    })
                    .ToListAsync();

                return students;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students");
                throw;
            }
        }

        public async Task<StudentDto?> GetStudentByIdAsync(Guid studentId)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .Include(s => s.StudentParents)
                        .ThenInclude(g => g.Parent)
                            .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (student == null)
                    return null;

                var guardianName = student.StudentParents
                                            .OrderByDescending(p => p.IsPrimaryContact)
                                            .Select(p => p.Parent?.User?.FullName)
                                            .FirstOrDefault(name => !string.IsNullOrEmpty(name));

                var parents = student.StudentParents.Select(p => new StudentParentDto
                                            {
                                                FullName = p.Parent?.User?.FullName,
                                                Relationship = p.Relationship,
                                                PhoneNumber = p.Parent?.User?.PhoneNumber,
                                                Email = p.Parent?.User?.Email,
                    IsPrimaryContact = p.IsPrimaryContact
                                            }).ToList();

                return new StudentDto
                {
                    StudentId = student.StudentId,
                    StudentNumber = student.StudentNumber,
                    FirstName = student.User.FirstName,
                    LastName = student.User.LastName,
                    Email = student.User?.Email ?? "N/A",
                    PhoneNumber = student.User?.PhoneNumber ?? student.User?.AlternatePhoneNumber?? "N/A",
                    Address = student.User?.Address ?? "N/A",
                    AdmissionDate = student.AdmissionDate,
                    ClassName = student.Class?.ClassName ?? "N/A",
                    GuardianName = guardianName,
                    Parents = parents,
                    MedicalInfo = student.MedicalInfo,
                    EmergencyContact = student.EmergencyContact,
                    IsActive = student.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student with ID {StudentId}", studentId);
                throw;
            }
        }

        public async Task<StudentDto?> GetStudentByNumberAsync(string studentNumber)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .Include(s => s.StudentParents)
                        .ThenInclude(g => g.Parent)
                            .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(s => s.StudentNumber == studentNumber);

                if (student == null)
                    return null;

                var guardianName = student.StudentParents
                                            .OrderByDescending(p => p.IsPrimaryContact)
                                            .Select(p => p.Parent?.User?.FullName)
                                            .FirstOrDefault(name => !string.IsNullOrEmpty(name));

                var parents = student.StudentParents.Select(p => new StudentParentDto
                {
                    FullName = p.Parent?.User?.FullName,
                    Relationship = p.Relationship,
                    PhoneNumber = p.Parent?.User?.PhoneNumber,
                    Email = p.Parent?.User?.Email,
                    IsPrimaryContact = p.IsPrimaryContact
                }).ToList();

                return new StudentDto
                {
                    StudentId = student.StudentId,
                    StudentNumber = student.StudentNumber,
                    FirstName = student.User.FirstName,
                    LastName = student.User.LastName,
                    Email = student.User?.Email ?? "N/A",
                    PhoneNumber = student.User?.PhoneNumber ?? student.User?.AlternatePhoneNumber ?? "N/A",
                    Address = student.User?.Address ?? "N/A",
                    AdmissionDate = student.AdmissionDate,
                    ClassName = student.Class?.ClassName ?? "N/A",
                    GuardianName = guardianName,
                    Parents = parents,
                    MedicalInfo = student.MedicalInfo,
                    EmergencyContact = student.EmergencyContact,
                    IsActive = student.IsActive
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student with number {StudentNumber}", studentNumber);
                throw;
            }
        }

        public async Task<StudentDto> CreateStudentAsync(CreateStudentDto createStudentDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create User first
                var user = new ApplicationUser
                {
                    Email = createStudentDto.Email,
                    PasswordHash = HashPassword("TempPassword123"), // Temporary password
                    FirstName = createStudentDto.FirstName,
                    MiddleName = createStudentDto.MiddleName,
                    LastName = createStudentDto.LastName,
                    PhoneNumber = createStudentDto.PhoneNumber,
                    Gender = createStudentDto.Gender,
                    DateOfBirth = createStudentDto.DateOfBirth,
                    Address = createStudentDto.Address,
                    City = createStudentDto.City,
                    State = createStudentDto.State,
                    PostalCode = createStudentDto.PostalCode,
                    Country = createStudentDto.Country,
                    AlternatePhoneNumber = createStudentDto.AlternatePhoneNumber,
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate student number
                var studentNumber = await GenerateStudentNumberAsync();

                // Create Student
                var student = new Student
                {
                    UserId = user.Id,
                    StudentNumber = studentNumber,
                    AdmissionDate = createStudentDto.AdmissionDate,
                    ClassId = createStudentDto.ClassId,
                    MedicalInfo = createStudentDto.MedicalInfo,
                    EmergencyContact = createStudentDto.EmergencyContact,
                    BloodGroup = createStudentDto.BloodGroup,
                    Allergies = createStudentDto.Allergies,
                    IsActive = createStudentDto.IsActive,
                    CreatedDate = createStudentDto.CreatedDate
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var result = await GetStudentByIdAsync(student.StudentId);
                if (result == null)
                {
                    throw new InvalidOperationException($"Student with ID {student.StudentId} was not found after creation.");
                }

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating student");
                throw;
            }
        }


        public async Task<bool> UpdateStudentAsync(Guid studentId, UpdateStudentDto updateStudentDto)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .Include(s => s.StudentParents)
                        .ThenInclude(g => g.Parent)
                            .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (student == null)
                    return false;

                // Update User information
                student.User.FirstName = updateStudentDto.FirstName;
                student.User.LastName = updateStudentDto.LastName;
                student.User.PhoneNumber = updateStudentDto.PhoneNumber;
                student.User.Address = updateStudentDto.Address;

                // Update Student information
                student.ClassId = updateStudentDto.ClassId;
                student.MedicalInfo = updateStudentDto.MedicalInfo;
                student.EmergencyContact = updateStudentDto.EmergencyContact;
                student.IsActive = updateStudentDto.IsActive;
                student.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student with ID {StudentId}", studentId);
                throw;
            }
        }

        public async Task<bool> DeleteStudentAsync(Guid studentId)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (student == null)
                    return false;

                _context.Students.Remove(student);
                _context.Users.Remove(student.User);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student with ID {StudentId}", studentId);
                throw;
            }
        }

        public async Task<bool> DeactivateStudentAsync(Guid studentId)
        {
            try
            {
                var student = await _context.Students.FindAsync(studentId);
                if (student == null)
                    return false;

                student.IsActive = false;
                student.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating student with ID {StudentId}", studentId);
                throw;
            }
        }

        public async Task<IEnumerable<StudentAttendanceDto>> GetStudentAttendanceAsync(Guid studentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.Attendances
                    .Include(a => a.Class)
                    .Where(a => a.StudentId == studentId);

                if (fromDate.HasValue)
                    query = query.Where(a => a.AttendanceDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(a => a.AttendanceDate <= toDate.Value);

                var attendance = await query
                    .OrderByDescending(a => a.AttendanceDate)
                    .Select(a => new StudentAttendanceDto
                    {
                        AttendanceId = a.AttendanceId,
                        AttendanceDate = a.AttendanceDate,
                        Status = a.Status.ToString(),
                        Remarks = a.Remarks,
                        ClassName = a.Class!.ClassName
                    })
                    .ToListAsync();

                return attendance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for student {StudentId}", studentId);
                throw;
            }
        }

        public async Task<IEnumerable<StudentGradeDto>> GetStudentGradesAsync(Guid studentId, string? academicYear = null)
        {
            try
            {
                var query = _context.ExamResults
                    .Include(er => er.Exam)
                    .Include(er => er.Subject)
                    .Where(er => er.StudentId == studentId);

                // Note: This would need to be adjusted based on your actual ExamResult model
                var grades = await query
                    .OrderByDescending(er => er.Exam.StartDate)
                    .Select(er => new StudentGradeDto
                    {
                        ResultId = er.ResultId,
                        ExamName = er.Exam.ExamName,
                        SubjectName = er.Subject.SubjectName,
                        MarksObtained = er.MarksObtained,
                        MaxMarks = er.MaxMarks,
                        Grade = er.Grade?? "N/A",
                        ExamDate = er.Exam.StartDate
                    })
                    .ToListAsync();

                return grades;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving grades for student {StudentId}", studentId);
                throw;
            }
        }

        public async Task<bool> TransferStudentAsync(Guid studentId, Guid newClassId)
        {
            try
            {
                var student = await _context.Students.FindAsync(studentId);
                if (student == null)
                    return false;

                student.ClassId = newClassId;
                student.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring student {StudentId} to class {ClassId}", studentId, newClassId);
                throw;
            }
        }

        public async Task<int> GetStudentCountByClassAsync(Guid classId)
        {
            try
            {
                return await _context.Students
                    .Where(s => s.ClassId == classId && s.IsActive)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student count for class {ClassId}", classId);
                throw;
            }
        }

        public async Task<IEnumerable<StudentDto>> SearchStudentsAsync(string searchTerm)
        {
            try
            {
                var students = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .Include(s => s.StudentParents)
                        .ThenInclude(g => g.Parent)
                            .ThenInclude(p => p.User)
                    .Where(s => s.User.FirstName.Contains(searchTerm) ||
                               s.User.LastName.Contains(searchTerm) ||
                               s.StudentNumber.Contains(searchTerm) ||
                               s.User!.Email.Contains(searchTerm))
                    .Select(s => new StudentDto
                    {
                        StudentId = s.StudentId,
                        StudentNumber = s.StudentNumber,
                        FirstName = s.User.FirstName,
                        LastName = s.User.LastName,
                        Email = s.User!.Email ?? "N/A",
                        PhoneNumber = s.User.PhoneNumber,
                        Address = s.User.Address,
                        AdmissionDate = s.AdmissionDate,
                        ClassName = s.Class!.ClassName ?? "N/A",
                        //GuardianName = s.Guardian != null ? s.Guardian.User.FullName : null,
                        MedicalInfo = s.MedicalInfo,
                        EmergencyContact = s.EmergencyContact,
                        IsActive = s.IsActive
                    })
                    .ToListAsync();

                return students;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching students with term {SearchTerm}", searchTerm);
                throw;
            }
        }

        private async Task<string> GenerateStudentNumberAsync()
        {
            var year = DateTime.Now.Year.ToString();
            var lastStudent = await _context.Students
                .Where(s => s.StudentNumber.StartsWith(year))
                .OrderByDescending(s => s.StudentNumber)
                .FirstOrDefaultAsync();

            if (lastStudent == null)
            {
                return $"{year}001";
            }

            var lastNumber = int.Parse(lastStudent.StudentNumber.Substring(4));
            var newNumber = lastNumber + 1;
            return $"{year}{newNumber:D3}";
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}


