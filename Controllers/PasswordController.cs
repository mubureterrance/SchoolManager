using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolManager.DTOs;
using SchoolManager.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SchoolManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PasswordController : ControllerBase
    {
        private readonly IPasswordService _passwordService;
        private readonly IUserService _userService;
        private readonly ILogger<PasswordController> _logger;

        public PasswordController(
            IPasswordService passwordService,
            IUserService userService,
            ILogger<PasswordController> logger)
        {
            _passwordService = passwordService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Validate password against policy
        /// </summary>
        [HttpPost("validate-policy")]
        public async Task<ActionResult<ApiResponse<bool>>> ValidatePasswordPolicy([FromBody] string password)
        {
            try
            {
                var isValid = await _passwordService.ValidatePasswordPolicyAsync(password);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = isValid,
                    Message = isValid ? "Password meets policy requirements" : "Password does not meet policy requirements"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password policy");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while validating password policy"
                });
            }
        }

        /// <summary>
        /// Check if password has been reused
        /// </summary>
        [HttpPost("check-reuse")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckPasswordReuse([FromBody] CheckPasswordReuseRequest request)
        {
            try
            {
                var isReused = await _passwordService.IsPasswordReusedAsync(request.UserId, request.Password);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = isReused,
                    Message = isReused ? "Password has been used recently" : "Password has not been used recently"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password reuse for user {UserId}", request.UserId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while checking password reuse"
                });
            }
        }

        /// <summary>
        /// Check if password is expired
        /// </summary>
        [HttpGet("check-expiration/{userId:guid}")]
        [Authorize(Policy = "ViewUsers")]
        public async Task<ActionResult<ApiResponse<bool>>> CheckPasswordExpiration(Guid userId)
        {
            try
            {
                var isExpired = await _passwordService.IsPasswordExpiredAsync(userId);
                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = isExpired,
                    Message = isExpired ? "Password is expired" : "Password is not expired"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password expiration for user {UserId}", userId);
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while checking password expiration"
                });
            }
        }

        /// <summary>
        /// Generate random password
        /// </summary>
        [HttpPost("generate")]
        [Authorize(Policy = "GeneratePasswords")]
        public async Task<ActionResult<ApiResponse<string>>> GenerateRandomPassword([FromQuery] int length = 12)
        {
            try
            {
                if (length < 8 || length > 128)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Password length must be between 8 and 128 characters"
                    });
                }

                var password = await _passwordService.GenerateRandomPasswordAsync(length);
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = password,
                    Message = "Random password generated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating random password");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while generating password"
                });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }
    }

    public class CheckPasswordReuseRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
