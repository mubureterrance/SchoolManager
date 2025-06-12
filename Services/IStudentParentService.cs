
using SchoolManager.DTOs;
using SchoolManager.Enums;
using SchoolManager.Models.Common;

namespace SchoolManager.Services
{
    public interface IStudentParentService
    {
        // Basic CRUD Operations
        Task<ServiceResult<StudentParentDto>> CreateStudentParentRelationshipAsync(CreateStudentParentDto createDto);
        Task<ServiceResult<StudentParentDto>> GetStudentParentRelationshipAsync(Guid studentParentId);
        Task<ServiceResult<StudentParentDto>> UpdateStudentParentRelationshipAsync(Guid studentParentId, UpdateStudentParentDto updateDto);
        Task<ServiceResult<bool>> DeleteStudentParentRelationshipAsync(Guid studentParentId);

        // Student-Parent Relationship Queries
        Task<ServiceResult<StudentWithParentsDto>> GetStudentWithParentsAsync(Guid studentId);
        Task<ServiceResult<ParentWithStudentsDto>> GetParentWithStudentsAsync(Guid parentId);
        Task<ServiceResult<List<StudentParentRelationshipDto>>> GetParentsByStudentIdAsync(Guid studentId);
        Task<ServiceResult<List<ParentStudentRelationshipDto>>> GetStudentsByParentIdAsync(Guid parentId);

        // Contact Management
        Task<ServiceResult<List<EmergencyContactDto>>> GetEmergencyContactsForStudentAsync(Guid studentId);
        Task<ServiceResult<EmergencyContactDto>> GetPrimaryContactForStudentAsync(Guid studentId);
        Task<ServiceResult<List<EmergencyContactDto>>> GetAuthorizedPickupContactsAsync(Guid studentId);
        Task<ServiceResult<bool>> UpdateContactPermissionsAsync(Guid studentParentId, ContactPermissionsDto permissionsDto);

        // Relationship Management
        Task<ServiceResult<bool>> SetPrimaryContactAsync(Guid studentId, Guid parentId);
        Task<ServiceResult<bool>> AddEmergencyContactAsync(Guid studentId, Guid parentId);
        Task<ServiceResult<bool>> RemoveEmergencyContactAsync(Guid studentId, Guid parentId);
        Task<ServiceResult<bool>> UpdatePickupPermissionAsync(Guid studentParentId, bool canPickup);

        // Validation Operations
        Task<ServiceResult<bool>> RelationshipExistsAsync(Guid studentId, Guid parentId);
        Task<ServiceResult<bool>> CanParentPickupStudentAsync(Guid studentId, Guid parentId);
        Task<ServiceResult<bool>> IsParentPrimaryContactAsync(Guid studentId, Guid parentId);
        Task<ServiceResult<bool>> IsParentEmergencyContactAsync(Guid studentId, Guid parentId);

        // Bulk Operations
        Task<ServiceResult<List<StudentParentDto>>> CreateMultipleRelationshipsAsync(List<CreateStudentParentDto> createDtos);
        Task<ServiceResult<bool>> TransferStudentToNewParentAsync(Guid studentId, Guid oldParentId, Guid newParentId, ParentRelationship relationship);
        Task<ServiceResult<bool>> UpdateMultipleContactPermissionsAsync(List<ContactPermissionsDto> permissionsList);

        // Search and Filter Operations
        Task<ServiceResult<List<StudentParentDto>>> GetRelationshipsByRelationshipTypeAsync(ParentRelationship relationshipType);
        Task<ServiceResult<List<StudentWithParentsDto>>> GetStudentsWithoutPrimaryContactAsync();
        Task<ServiceResult<List<StudentWithParentsDto>>> GetStudentsWithoutEmergencyContactAsync();
        Task<ServiceResult<List<ParentWithStudentsDto>>> GetParentsWithMultipleChildrenAsync();

        // Statistics Operations
        Task<ServiceResult<Dictionary<ParentRelationship, int>>> GetRelationshipTypeStatsAsync();
        Task<ServiceResult<int>> GetTotalRelationshipsCountAsync();
        Task<ServiceResult<int>> GetOrphanedStudentsCountAsync(); // Students without any parent relationships
        Task<ServiceResult<List<Guid>>> GetOrphanedStudentsAsync();
    }
}
