using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.DTOs;
using SchoolManager.Models;
using SchoolManager.Models.Common;

namespace SchoolManager.Services.Implementations
{
    /// <summary>
    /// Service implementation for managing parent operations
    /// </summary>
    public class ParentService : IParentService
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<ParentService> _logger;
        private readonly IUserService _userService;

        public ParentService(
            SchoolManagementDbContext context,
            ILogger<ParentService> logger,
            IUserService userService)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
        }

        // ===============================
        // BASIC CRUD OPERATIONS
        // ===============================

        public async Task<ServiceResult<ParentDto>> CreateParentAsync(CreateParentDto createParentDto)
        {
            try
            {
                _logger.LogInformation("Creating parent for user {UserId}", createParentDto.UserId);

                // Validate that user exists and is not already a parent
                var userExists = await _userService.UserExistsAsync(createParentDto.UserId);
                if (!userExists)
                {
                    return ServiceResult<ParentDto>.Failure("User not found", ErrorCodes.NotFound);
                }

                var parentExists = await ParentExistsByUserIdAsync(createParentDto.UserId);
                if (parentExists.IsSuccess && parentExists.Data)
                {
                    return ServiceResult<ParentDto>.Failure("Parent profile already exists for this user", ErrorCodes.ParentAlreadyExists);
                }

                var parent = new Parent
                {
                    ParentId = Guid.NewGuid(),
                    UserId = createParentDto.UserId,
                    Occupation = createParentDto.Occupation?.Trim(),
                    WorkAddress = createParentDto.WorkAddress?.Trim(),
                    WorkPhoneNumber = createParentDto.WorkPhoneNumber?.Trim(),
                    AnnualIncome = createParentDto.AnnualIncome,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Parents.Add(parent);
                await _context.SaveChangesAsync();

                var parentDto = await GetParentDtoAsync(parent.ParentId);
                if (parentDto == null)
                {
                    return ServiceResult<ParentDto>.Failure("Failed to retrieve created parent", ErrorCodes.InternalError);
                }

                _logger.LogInformation("Successfully created parent {ParentId} for user {UserId}", parent.ParentId, createParentDto.UserId);
                return ServiceResult<ParentDto>.Success(parentDto, "Parent created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating parent for user {UserId}", createParentDto.UserId);
                return ServiceResult<ParentDto>.Failure("An error occurred while creating the parent", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<ParentDto>> GetParentByIdAsync(Guid parentId)
        {
            try
            {
                var parentDto = await GetParentDtoAsync(parentId);
                if (parentDto == null)
                {
                    return ServiceResult<ParentDto>.Failure("Parent not found", ErrorCodes.ParentNotFound);
                }

                return ServiceResult<ParentDto>.Success(parentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parent {ParentId}", parentId);
                return ServiceResult<ParentDto>.Failure("An error occurred while retrieving the parent", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<ParentDto>> GetParentByUserIdAsync(Guid userId)
        {
            try
            {
                var parent = await _context.Parents
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (parent == null)
                {
                    return ServiceResult<ParentDto>.Failure("Parent not found", ErrorCodes.ParentNotFound);
                }

                var parentDto = MapToParentDto(parent);
                return ServiceResult<ParentDto>.Success(parentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parent by user ID {UserId}", userId);
                return ServiceResult<ParentDto>.Failure("An error occurred while retrieving the parent", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<ParentDto>> UpdateParentAsync(Guid parentId, UpdateParentDto updateParentDto)
        {
            try
            {
                _logger.LogInformation("Updating parent {ParentId}", parentId);

                var parent = await _context.Parents.FindAsync(parentId);
                if (parent == null)
                {
                    return ServiceResult<ParentDto>.Failure("Parent not found", ErrorCodes.ParentNotFound);
                }

                // Update properties
                parent.Occupation = updateParentDto.Occupation?.Trim();
                parent.WorkAddress = updateParentDto.WorkAddress?.Trim();
                parent.WorkPhoneNumber = updateParentDto.WorkPhoneNumber?.Trim();
                parent.AnnualIncome = updateParentDto.AnnualIncome;
                parent.IsActive = updateParentDto.IsActive;
                parent.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var parentDto = await GetParentDtoAsync(parentId);
                if (parentDto == null)
                {
                    return ServiceResult<ParentDto>.Failure("Failed to retrieve updated parent", ErrorCodes.InternalError);
                }

                _logger.LogInformation("Successfully updated parent {ParentId}", parentId);
                return ServiceResult<ParentDto>.Success(parentDto, "Parent updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating parent {ParentId}", parentId);
                return ServiceResult<ParentDto>.Failure("An error occurred while updating the parent", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<bool>> DeleteParentAsync(Guid parentId)
        {
            try
            {
                _logger.LogInformation("Deleting parent {ParentId}", parentId);

                var parent = await _context.Parents
                    .Include(p => p.StudentParents)
                    .FirstOrDefaultAsync(p => p.ParentId == parentId);

                if (parent == null)
                {
                    return ServiceResult<bool>.Failure("Parent not found", ErrorCodes.ParentNotFound);
                }

                // Check if parent has active relationships with students
                if (parent.StudentParents.Any())
                {
                    return ServiceResult<bool>.Failure("Cannot delete parent with active student relationships", ErrorCodes.ParentHasActiveChildren);
                }

                _context.Parents.Remove(parent);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted parent {ParentId}", parentId);
                return ServiceResult<bool>.Success(true, "Parent deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting parent {ParentId}", parentId);
                return ServiceResult<bool>.Failure("An error occurred while deleting the parent", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<bool>> DeactivateParentAsync(Guid parentId)
        {
            try
            {
                _logger.LogInformation("Deactivating parent {ParentId}", parentId);

                var parent = await _context.Parents.FindAsync(parentId);
                if (parent == null)
                {
                    return ServiceResult<bool>.Failure("Parent not found", ErrorCodes.ParentNotFound);
                }

                parent.IsActive = false;
                parent.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deactivated parent {ParentId}", parentId);
                return ServiceResult<bool>.Success(true, "Parent deactivated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating parent {ParentId}", parentId);
                return ServiceResult<bool>.Failure("An error occurred while deactivating the parent", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<bool>> ActivateParentAsync(Guid parentId)
        {
            try
            {
                _logger.LogInformation("Activating parent {ParentId}", parentId);

                var parent = await _context.Parents.FindAsync(parentId);
                if (parent == null)
                {
                    return ServiceResult<bool>.Failure("Parent not found", ErrorCodes.ParentNotFound);
                }

                parent.IsActive = true;
                parent.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully activated parent {ParentId}", parentId);
                return ServiceResult<bool>.Success(true, "Parent activated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating parent {ParentId}", parentId);
                return ServiceResult<bool>.Failure("An error occurred while activating the parent", ErrorCodes.InternalError);
            }
        }

        // ===============================
        // QUERY OPERATIONS
        // ===============================

        public async Task<ServiceResult<PagedResult2<ParentSummaryDto>>> GetParentsAsync(int page = 1, int pageSize = 10, string? searchTerm = null, bool? isActive = null)
        {
            try
            {
                var query = _context.Parents
                    .Include(p => p.User)
                    .Include(p => p.StudentParents)
                    .AsQueryable();

                // Apply filters
                if (isActive.HasValue)
                {
                    query = query.Where(p => p.IsActive == isActive.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    query = query.Where(p =>
                        p.User.FirstName.ToLower().Contains(searchLower) ||
                        p.User.LastName.ToLower().Contains(searchLower) ||
                        p.User.Email.ToLower().Contains(searchLower) ||
                        (p.Occupation != null && p.Occupation.ToLower().Contains(searchLower)));
                }

                var totalCount = await query.CountAsync();
                var parents = await query
                    .OrderBy(p => p.User.LastName)
                    .ThenBy(p => p.User.FirstName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new ParentSummaryDto
                    {
                        ParentId = p.ParentId,
                        FullName = $"{p.User.FirstName} {p.User.MiddleName} {p.User.LastName}".Replace("  ", " ").Trim(),
                        Email = p.User.Email,
                        PhoneNumber = p.User.PhoneNumber,
                        Occupation = p.Occupation,
                        IsActive = p.IsActive,
                        NumberOfChildren = p.StudentParents.Count
                    })
                    .ToListAsync();

                var pagedResult = PagedResult2<ParentSummaryDto>.Create(parents, totalCount, page, pageSize);
                return ServiceResult<PagedResult2<ParentSummaryDto>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parents page {Page}", page);
                return ServiceResult<PagedResult2<ParentSummaryDto>>.Failure("An error occurred while retrieving parents", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<List<ParentSummaryDto>>> GetActiveParentsAsync()
        {
            try
            {
                var parents = await _context.Parents
                    .Include(p => p.User)
                    .Include(p => p.StudentParents)
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.User.LastName)
                    .ThenBy(p => p.User.FirstName)
                    .Select(p => new ParentSummaryDto
                    {
                        ParentId = p.ParentId,
                        FullName = $"{p.User.FirstName} {p.User.MiddleName} {p.User.LastName}".Replace("  ", " ").Trim(),
                        Email = p.User.Email,
                        PhoneNumber = p.User.PhoneNumber,
                        Occupation = p.Occupation,
                        IsActive = p.IsActive,
                        NumberOfChildren = p.StudentParents.Count
                    })
                    .ToListAsync();

                return ServiceResult<List<ParentSummaryDto>>.Success(parents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active parents");
                return ServiceResult<List<ParentSummaryDto>>.Failure("An error occurred while retrieving active parents", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<ParentWithChildrenDto>> GetParentWithChildrenAsync(Guid parentId)
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
                    return ServiceResult<ParentWithChildrenDto>.Failure("Parent not found", ErrorCodes.ParentNotFound);
                }

                var parentDto = new ParentWithChildrenDto
                {
                    ParentId = parent.ParentId,
                    UserId = parent.UserId,
                    Occupation = parent.Occupation,
                    WorkAddress = parent.WorkAddress,
                    WorkPhoneNumber = parent.WorkPhoneNumber,
                    AnnualIncome = parent.AnnualIncome,
                    IsActive = parent.IsActive,
                    CreatedDate = parent.CreatedDate,
                    LastModifiedDate = parent.LastModifiedDate,
                    FirstName = parent.User.FirstName,
                    LastName = parent.User.LastName,
                    MiddleName = parent.User.MiddleName,
                    FullName = parent.User.FullName,
                    Email = parent.User.Email,
                    PhoneNumber = parent.User.PhoneNumber,
                    Gender = parent.User.Gender,
                    DateOfBirth = parent.User.DateOfBirth,
                    Address = parent.User.Address,
                    City = parent.User.City,
                    State = parent.User.State,
                    PostalCode = parent.User.PostalCode,
                    Country = parent.User.Country,
                    AlternatePhoneNumber = parent.User.AlternatePhoneNumber,
                    ProfilePictureUrl = parent.User.ProfilePictureUrl,
                    Children = parent.StudentParents.Select(sp => new ParentChildDto
                    {
                        StudentId = sp.StudentId,
                        StudentNumber = sp.Student.StudentNumber,
                        FullName = sp.Student.User.FullName,
                        RelationshipToStudent = sp.RelationshipToStudent,
                        IsPrimaryContact = sp.IsPrimaryContact,
                        IsEmergencyContact = sp.IsEmergencyContact,
                        CanPickupStudent = sp.CanPickupStudent,
                        ClassName = sp.Student.Class?.ClassName,
                        IsActive = sp.Student.IsActive
                    }).ToList()
                };

                return ServiceResult<ParentWithChildrenDto>.Success(parentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parent with children {ParentId}", parentId);
                return ServiceResult<ParentWithChildrenDto>.Failure("An error occurred while retrieving parent with children", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<List<ParentSummaryDto>>> GetParentsByOccupationAsync(string occupation)
        {
            try
            {
                var parents = await _context.Parents
                    .Include(p => p.User)
                    .Include(p => p.StudentParents)
                    .Where(p => p.Occupation != null && p.Occupation.ToLower() == occupation.ToLower())
                    .OrderBy(p => p.User.LastName)
                    .ThenBy(p => p.User.FirstName)
                    .Select(p => new ParentSummaryDto
                    {
                        ParentId = p.ParentId,
                        FullName = $"{p.User.FirstName} {p.User.MiddleName} {p.User.LastName}".Replace("  ", " ").Trim(),
                        Email = p.User.Email,
                        PhoneNumber = p.User.PhoneNumber,
                        Occupation = p.Occupation,
                        IsActive = p.IsActive,
                        NumberOfChildren = p.StudentParents.Count
                    })
                    .ToListAsync();

                return ServiceResult<List<ParentSummaryDto>>.Success(parents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving parents by occupation {Occupation}", occupation);
                return ServiceResult<List<ParentSummaryDto>>.Failure("An error occurred while retrieving parents by occupation", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<List<ParentSummaryDto>>> SearchParentsAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return ServiceResult<List<ParentSummaryDto>>.Success(new List<ParentSummaryDto>());
                }

                var searchLower = searchTerm.ToLower();
                var parents = await _context.Parents
                    .Include(p => p.User)
                    .Include(p => p.StudentParents)
                    .Where(p =>
                        p.User.FirstName.ToLower().Contains(searchLower) ||
                        p.User.LastName.ToLower().Contains(searchLower) ||
                        p.User.Email.ToLower().Contains(searchLower) ||
                        (p.Occupation != null && p.Occupation.ToLower().Contains(searchLower)) ||
                        (p.User.PhoneNumber != null && p.User.PhoneNumber.Contains(searchTerm)))
                    .OrderBy(p => p.User.LastName)
                    .ThenBy(p => p.User.FirstName)
                    .Select(p => new ParentSummaryDto
                    {
                        ParentId = p.ParentId,
                        FullName = $"{p.User.FirstName} {p.User.MiddleName} {p.User.LastName}".Replace("  ", " ").Trim(),
                        Email = p.User.Email,
                        PhoneNumber = p.User.PhoneNumber,
                        Occupation = p.Occupation,
                        IsActive = p.IsActive,
                        NumberOfChildren = p.StudentParents.Count
                    })
                    .ToListAsync();

                return ServiceResult<List<ParentSummaryDto>>.Success(parents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching parents with term {SearchTerm}", searchTerm);
                return ServiceResult<List<ParentSummaryDto>>.Failure("An error occurred while searching parents", ErrorCodes.InternalError);
            }
        }

        // ===============================
        // VALIDATION OPERATIONS
        // ===============================

        public async Task<ServiceResult<bool>> ParentExistsAsync(Guid parentId)
        {
            try
            {
                var exists = await _context.Parents.AnyAsync(p => p.ParentId == parentId);
                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if parent exists {ParentId}", parentId);
                return ServiceResult<bool>.Failure("An error occurred while checking parent existence", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<bool>> ParentExistsByUserIdAsync(Guid userId)
        {
            try
            {
                var exists = await _context.Parents.AnyAsync(p => p.UserId == userId);
                return ServiceResult<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if parent exists by user ID {UserId}", userId);
                return ServiceResult<bool>.Failure("An error occurred while checking parent existence", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<bool>> IsParentActiveAsync(Guid parentId)
        {
            try
            {
                var parent = await _context.Parents.FindAsync(parentId);
                if (parent == null)
                {
                    return ServiceResult<bool>.Failure("Parent not found", ErrorCodes.ParentNotFound);
                }

                return ServiceResult<bool>.Success(parent.IsActive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if parent is active {ParentId}", parentId);
                return ServiceResult<bool>.Failure("An error occurred while checking parent status", ErrorCodes.InternalError);
            }
        }

        // ===============================
        // STATISTICS OPERATIONS
        // ===============================

        public async Task<ServiceResult<int>> GetTotalParentsCountAsync()
        {
            try
            {
                var count = await _context.Parents.CountAsync();
                return ServiceResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total parents count");
                return ServiceResult<int>.Failure("An error occurred while getting parents count", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<int>> GetActiveParentsCountAsync()
        {
            try
            {
                var count = await _context.Parents.CountAsync(p => p.IsActive);
                return ServiceResult<int>.Success(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active parents count");
                return ServiceResult<int>.Failure("An error occurred while getting active parents count", ErrorCodes.InternalError);
            }
        }

        public async Task<ServiceResult<Dictionary<string, int>>> GetParentsByOccupationStatsAsync()
        {
            try
            {
                var stats = await _context.Parents
                    .Where(p => p.IsActive && !string.IsNullOrEmpty(p.Occupation))
                    .GroupBy(p => p.Occupation)
                    .Select(g => new { Occupation = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToDictionaryAsync(x => x.Occupation!, x => x.Count);

                return ServiceResult<Dictionary<string, int>>.Success(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parents by occupation statistics");
                return ServiceResult<Dictionary<string, int>>.Failure("An error occurred while getting occupation statistics", ErrorCodes.InternalError);
            }
        }

        // ===============================
        // PRIVATE HELPER METHODS
        // ===============================

        private async Task<ParentDto?> GetParentDtoAsync(Guid parentId)
        {
            var parent = await _context.Parents
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.ParentId == parentId);

            return parent == null ? null : MapToParentDto(parent);
        }

        private static ParentDto MapToParentDto(Parent parent)
        {
            return new ParentDto
            {
                ParentId = parent.ParentId,
                UserId = parent.UserId,
                Occupation = parent.Occupation,
                WorkAddress = parent.WorkAddress,
                WorkPhoneNumber = parent.WorkPhoneNumber,
                AnnualIncome = parent.AnnualIncome,
                IsActive = parent.IsActive,
                CreatedDate = parent.CreatedDate,
                LastModifiedDate = parent.LastModifiedDate,
                FirstName = parent.User.FirstName,
                LastName = parent.User.LastName,
                MiddleName = parent.User.MiddleName,
                FullName = parent.User.FullName,
                Email = parent.User.Email,
                PhoneNumber = parent.User.PhoneNumber,
                Gender = parent.User.Gender,
                DateOfBirth = parent.User.DateOfBirth,
                Address = parent.User.Address,
                City = parent.User.City,
                State = parent.User.State,
                PostalCode = parent.User.PostalCode,
                Country = parent.User.Country,
                AlternatePhoneNumber = parent.User.AlternatePhoneNumber,
                ProfilePictureUrl = parent.User.ProfilePictureUrl
            };
        }
    }
}
