using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManager.DTOs;
using SchoolManager.Models;
using SchoolManager.Services;
using System.Security.Claims;

namespace SchoolManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;
        private readonly IAuthenticationService _authService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            IPasswordService passwordService,
            IAuthenticationService authService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _passwordService = passwordService;
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Get paginated list of users
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "ViewUsers")]
        public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var users = await _userService.GetUsersAsync(page, pageSize);
                var userDtos = users.Select(MapToUserDto).ToList();

                var result = new PagedResult<UserDto>
                {
                    Items = userDtos,
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = userDtos.Count // In real implementation, get actual count
                };

                return Ok(new ApiResponse<PagedResult<UserDto>>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new ApiResponse<PagedResult<UserDto>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving users"
                });
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize(Policy = "ViewUsers")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUser(Guid id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                var userDto = MapToUserDto(user);
                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = userDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user"
                });
            }
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUserProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "User profile not found"
                    });
                }

                var userDto = MapToUserDto(user);
                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Data = userDto
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user profile");
                return StatusCode(500, new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "An error occurred while retrieving profile"
                });
            }
        }

        /// <summary>
        /// Create new user
        /// </summary>
        [HttpPost]
        [Authorize(Policy = "CreateUsers")]
        public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                // Validate password policy
                var isValidPassword = await _passwordService.ValidatePasswordPolicyAsync(request.Password);
                if (!isValidPassword)
                {
                    return BadRequest(new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "Password does not meet policy requirements"
                    });
                }

                var user = new ApplicationUser
                {
                    Email = request.Email,
                    UserName = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    MiddleName = request.MiddleName,
                    PhoneNumber = request.PhoneNumber,
                    Gender = request.Gender,
                    DateOfBirth = request.DateOfBirth,
                    Address = request.Address,
                    City = request.City,
                    State = request.State,
                    PostalCode = request.PostalCode,
                    Country = request.Country,
                    AlternatePhoneNumber = request.AlternatePhoneNumber,
                    CreatedBy = GetCurrentUserId().ToString()
                };

                var result = await _userService.CreateUserAsync(user, request.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} created successfully", request.Email);
                    var userDto = MapToUserDto(user);
                    return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new ApiResponse<UserDto>
                    {
                        Success = true,
                        Data = userDto,
                        Message = "User created successfully"
                    });
                }

                return BadRequest(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Failed to create user",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "An error occurred while creating user"
                });
            }
        }

        /// <summary>
        /// Update user
        /// </summary>
        [HttpPut("{id:guid}")]
        [Authorize(Policy = "UpdateUsers")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new ApiResponse<UserDto>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                // Update user properties
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.MiddleName = request.MiddleName;
                user.PhoneNumber = request.PhoneNumber;
                user.Gender = request.Gender;
                user.DateOfBirth = request.DateOfBirth;
                user.Address = request.Address;
                user.City = request.City;
                user.State = request.State;
                user.PostalCode = request.PostalCode;
                user.Country = request.Country;
                user.AlternatePhoneNumber = request.AlternatePhoneNumber;
                user.IsActive = request.IsActive;
                user.LastModifiedDate = DateTime.UtcNow;
                user.LastModifiedBy = GetCurrentUserId().ToString();

                var result = await _userService.UpdateUserAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} updated successfully", id);
                    var userDto = MapToUserDto(user);
                    return Ok(new ApiResponse<UserDto>
                    {
                        Success = true,
                        Data = userDto,
                        Message = "User updated successfully"
                    });
                }

                return BadRequest(new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "Failed to update user",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, new ApiResponse<UserDto>
                {
                    Success = false,
                    Message = "An error occurred while updating user"
                });
            }
        }

        /// <summary>
        /// Delete user
        /// </summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "DeleteUsers")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(Guid id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} deleted successfully", id);
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "User deleted successfully"
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to delete user",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while deleting user"
                });
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("{id:guid}/change-password")]
        [Authorize(Policy = "ChangePasswords")]
        public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
        {
            try
            {
                // Validate password policy
                var isValidPassword = await _passwordService.ValidatePasswordPolicyAsync(request.NewPassword);
                if (!isValidPassword)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "New password does not meet policy requirements"
                    });
                }

                var result = await _userService.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password changed successfully for user {UserId}", id);
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Password changed successfully"
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to change password",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while changing password"
                });
            }
        }

        /// <summary>
        /// Lock user account
        /// </summary>
        [HttpPost("{id:guid}/lock")]
        [Authorize(Policy = "LockUsers")]
        public async Task<ActionResult<ApiResponse<bool>>> LockUser(Guid id, [FromBody] LockUserRequest request)
        {
            try
            {
                var result = await _authService.LockUserAccountAsync(id, request.Reason, request.LockoutEnd);

                if (result)
                {
                    _logger.LogInformation("User {UserId} locked successfully", id);
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "User account locked successfully"
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to lock user account"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user {UserId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while locking user account"
                });
            }
        }

        /// <summary>
        /// Unlock user account
        /// </summary>
        [HttpPost("{id:guid}/unlock")]
        [Authorize(Policy = "UnlockUsers")]
        public async Task<ActionResult<ApiResponse<bool>>> UnlockUser(Guid id)
        {
            try
            {
                var result = await _authService.UnlockUserAccountAsync(id);

                if (result)
                {
                    _logger.LogInformation("User {UserId} unlocked successfully", id);
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "User account unlocked successfully"
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to unlock user account"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user {UserId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while unlocking user account"
                });
            }
        }

        /// <summary>
        /// Check if user account is locked
        /// </summary>
        [HttpGet("{id:guid}/lock-status")]
        [Authorize(Policy = "ViewUsers")]
        public async Task<ActionResult<ApiResponse<bool>>> IsAccountLocked(Guid id)
        {
            try
            {
                var isLocked = await _authService.IsAccountLockedAsync(id);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = isLocked,
                    Message = isLocked ? "Account is locked" : "Account is not locked"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking lock status for user {UserId}", id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while checking account lock status"
                });
            }
        }

        /// <summary>
        /// Get user permissions
        /// </summary>
        [HttpGet("{id:guid}/permissions")]
        [Authorize(Policy = "ViewPermissions")]
        public async Task<ActionResult<ApiResponse<List<string>>>> GetUserPermissions(Guid id)
        {
            try
            {
                var permissions = await _userService.GetUserPermissionsAsync(id);
                return Ok(new ApiResponse<List<string>>
                {
                    Success = true,
                    Data = permissions,
                    Message = "User permissions retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions for user {UserId}", id);
                return StatusCode(500, new ApiResponse<List<string>>
                {
                    Success = false,
                    Message = "An error occurred while retrieving user permissions"
                });
            }
        }

        /// <summary>
        /// Check if user has specific permission
        /// </summary>
        [HttpGet("{id:guid}/permissions/{permission}")]
        [Authorize(Policy = "ViewPermissions")]
        public async Task<ActionResult<ApiResponse<bool>>> HasPermission(Guid id, string permission)
        {
            try
            {
                var hasPermission = await _userService.HasPermissionAsync(id, permission);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = hasPermission,
                    Message = hasPermission ? "User has permission" : "User does not have permission"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission {Permission} for user {UserId}", permission, id);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while checking user permission"
                });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private UserDto MapToUserDto(ApplicationUser user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                MiddleName = user.MiddleName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                City = user.City,
                State = user.State,
                PostalCode = user.PostalCode,
                Country = user.Country,
                AlternatePhoneNumber = user.AlternatePhoneNumber,
                ProfilePictureUrl = user.ProfilePictureUrl,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                LastLoginDate = user.LastLoginDate,
                // These would be populated from service calls in real implementation
                Roles = new List<string>(),
                Permissions = new List<string>()
            };
        }
    }
}
