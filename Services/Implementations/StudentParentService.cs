using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.DTOs;
using SchoolManager.Enums;
using SchoolManager.Models;
using SchoolManager.Models.Common;

namespace SchoolManager.Services.Implementations
{
    public class StudentParentService : IStudentParentService
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<StudentParentService> _logger;

        public StudentParentService(SchoolManagementDbContext context, ILogger<StudentParentService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Basic CRUD Operations

        public async Task<ServiceResult<StudentParentDto>> CreateStudentParentRelationshipAsync(CreateStudentParentDto createDto)
        {
            try
            {
                // Validate that student and parent exist
                var student = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .FirstOrDefaultAsync(s => s.StudentId == createDto.StudentId);

                if (student == null)
                {
                    return ServiceResult<StudentParentDto>.Failure("Student not found.");
                }

                var parent = await _context.Parents
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.ParentId == createDto.ParentId);

                if (parent == null)
                {
                    return ServiceResult<StudentParentDto>.Failure("Parent not found.");
                }

                // Check if relationship already exists
                var existingRelationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentId == createDto.StudentId && sp.ParentId == createDto.ParentId);

                if (existingRelationship != null)
                {
                    return ServiceResult<StudentParentDto>.Failure("Relationship between student and parent already exists.");
                }

                // If setting as primary contact, ensure no other parent is primary for this student
                if (createDto.IsPrimaryContact)
                {
                    await UnsetOtherPrimaryContactsAsync(createDto.StudentId);
                }

                var studentParent = new StudentParent
                {
                    StudentParentId = Guid.NewGuid(),
                    StudentId = createDto.StudentId,
                    ParentId = createDto.ParentId,
                    RelationshipToStudent = createDto.RelationshipToStudent,
                    IsPrimaryContact = createDto.IsPrimaryContact,
                    IsEmergencyContact = createDto.IsEmergencyContact,
                    CanPickupStudent = createDto.CanPickupStudent,
                    CreatedDate = DateTime.UtcNow
                };

                _context.StudentParents.Add(studentParent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created student-parent relationship: {StudentId} - {ParentId}",
                    createDto.StudentId, createDto.ParentId);

                return ServiceResult<StudentParentDto>.Success(await MapToStudentParentDtoAsync(studentParent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student-parent relationship");
                return ServiceResult<StudentParentDto>.Failure("An error occurred while creating the relationship.");
            }
        }

        public async Task<ServiceResult<StudentParentDto>> GetStudentParentRelationshipAsync(Guid studentParentId)
        {
            try
            {
                var relationship = await _context.StudentParents
                    .Include(sp => sp.Student)
                        .ThenInclude(s => s.User)
                    .Include(sp => sp.Student.Class)
                    .Include(sp => sp.Parent)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(sp => sp.StudentParentId == studentParentId);

                if (relationship == null)
                {
                    return ServiceResult<StudentParentDto>.Failure("Student-parent relationship not found.");
                }

                return ServiceResult<StudentParentDto>.Success(await MapToStudentParentDtoAsync(relationship));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student-parent relationship {Id}", studentParentId);
                return ServiceResult<StudentParentDto>.Failure("An error occurred while retrieving the relationship.");
            }
        }

        public async Task<ServiceResult<StudentParentDto>> UpdateStudentParentRelationshipAsync(Guid studentParentId, UpdateStudentParentDto updateDto)
        {
            try
            {
                var relationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentParentId == studentParentId);

                if (relationship == null)
                {
                    return ServiceResult<StudentParentDto>.Failure("Student-parent relationship not found.");
                }

                // If setting as primary contact, ensure no other parent is primary for this student
                if (updateDto.IsPrimaryContact && !relationship.IsPrimaryContact)
                {
                    await UnsetOtherPrimaryContactsAsync(relationship.StudentId);
                }

                relationship.RelationshipToStudent = updateDto.RelationshipToStudent;
                relationship.IsPrimaryContact = updateDto.IsPrimaryContact;
                relationship.IsEmergencyContact = updateDto.IsEmergencyContact;
                relationship.CanPickupStudent = updateDto.CanPickupStudent;
                relationship.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated student-parent relationship {Id}", studentParentId);

                return ServiceResult<StudentParentDto>.Success(await MapToStudentParentDtoAsync(relationship));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student-parent relationship {Id}", studentParentId);
                return ServiceResult<StudentParentDto>.Failure("An error occurred while updating the relationship.");
            }
        }

