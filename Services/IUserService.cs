using Microsoft.AspNetCore.Identity;
using SchoolManager.Models;

namespace SchoolManager.Services
{
    public interface IUserService
    {
        Task<ApplicationUser?> GetUserByIdAsync(Guid userId);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<ApplicationUser?> GetUserByPhoneAsync(string phoneNumber);
        Task<IEnumerable<ApplicationUser>> GetUsersAsync(int page = 1, int pageSize = 10);
        Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password);
        Task<IdentityResult> UpdateUserAsync(ApplicationUser user);
        Task<IdentityResult> DeleteUserAsync(Guid userId);
        Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
        Task<IdentityResult> ResetPasswordAsync(Guid userId, string token, string newPassword);
        Task<string> GeneratePasswordResetTokenAsync(Guid userId);
        Task<bool> ValidatePasswordAsync(ApplicationUser user, string password);
        Task<List<string>> GetUserPermissionsAsync(Guid userId);
        Task<bool> HasPermissionAsync(Guid userId, string permission);
    }
}
