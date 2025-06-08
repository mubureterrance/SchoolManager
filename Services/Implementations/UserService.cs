using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SchoolManager.Data;
using SchoolManager.Models;

namespace SchoolManager.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly IPasswordService _passwordService;

        public UserService(
            UserManager<ApplicationUser> userManager,
            SchoolManagementDbContext context,
            ILogger<UserService> logger,
            IPasswordService passwordService)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _passwordService = passwordService;
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(Guid userId)
        {
            try
            {
                return await _userManager.FindByIdAsync(userId.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _userManager.FindByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return null;
            }
        }

        public async Task<ApplicationUser?> GetUserByPhoneAsync(string phoneNumber)
        {
            try
            {
                return await _context.Users
                    .FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by phone: {PhoneNumber}", phoneNumber);
                return null;
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                return await _context.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users page: {Page}, size: {PageSize}", page, pageSize);
                return Enumerable.Empty<ApplicationUser>();
            }
        }

        public async Task<IdentityResult> CreateUserAsync(ApplicationUser user, string password)
        {
            try
            {
                // Validate password policy
                if (!await _passwordService.ValidatePasswordPolicyAsync(password))
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "PasswordPolicy",
                        Description = "Password does not meet policy requirements"
                    });
                }

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    // Add password to history
                    await _passwordService.AddPasswordToHistoryAsync(user.Id, user.PasswordHash!);

                    _logger.LogInformation("User created successfully: {Email}", user.Email);
                }
                else
                {
                    _logger.LogWarning("Failed to create user: {Email}, Errors: {Errors}",
                        user.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Email}", user.Email);
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "CreateUserError",
                    Description = "An error occurred while creating the user"
                });
            }
        }

        public async Task<IdentityResult> UpdateUserAsync(ApplicationUser user)
        {
            try
            {
                user.LastModifiedDate = DateTime.UtcNow;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User updated successfully: {UserId}", user.Id);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "UpdateUserError",
                    Description = "An error occurred while updating the user"
                });
            }
        }

        public async Task<IdentityResult> DeleteUserAsync(Guid userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "UserNotFound",
                        Description = "User not found"
                    });
                }

                // Soft delete - mark as inactive
                user.IsActive = false;
                user.LastModifiedDate = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User deleted (soft delete): {UserId}", userId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "DeleteUserError",
                    Description = "An error occurred while deleting the user"
                });
            }
        }

        public async Task<IdentityResult> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "UserNotFound",
                        Description = "User not found"
                    });
                }

                // Validate new password policy
                if (!await _passwordService.ValidatePasswordPolicyAsync(newPassword))
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "PasswordPolicy",
                        Description = "New password does not meet policy requirements"
                    });
                }

                // Check password reuse
                if (await _passwordService.IsPasswordReusedAsync(userId, newPassword))
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "PasswordReused",
                        Description = "Cannot reuse a recent password"
                    });
                }

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

                if (result.Succeeded)
                {
                    // Add new password to history
                    await _passwordService.AddPasswordToHistoryAsync(userId, user.PasswordHash!);

                    // Update password change flag
                    user.RequirePasswordChange = false;
                    user.LastModifiedDate = DateTime.UtcNow;

                    // Update user in database
                    await _userManager.UpdateAsync(user);

                    // Log successful password change
                    _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                }
                else
                {
                    // Log failed password change attempt
                    _logger.LogWarning("Failed password change attempt for user {UserId}: {Errors}",
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password for user {UserId}", userId);
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InternalError",
                    Description = "An error occurred while changing the password"
                });
            }
        }

        public async Task<IdentityResult> ResetPasswordAsync(Guid userId, string token, string newPassword)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "UserNotFound",
                        Description = "User not found"
                    });
                }

                // Validate new password policy
                if (!await _passwordService.ValidatePasswordPolicyAsync(newPassword))
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "PasswordPolicy",
                        Description = "New password does not meet policy requirements"
                    });
                }

                // Check password reuse
                if (await _passwordService.IsPasswordReusedAsync(userId, newPassword))
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "PasswordReused",
                        Description = "Cannot reuse a recent password"
                    });
                }

                var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

                if (result.Succeeded)
                {
                    // Add new password to history
                    await _passwordService.AddPasswordToHistoryAsync(userId, user.PasswordHash!);

                    // Update password change flag and dates
                    user.RequirePasswordChange = false;
                    user.LastModifiedDate = DateTime.UtcNow;

                    // Update user in database
                    await _userManager.UpdateAsync(user);

                    _logger.LogInformation("Password reset successfully for user {UserId}", userId);
                }
                else
                {
                    _logger.LogWarning("Failed password reset attempt for user {UserId}: {Errors}",
                        userId, string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password for user {UserId}", userId);
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "InternalError",
                    Description = "An error occurred while resetting the password"
                });
            }
        }

        public async Task<string> GeneratePasswordResetTokenAsync(Guid userId)
        {
            try
            {
                var user = await GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Attempted to generate password reset token for non-existent user: {UserId}", userId);
                    throw new InvalidOperationException("User not found");
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                _logger.LogInformation("Password reset token generated for user {UserId}", userId);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating password reset token for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ValidatePasswordAsync(ApplicationUser user, string password)
        {
            try
            {
                if (user == null)
                {
                    return false;
                }

                return await _userManager.CheckPasswordAsync(user, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password for user {UserId}", user?.Id);
                return false;
            }
        }

        public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                            .ThenInclude(r => r.RolePermissions)
                                .ThenInclude(rp => rp.Permission)
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("User not found when getting permissions: {UserId}", userId);
                    return new List<string>();
                }

                var permissions = user.UserRoles
                    .Where(ur => ur.Role.IsActive)
                    .SelectMany(ur => ur.Role.RolePermissions)
                    .Where(rp => rp.Permission.IsActive)
                    .Select(rp => rp.Permission.PermissionName)
                    .Distinct()
                    .ToList();

                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions for user {UserId}", userId);
                return new List<string>();
            }
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string permission)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(permission))
                {
                    return false;
                }

                var userPermissions = await GetUserPermissionsAsync(userId);
                return userPermissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission, userId);
                return false;
            }
        }

    }
}
