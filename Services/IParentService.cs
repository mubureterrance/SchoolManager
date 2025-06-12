using SchoolManager.DTOs;
using SchoolManager.Models.Common;

namespace SchoolManager.Services
{
    public interface IParentService
    {
        // Basic CRUD Operations
        Task<ServiceResult<ParentDto>> CreateParentAsync(CreateParentDto createParentDto);
        Task<ServiceResult<ParentDto>> GetParentByIdAsync(Guid parentId);
        Task<ServiceResult<ParentDto>> GetParentByUserIdAsync(Guid userId);
        Task<ServiceResult<ParentDto>> UpdateParentAsync(Guid parentId, UpdateParentDto updateParentDto);
        Task<ServiceResult<bool>> PermanentlyDeleteParentAsync(Guid parentId);
        Task<ServiceResult<bool>> DeactivateParentAsync(Guid parentId);
        Task<ServiceResult<bool>> ActivateParentAsync(Guid parentId);

        // Query Operations
        Task<ServiceResult<PagedResult2<ParentSummaryDto>>> GetParentsAsync(int page = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null);
        Task<ServiceResult<List<ParentSummaryDto>>> GetActiveParentsAsync();
        Task<ServiceResult<ParentWithChildrenDto>> GetParentWithChildrenAsync(Guid parentId);
        Task<ServiceResult<List<ParentSummaryDto>>> GetParentsByOccupationAsync(string occupation);
        Task<ServiceResult<List<ParentSummaryDto>>> SearchParentsAsync(string searchTerm);

        // Validation Operations
        Task<ServiceResult<bool>> ParentExistsAsync(Guid parentId);
        Task<ServiceResult<bool>> ParentExistsByUserIdAsync(Guid userId);
        Task<ServiceResult<bool>> IsParentActiveAsync(Guid parentId);

        // Statistics Operations
        Task<ServiceResult<int>> GetTotalParentsCountAsync();
        Task<ServiceResult<int>> GetActiveParentsCountAsync();
        Task<ServiceResult<Dictionary<string, int>>> GetParentsByOccupationStatsAsync();
    }
}