        public async Task<ServiceResult<bool>> DeleteStudentParentRelationshipAsync(Guid studentParentId)
        {
            try
            {
                var relationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentParentId == studentParentId);

                if (relationship == null)
                {
                    return ServiceResult<bool>.Failure("Student-parent relationship not found.");
                }

                _context.StudentParents.Remove(relationship);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted student-parent relationship {Id}", studentParentId);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting student-parent relationship {Id}", studentParentId);
                return ServiceResult<bool>.Failure("An error occurred while deleting the relationship.");
            }
        }

        #endregion

        #region Student-Parent Relationship Queries

        public async Task<ServiceResult<StudentWithParentsDto>> GetStudentWithParentsAsync(Guid studentId)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .Include(s => s.StudentParents)
                        .ThenInclude(sp => sp.Parent)
                            .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(s => s.StudentId == studentId);

                if (student == null)
                {
                    return ServiceResult<StudentWithParentsDto>.Failure("Student not found.");
                }

                var dto = new StudentWithParentsDto
                {
                    StudentId = student.StudentId,
                    StudentNumber = student.StudentNumber,
                    FullName = student.User.FullName,
                    Email = student.User.Email,
                    ClassName = student.Class?.ClassName,
                    IsActive = student.IsActive,
                    Parents = student.StudentParents.Select(sp => new StudentParentRelationshipDto
                    {
                        StudentParentId = sp.StudentParentId,
                        ParentId = sp.ParentId,
                        ParentFullName = sp.Parent.User.FullName,
                        ParentEmail = sp.Parent.User.Email,
                        ParentPhoneNumber = sp.Parent.User.PhoneNumber,
                        ParentWorkPhoneNumber = sp.Parent.WorkPhoneNumber,
                        ParentOccupation = sp.Parent.Occupation,
                        RelationshipToStudent = sp.RelationshipToStudent,
                        IsPrimaryContact = sp.IsPrimaryContact,
                        IsEmergencyContact = sp.IsEmergencyContact,
                        CanPickupStudent = sp.CanPickupStudent,
                        ParentIsActive = sp.Parent.IsActive
                    }).ToList()
                };

                return ServiceResult<StudentWithParentsDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting student with parents {StudentId}", studentId);
                return ServiceResult<StudentWithParentsDto>.Failure("An error occurred while retrieving student information.");
            }
        }

        public async Task<ServiceResult<ParentWithStudentsDto>> GetParentWithStudentsAsync(Guid parentId)
        {
            try
            {
                var parent = await _context.Parents
                    .Include(p => p.User)
                    .Include(p => p.StudentParents)
                        .ThenInclude(sp => sp.Student)
                            .ThenInclude(s => s.User)
                    .Include(p => p.StudentParents)
                        .ThenInclude(sp => sp.Student)
                            .ThenInclude(s => s.Class)
                    .FirstOrDefaultAsync(p => p.ParentId == parentId);

                if (parent == null)
                {
                    return ServiceResult<ParentWithStudentsDto>.Failure("Parent not found.");
                }

                var dto = new ParentWithStudentsDto
                {
                    ParentId = parent.ParentId,
                    FullName = parent.User.FullName,
                    Email = parent.User.Email,
                    PhoneNumber = parent.User.PhoneNumber,
                    Occupation = parent.Occupation,
                    IsActive = parent.IsActive,
                    Students = parent.StudentParents.Select(sp => new ParentStudentRelationshipDto
                    {
                        StudentParentId = sp.StudentParentId,
                        StudentId = sp.StudentId,
                        StudentNumber = sp.Student.StudentNumber,
                        StudentFullName = sp.Student.User.FullName,
                        StudentEmail = sp.Student.User.Email,
                        ClassName = sp.Student.Class?.ClassName,
                        RelationshipToStudent = sp.RelationshipToStudent,
                        IsPrimaryContact = sp.IsPrimaryContact,
                        IsEmergencyContact = sp.IsEmergencyContact,
                        CanPickupStudent = sp.CanPickupStudent,
                        StudentIsActive = sp.Student.IsActive
                    }).ToList()
                };

                return ServiceResult<ParentWithStudentsDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parent with students {ParentId}", parentId);
                return ServiceResult<ParentWithStudentsDto>.Failure("An error occurred while retrieving parent information.");
            }
        }

        public async Task<ServiceResult<List<StudentParentRelationshipDto>>> GetParentsByStudentIdAsync(Guid studentId)
        {
            try
            {
                var relationships = await _context.StudentParents
                    .Include(sp => sp.Parent)
                        .ThenInclude(p => p.User)
                    .Where(sp => sp.StudentId == studentId)
                    .ToListAsync();

                var dtos = relationships.Select(sp => new StudentParentRelationshipDto
                {
                    StudentParentId = sp.StudentParentId,
                    ParentId = sp.ParentId,
                    ParentFullName = sp.Parent.User.FullName,
                    ParentEmail = sp.Parent.User.Email,
                    ParentPhoneNumber = sp.Parent.User.PhoneNumber,
                    ParentWorkPhoneNumber = sp.Parent.WorkPhoneNumber,
                    ParentOccupation = sp.Parent.Occupation,
                    RelationshipToStudent = sp.RelationshipToStudent,
                    IsPrimaryContact = sp.IsPrimaryContact,
                    IsEmergencyContact = sp.IsEmergencyContact,
                    CanPickupStudent = sp.CanPickupStudent,
                    ParentIsActive = sp.Parent.IsActive
                }).ToList();

                return ServiceResult<List<StudentParentRelationshipDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parents for student {StudentId}", studentId);
                return ServiceResult<List<StudentParentRelationshipDto>>.Failure("An error occurred while retrieving parent information.");
            }
        }

        public async Task<ServiceResult<List<ParentStudentRelationshipDto>>> GetStudentsByParentIdAsync(Guid parentId)
        {
            try
            {
                var relationships = await _context.StudentParents
                    .Include(sp => sp.Student)
                        .ThenInclude(s => s.User)
                    .Include(sp => sp.Student)
                        .ThenInclude(s => s.Class)
                    .Where(sp => sp.ParentId == parentId)
                    .ToListAsync();

                var dtos = relationships.Select(sp => new ParentStudentRelationshipDto
                {
                    StudentParentId = sp.StudentParentId,
                    StudentId = sp.StudentId,
                    StudentNumber = sp.Student.StudentNumber,
                    StudentFullName = sp.Student.User.FullName,
                    StudentEmail = sp.Student.User.Email,
                    ClassName = sp.Student.Class?.ClassName,
                    RelationshipToStudent = sp.RelationshipToStudent,
                    IsPrimaryContact = sp.IsPrimaryContact,
                    IsEmergencyContact = sp.IsEmergencyContact,
                    CanPickupStudent = sp.CanPickupStudent,
                    StudentIsActive = sp.Student.IsActive
                }).ToList();

                return ServiceResult<List<ParentStudentRelationshipDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students for parent {ParentId}", parentId);
                return ServiceResult<List<ParentStudentRelationshipDto>>.Failure("An error occurred while retrieving student information.");
            }
        }

        #endregion

        #region Contact Management

        public async Task<ServiceResult<List<EmergencyContactDto>>> GetEmergencyContactsForStudentAsync(Guid studentId)
        {
            try
            {
                var emergencyContacts = await _context.StudentParents
                    .Include(sp => sp.Parent)
                        .ThenInclude(p => p.User)
                    .Where(sp => sp.StudentId == studentId && sp.IsEmergencyContact)
                    .Select(sp => new EmergencyContactDto
                    {
                        ParentId = sp.ParentId,
                        FullName = sp.Parent.User.FullName,
                        Email = sp.Parent.User.Email,
                        PhoneNumber = sp.Parent.User.PhoneNumber,
                        WorkPhoneNumber = sp.Parent.WorkPhoneNumber,
                        AlternatePhoneNumber = sp.Parent.User.AlternatePhoneNumber,
                        RelationshipToStudent = sp.RelationshipToStudent,
                        IsPrimaryContact = sp.IsPrimaryContact,
                        CanPickupStudent = sp.CanPickupStudent,
                        WorkAddress = sp.Parent.WorkAddress
                    })
                    .ToListAsync();

                return ServiceResult<List<EmergencyContactDto>>.Success(emergencyContacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emergency contacts for student {StudentId}", studentId);
                return ServiceResult<List<EmergencyContactDto>>.Failure("An error occurred while retrieving emergency contacts.");
            }
        }

        public async Task<ServiceResult<EmergencyContactDto>> GetPrimaryContactForStudentAsync(Guid studentId)
        {
            try
            {
                var primaryContact = await _context.StudentParents
                    .Include(sp => sp.Parent)
                        .ThenInclude(p => p.User)
                    .Where(sp => sp.StudentId == studentId && sp.IsPrimaryContact)
                    .Select(sp => new EmergencyContactDto
                    {
                        ParentId = sp.ParentId,
                        FullName = sp.Parent.User.FullName,
                        Email = sp.Parent.User.Email,
                        PhoneNumber = sp.Parent.User.PhoneNumber,
                        WorkPhoneNumber = sp.Parent.WorkPhoneNumber,
                        AlternatePhoneNumber = sp.Parent.User.AlternatePhoneNumber,
                        RelationshipToStudent = sp.RelationshipToStudent,
                        IsPrimaryContact = sp.IsPrimaryContact,
                        CanPickupStudent = sp.CanPickupStudent,
                        WorkAddress = sp.Parent.WorkAddress
                    })
                    .FirstOrDefaultAsync();

                if (primaryContact == null)
                {
                    return ServiceResult<EmergencyContactDto>.Failure("No primary contact found for this student.");
                }

                return ServiceResult<EmergencyContactDto>.Success(primaryContact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting primary contact for student {StudentId}", studentId);
                return ServiceResult<EmergencyContactDto>.Failure("An error occurred while retrieving primary contact.");
            }
        }

        public async Task<ServiceResult<List<EmergencyContactDto>>> GetAuthorizedPickupContactsAsync(Guid studentId)
        {
            try
            {
                var pickupContacts = await _context.StudentParents
                    .Include(sp => sp.Parent)
                        .ThenInclude(p => p.User)
                    .Where(sp => sp.StudentId == studentId && sp.CanPickupStudent)
                    .Select(sp => new EmergencyContactDto
                    {
                        ParentId = sp.ParentId,
                        FullName = sp.Parent.User.FullName,
                        Email = sp.Parent.User.Email,
                        PhoneNumber = sp.Parent.User.PhoneNumber,
                        WorkPhoneNumber = sp.Parent.WorkPhoneNumber,
                        AlternatePhoneNumber = sp.Parent.User.AlternatePhoneNumber,
                        RelationshipToStudent = sp.RelationshipToStudent,
                        IsPrimaryContact = sp.IsPrimaryContact,
                        CanPickupStudent = sp.CanPickupStudent,
                        WorkAddress = sp.Parent.WorkAddress
                    })
                    .ToListAsync();

                return ServiceResult<List<EmergencyContactDto>>.Success(pickupContacts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting authorized pickup contacts for student {StudentId}", studentId);
                return ServiceResult<List<EmergencyContactDto>>.Failure("An error occurred while retrieving pickup contacts.");
            }
        }

        public async Task<ServiceResult<bool>> UpdateContactPermissionsAsync(Guid studentParentId, ContactPermissionsDto permissionsDto)
        {
            try
            {
                var relationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentParentId == studentParentId);

                if (relationship == null)
                {
                    return ServiceResult<bool>.Failure("Student-parent relationship not found.");
                }

                // If setting as primary contact, ensure no other parent is primary for this student
                if (permissionsDto.IsPrimaryContact && !relationship.IsPrimaryContact)
                {
                    await UnsetOtherPrimaryContactsAsync(relationship.StudentId);
                }

                relationship.IsPrimaryContact = permissionsDto.IsPrimaryContact;
                relationship.IsEmergencyContact = permissionsDto.IsEmergencyContact;
                relationship.CanPickupStudent = permissionsDto.CanPickupStudent;
                relationship.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated contact permissions for relationship {Id}", studentParentId);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact permissions for relationship {Id}", studentParentId);
                return ServiceResult<bool>.Failure("An error occurred while updating contact permissions.");
            }
        }

        #endregion

        #region Relationship Management

        public async Task<ServiceResult<bool>> SetPrimaryContactAsync(Guid studentId, Guid parentId)
        {
            try
            {
                // First, unset all existing primary contacts for this student
                await UnsetOtherPrimaryContactsAsync(studentId);

                // Set the specified parent as primary contact
                var relationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.ParentId == parentId);

                if (relationship == null)
                {
                    return ServiceResult<bool>.Failure("Student-parent relationship not found.");
                }

                relationship.IsPrimaryContact = true;
                relationship.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Set primary contact for student {StudentId} to parent {ParentId}", studentId, parentId);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting primary contact for student {StudentId}", studentId);
                return ServiceResult<bool>.Failure("An error occurred while setting primary contact.");
            }
        }

        public async Task<ServiceResult<bool>> AddEmergencyContactAsync(Guid studentId, Guid parentId)
        {
            try
            {
                var relationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.ParentId == parentId);

                if (relationship == null)
                {
                    return ServiceResult<bool>.Failure("Student-parent relationship not found.");
                }

                relationship.IsEmergencyContact = true;
                relationship.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Added emergency contact for student {StudentId}: parent {ParentId}", studentId, parentId);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding emergency contact for student {StudentId}", studentId);
                return ServiceResult<bool>.Failure("An error occurred while adding emergency contact.");
            }
        }

        public async Task<ServiceResult<bool>> RemoveEmergencyContactAsync(Guid studentId, Guid parentId)
        {
            try
            {
                var relationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.ParentId == parentId);

                if (relationship == null)
                {
                    return ServiceResult<bool>.Failure("Student-parent relationship not found.");
                }

                relationship.IsEmergencyContact = false;
                relationship.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Removed emergency contact for student {StudentId}: parent {ParentId}", studentId, parentId);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing emergency contact for student {StudentId}", studentId);
                return ServiceResult<bool>.Failure("An error occurred while removing emergency contact.");
            }
        }

        public async Task<ServiceResult<bool>> UpdatePickupPermissionAsync(Guid studentParentId, bool canPickup)
        {
            try
            {
                var relationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentParentId == studentParentId);

                if (relationship == null)
                {
                    return ServiceResult<bool>.Failure("Student-parent relationship not found.");
                }

                relationship.CanPickupStudent = canPickup;
                relationship.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated pickup permission for relationship {Id} to {Permission}", studentParentId, canPickup);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating pickup permission for relationship {Id}", studentParentId);
                return ServiceResult<bool>.Failure("An error occurred while updating pickup permission.");
            }
        }

        #endregion

        #region Validation Operations

        public async Task<ServiceResult<bool>> RelationshipExistsAsync(Guid studentId, Guid parentId)
        {
            try
            {
                var exists = await _context.StudentParents
                    .AnyAsync(sp => sp.StudentId == studentId && sp.ParentId == parentId);

                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if relationship exists for student {StudentId} and parent {ParentId}", studentId, parentId);
                return ServiceResult<bool>.Failure("An error occurred while checking relationship existence.");
            }
        }

        public async Task<ServiceResult<bool>> CanParentPickupStudentAsync(Guid studentId, Guid parentId)
        {
            try
            {
                var relationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.ParentId == parentId);

                if (relationship == null)
                {
                    return ServiceResult<bool>.Success(false);
                }

                return ServiceResult<bool>.Success(relationship.CanPickupStudent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking pickup permission for student {StudentId} and parent {ParentId}", studentId, parentId);
                return ServiceResult<bool>.Failure("An error occurred while checking pickup permission.");
            }
        }

        public async Task<ServiceResult<bool>> IsParentPrimaryContactAsync(Guid studentId, Guid parentId)
        {
            try
            {
                var relationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.ParentId == parentId);

                if (relationship == null)
                {
                    return ServiceResult<bool>.Success(false);
                }

                return ServiceResult<bool>.Success(relationship.IsPrimaryContact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking primary contact status for student {StudentId} and parent {ParentId}", studentId, parentId);
                return ServiceResult<bool>.Failure("An error occurred while checking primary contact status.");
            }
        }

        public async Task<ServiceResult<bool>> IsParentEmergencyContactAsync(Guid studentId, Guid parentId)
        {
            try
            {
                var relationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.ParentId == parentId);

                if (relationship == null)
                {
                    return ServiceResult<bool>.Success(false);
                }

                return ServiceResult<bool>.Success(relationship.IsEmergencyContact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking emergency contact status for student {StudentId} and parent {ParentId}", studentId, parentId);
                return ServiceResult<bool>.Failure("An error occurred while checking emergency contact status.");
            }
        }

        #endregion

        #region Bulk Operations

        public async Task<ServiceResult<List<StudentParentDto>>> CreateMultipleRelationshipsAsync(List<CreateStudentParentDto> createDtos)
        {
            try
            {
                var results = new List<StudentParentDto>();

                foreach (var createDto in createDtos)
                {
                    var result = await CreateStudentParentRelationshipAsync(createDto);
                    if (result.IsSuccess)
                    {
                        results.Add(result.Data);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to create relationship for Student {StudentId} and Parent {ParentId}: {Error}",
                            createDto.StudentId, createDto.ParentId, result.Message);
                    }
                }

                _logger.LogInformation("Created {Count} out of {Total} relationships", results.Count, createDtos.Count);

                return ServiceResult<List<StudentParentDto>>.Success(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating multiple relationships");
                return ServiceResult<List<StudentParentDto>>.Failure("An error occurred while creating multiple relationships.");
            }
        }

        public async Task<ServiceResult<bool>> TransferStudentToNewParentAsync(Guid studentId, Guid oldParentId, Guid newParentId, ParentRelationship relationship)
        {
            try
            {
                // Check if old relationship exists
                var oldRelationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.ParentId == oldParentId);

                if (oldRelationship == null)
                {
                    return ServiceResult<bool>.Failure("Original student-parent relationship not found.");
                }

                // Check if new parent exists
                var newParent = await _context.Parents
                    .FirstOrDefaultAsync(p => p.ParentId == newParentId);

                if (newParent == null)
                {
                    return ServiceResult<bool>.Failure("New parent not found.");
                }

                // Check if relationship with new parent already exists
                var existingNewRelationship = await _context.StudentParents
                    .FirstOrDefaultAsync(sp => sp.StudentId == studentId && sp.ParentId == newParentId);

                if (existingNewRelationship != null)
                {
                    return ServiceResult<bool>.Failure("Relationship with new parent already exists.");
                }

                // Store old relationship properties
                var wasPrimaryContact = oldRelationship.IsPrimaryContact;
                var wasEmergencyContact = oldRelationship.IsEmergencyContact;
                var canPickupStudent = oldRelationship.CanPickupStudent;

                // Remove old relationship
                _context.StudentParents.Remove(oldRelationship);

                // Create new relationship
                var newRelationship = new StudentParent
                {
                    StudentParentId = Guid.NewGuid(),
                    StudentId = studentId,
                    ParentId = newParentId,
                    RelationshipToStudent = relationship,
                    IsPrimaryContact = wasPrimaryContact,
                    IsEmergencyContact = wasEmergencyContact,
                    CanPickupStudent = canPickupStudent,
                    CreatedDate = DateTime.UtcNow
                };

                _context.StudentParents.Add(newRelationship);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Transferred student {StudentId} from parent {OldParentId} to parent {NewParentId}",
                    studentId, oldParentId, newParentId);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring student {StudentId} from parent {OldParentId} to parent {NewParentId}",
                    studentId, oldParentId, newParentId);
                return ServiceResult<bool>.Failure("An error occurred while transferring student to new parent.");
            }
        }

        public async Task<ServiceResult<bool>> UpdateMultipleContactPermissionsAsync(List<ContactPermissionsDto> permissionsList)
        {
            try
            {
                var updatedCount = 0;

                foreach (var permissions in permissionsList)
                {
                    var relationship = await _context.StudentParents
                        .FirstOrDefaultAsync(sp => sp.StudentParentId == permissions.StudentParentId);

                    if (relationship != null)
                    {
                        // If setting as primary contact, ensure no other parent is primary for this student
                        if (permissions.IsPrimaryContact && !relationship.IsPrimaryContact)
                        {
                            await UnsetOtherPrimaryContactsAsync(relationship.StudentId);
                        }

                        relationship.IsPrimaryContact = permissions.IsPrimaryContact;
                        relationship.IsEmergencyContact = permissions.IsEmergencyContact;
                        relationship.CanPickupStudent = permissions.CanPickupStudent;
                        relationship.LastModifiedDate = DateTime.UtcNow;

                        updatedCount++;
                    }
                    else
                    {
                        _logger.LogWarning("Student-parent relationship {Id} not found", permissions.StudentParentId);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated contact permissions for {Count} relationships", updatedCount);

                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating multiple contact permissions");
                return ServiceResult<bool>.Failure("An error occurred while updating contact permissions.");
            }
        }

        #endregion

        #region Search and Filter Operations

        public async Task<ServiceResult<List<StudentParentDto>>> GetRelationshipsByRelationshipTypeAsync(ParentRelationship relationshipType)
        {
            try
            {
                var relationships = await _context.StudentParents
                    .Include(sp => sp.Student)
                        .ThenInclude(s => s.User)
                    .Include(sp => sp.Student.Class)
                    .Include(sp => sp.Parent)
                        .ThenInclude(p => p.User)
                    .Where(sp => sp.RelationshipToStudent == relationshipType)
                    .ToListAsync();

                var dtos = new List<StudentParentDto>();
                foreach (var relationship in relationships)
                {
                    dtos.Add(await MapToStudentParentDtoAsync(relationship));
                }

                return ServiceResult<List<StudentParentDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting relationships by type {RelationshipType}", relationshipType);
                return ServiceResult<List<StudentParentDto>>.Failure("An error occurred while retrieving relationships by type.");
            }
        }

        public async Task<ServiceResult<List<StudentWithParentsDto>>> GetStudentsWithoutPrimaryContactAsync()
        {
            try
            {
                // Get students who either have no parent relationships or no primary contact
                var studentsWithoutPrimary = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .Include(s => s.StudentParents)
                        .ThenInclude(sp => sp.Parent)
                            .ThenInclude(p => p.User)
                    .Where(s => s.IsActive &&
                           (!s.StudentParents.Any() || !s.StudentParents.Any(sp => sp.IsPrimaryContact)))
                    .ToListAsync();

                var dtos = studentsWithoutPrimary.Select(student => new StudentWithParentsDto
                {
                    StudentId = student.StudentId,
                    StudentNumber = student.StudentNumber,
                    FullName = student.User.FullName,
                    Email = student.User.Email,
                    ClassName = student.Class?.ClassName,
                    IsActive = student.IsActive,
                    Parents = student.StudentParents.Select(sp => new StudentParentRelationshipDto
                    {
                        StudentParentId = sp.StudentParentId,
                        ParentId = sp.ParentId,
                        ParentFullName = sp.Parent.User.FullName,
                        ParentEmail = sp.Parent.User.Email,
                        ParentPhoneNumber = sp.Parent.User.PhoneNumber,
                        ParentWorkPhoneNumber = sp.Parent.WorkPhoneNumber,
                        ParentOccupation = sp.Parent.Occupation,
                        RelationshipToStudent = sp.RelationshipToStudent,
                        IsPrimaryContact = sp.IsPrimaryContact,
                        IsEmergencyContact = sp.IsEmergencyContact,
                        CanPickupStudent = sp.CanPickupStudent,
                        ParentIsActive = sp.Parent.IsActive
                    }).ToList()
                }).ToList();

                return ServiceResult<List<StudentWithParentsDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students without primary contact");
                return ServiceResult<List<StudentWithParentsDto>>.Failure("An error occurred while retrieving students without primary contact.");
            }
        }

        public async Task<ServiceResult<List<StudentWithParentsDto>>> GetStudentsWithoutEmergencyContactAsync()
        {
            try
            {
                // Get students who either have no parent relationships or no emergency contact
                var studentsWithoutEmergency = await _context.Students
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .Include(s => s.StudentParents)
                        .ThenInclude(sp => sp.Parent)
                            .ThenInclude(p => p.User)
                    .Where(s => s.IsActive &&
                           (!s.StudentParents.Any() || !s.StudentParents.Any(sp => sp.IsEmergencyContact)))
                    .ToListAsync();

                var dtos = studentsWithoutEmergency.Select(student => new StudentWithParentsDto
                {
                    StudentId = student.StudentId,
                    StudentNumber = student.StudentNumber,
                    FullName = student.User.FullName,
                    Email = student.User.Email,
                    ClassName = student.Class?.ClassName,
                    IsActive = student.IsActive,
                    Parents = student.StudentParents.Select(sp => new StudentParentRelationshipDto
                    {
                        StudentParentId = sp.StudentParentId,
                        ParentId = sp.ParentId,
                        ParentFullName = sp.Parent.User.FullName,
                        ParentEmail = sp.Parent.User.Email,
                        ParentPhoneNumber = sp.Parent.User.PhoneNumber,
                        ParentWorkPhoneNumber = sp.Parent.WorkPhoneNumber,
                        ParentOccupation = sp.Parent.Occupation,
                        RelationshipToStudent = sp.RelationshipToStudent,
                        IsPrimaryContact = sp.IsPrimaryContact,
                        IsEmergencyContact = sp.IsEmergencyContact,
                        CanPickupStudent = sp.CanPickupStudent,
                        ParentIsActive = sp.Parent.IsActive
                    }).ToList()
                }).ToList();

                return ServiceResult<List<StudentWithParentsDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students without emergency contact");
                return ServiceResult<List<StudentWithParentsDto>>.Failure("An error occurred while retrieving students without emergency contact.");
            }
        }

        public async Task<ServiceResult<List<ParentWithStudentsDto>>> GetParentsWithMultipleChildrenAsync()
        {
            try
            {
                var parentsWithMultipleChildren = await _context.Parents
                    .Include(p => p.User)
                    .Include(p => p.StudentParents)
                        .ThenInclude(sp => sp.Student)
                            .ThenInclude(s => s.User)
                    .Include(p => p.StudentParents)
                        .ThenInclude(sp => sp.Student)
                            .ThenInclude(s => s.Class)
                    .Where(p => p.IsActive && p.StudentParents.Count > 1)
                    .ToListAsync();

                var dtos = parentsWithMultipleChildren.Select(parent => new ParentWithStudentsDto
                {
                    ParentId = parent.ParentId,
                    FullName = parent.User.FullName,
                    Email = parent.User.Email,
                    PhoneNumber = parent.User.PhoneNumber,
                    Occupation = parent.Occupation,
                    IsActive = parent.IsActive,
                    Students = parent.StudentParents.Select(sp => new ParentStudentRelationshipDto
                    {
                        StudentParentId = sp.StudentParentId,
                        StudentId = sp.StudentId,
                        StudentNumber = sp.Student.StudentNumber,
                        StudentFullName = sp.Student.User.FullName,
                        StudentEmail = sp.Student.User.Email,
                        ClassName = sp.Student.Class?.ClassName,
                        RelationshipToStudent = sp.RelationshipToStudent,
                        IsPrimaryContact = sp.IsPrimaryContact,
                        IsEmergencyContact = sp.IsEmergencyContact,
                        CanPickupStudent = sp.CanPickupStudent,
                        StudentIsActive = sp.Student.IsActive
                    }).ToList()
                }).ToList();

                return ServiceResult<List<ParentWithStudentsDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parents with multiple children");
                return ServiceResult<List<ParentWithStudentsDto>>.Failure("An error occurred while retrieving parents with multiple children.");
            }
        }

        #endregion

        #region Statistics Operations

        public async Task<ServiceResult<Dictionary<ParentRelationship, int>>> GetRelationshipTypeStatsAsync()
        {
            try
            {
                var stats = await _context.StudentParents
                    .GroupBy(sp => sp.RelationshipToStudent)
                    .Select(g => new { RelationshipType = g.Key, Count = g.Count() })
                    .ToListAsync();

                var result = stats.ToDictionary(s => s.RelationshipType, s => s.Count);

                return ServiceResult<Dictionary<ParentRelationship, int>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting relationship type statistics");
                return ServiceResult<Dictionary<ParentRelationship, int>>.Failure("An error occurred while retrieving relationship statistics.");
            }
        }

        public async Task<ServiceResult<int>> GetTotalRelationshipsCountAsync()
        {
            try
            {
                var count = await _context.StudentParents.CountAsync();
                return ServiceResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total relationships count");
                return ServiceResult<int>.Failure("An error occurred while counting relationships.");
            }
        }

        public async Task<ServiceResult<int>> GetOrphanedStudentsCountAsync()
        {
            try
            {
                var count = await _context.Students
                    .Where(s => s.IsActive && !s.StudentParents.Any())
                    .CountAsync();

                return ServiceResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orphaned students count");
                return ServiceResult<int>.Failure("An error occurred while counting orphaned students.");
            }
        }

        public async Task<ServiceResult<List<Guid>>> GetOrphanedStudentsAsync()
        {
            try
            {
                var orphanedStudentIds = await _context.Students
                    .Where(s => s.IsActive && !s.StudentParents.Any())
                    .Select(s => s.StudentId)
                    .ToListAsync();

                return ServiceResult<List<Guid>>.Success(orphanedStudentIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orphaned students");
                return ServiceResult<List<Guid>>.Failure("An error occurred while retrieving orphaned students.");
            }
        }

        #endregion

        #region Helper Methods

        private async Task UnsetOtherPrimaryContactsAsync(Guid studentId)
        {
            var existingPrimaryContacts = await _context.StudentParents
                .Where(sp => sp.StudentId == studentId && sp.IsPrimaryContact)
                .ToListAsync();

            foreach (var contact in existingPrimaryContacts)
            {
                contact.IsPrimaryContact = false;
                contact.LastModifiedDate = DateTime.UtcNow;
            }
        }

        private async Task<StudentParentDto> MapToStudentParentDtoAsync(StudentParent studentParent)
        {
            // Load related entities if not already loaded
            if (studentParent.Student?.User == null)
            {
                await _context.Entry(studentParent)
                    .Reference(sp => sp.Student)
                    .Query()
                    .Include(s => s.User)
                    .Include(s => s.Class)
                    .LoadAsync();
            }

            if (studentParent.Parent?.User == null)
            {
                await _context.Entry(studentParent)
                    .Reference(sp => sp.Parent)
                    .Query()
                    .Include(p => p.User)
                    .LoadAsync();
            }

            return new StudentParentDto
            {
                StudentParentId = studentParent.StudentParentId,
                StudentId = studentParent.StudentId,
                ParentId = studentParent.ParentId,
                RelationshipToStudent = studentParent.RelationshipToStudent,
                IsPrimaryContact = studentParent.IsPrimaryContact,
                IsEmergencyContact = studentParent.IsEmergencyContact,
                CanPickupStudent = studentParent.CanPickupStudent,
                CreatedDate = studentParent.CreatedDate,
                LastModifiedDate = studentParent.LastModifiedDate,
                StudentNumber = studentParent.Student.StudentNumber,
                StudentFullName = studentParent.Student.User.FullName,
                StudentEmail = studentParent.Student.User.Email,
                ClassName = studentParent.Student.Class?.ClassName,
                StudentIsActive = studentParent.Student.IsActive,
                ParentFullName = studentParent.Parent.User.FullName,
                ParentEmail = studentParent.Parent.User.Email,
                ParentPhoneNumber = studentParent.Parent.User.PhoneNumber,
                ParentOccupation = studentParent.Parent.Occupation,
                ParentIsActive = studentParent.Parent.IsActive
            };
        }

        #endregion
    }
}
