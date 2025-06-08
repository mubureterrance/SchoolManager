using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SchoolManager.DTOs;
using SchoolManager.Models;
using SchoolManager.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace SchoolManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IUserService _userService;
        private readonly IPasswordService _passwordService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthenticationService authService,
            IUserService userService,
            IPasswordService passwordService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _userService = userService;
            _passwordService = passwordService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate user with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthenticationResult>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var ipAddress = GetClientIpAddress();
                var result = await _authService.AuthenticateAsync(request.Email, request.Password, ipAddress);

                if (result.Success)
                {
                    _logger.LogInformation("User {Email} logged in successfully", request.Email);
                    return Ok(new ApiResponse<AuthenticationResult>
                    {
                        Success = true,
                        Data = result,
                        Message = "Login successful"
                    });
                }

                _logger.LogWarning("Failed login attempt for {Email}", request.Email);
                return BadRequest(new ApiResponse<AuthenticationResult>
                {
                    Success = false,
                    Message = result.ErrorMessage ?? "Login failed",
                    Errors = result.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", request.Email);
                return StatusCode(500, new ApiResponse<AuthenticationResult>
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        /// <summary>
        /// Refresh authentication token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<AuthenticationResult>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var ipAddress = GetClientIpAddress();
                var result = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

                if (result.Success)
                {
                    return Ok(new ApiResponse<AuthenticationResult>
                    {
                        Success = true,
                        Data = result,
                        Message = "Token refreshed successfully"
                    });
                }

                return BadRequest(new ApiResponse<AuthenticationResult>
                {
                    Success = false,
                    Message = result.ErrorMessage ?? "Token refresh failed",
                    Errors = result.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, new ApiResponse<AuthenticationResult>
                {
                    Success = false,
                    Message = "An error occurred during token refresh"
                });
            }
        }

        /// <summary>
        /// Logout current session
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> Logout()
        {
            try
            {
                var userId = GetCurrentUserId();
                var sessionToken = GetSessionToken();

                var result = await _authService.LogoutAsync(userId, sessionToken);

                if (result)
                {
                    _logger.LogInformation("User {UserId} logged out successfully", userId);
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Logout successful"
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Logout failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred during logout"
                });
            }
        }

        /// <summary>
        /// Logout all sessions for current user
        /// </summary>
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<bool>>> LogoutAllSessions()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _authService.LogoutAllSessionsAsync(userId);

                if (result)
                {
                    _logger.LogInformation("All sessions logged out for user {UserId}", userId);
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "All sessions logged out successfully"
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Failed to logout all sessions"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout all sessions");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred during logout"
                });
            }
        }

        /// <summary>
        /// Generate two-factor authentication token
        /// </summary>
        [HttpPost("two-factor/generate")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> GenerateTwoFactorToken([FromQuery] string tokenType = "SMS")
        {
            try
            {
                var userId = GetCurrentUserId();
                var token = await _authService.GenerateTwoFactorTokenAsync(userId, tokenType);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = token,
                    Message = $"Two-factor token sent via {tokenType}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating two-factor token");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while generating two-factor token"
                });
            }
        }

        /// <summary>
        /// Validate two-factor authentication token
        /// </summary>
        [HttpPost("two-factor/validate")]
        public async Task<ActionResult<ApiResponse<bool>>> ValidateTwoFactorToken([FromBody] TwoFactorRequest request)
        {
            try
            {
                var isValid = await _authService.ValidateTwoFactorTokenAsync(request.UserId, request.Token, request.TokenType);

                return Ok(new ApiResponse<bool>
                {
                    Success = true,
                    Data = isValid,
                    Message = isValid ? "Token is valid" : "Token is invalid"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating two-factor token");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred while validating token"
                });
            }
        }

        /// <summary>
        /// Request password reset token
        /// </summary>
        [HttpPost("password/reset-request")]
        public async Task<ActionResult<ApiResponse<string>>> RequestPasswordReset([FromBody] string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal if email exists
                    return Ok(new ApiResponse<string>
                    {
                        Success = true,
                        Message = "If the email exists, a reset link will be sent"
                    });
                }

                var token = await _userService.GeneratePasswordResetTokenAsync(user.Id);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Data = token,
                    Message = "Password reset token generated"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting password reset");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "An error occurred while processing password reset request"
                });
            }
        }

        /// <summary>
        /// Reset password with token
        /// </summary>
        [HttpPost("password/reset")]
        public async Task<ActionResult<ApiResponse<bool>>> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(request.Email);
                if (user == null)
                {
                    return BadRequest(new ApiResponse<bool>
                    {
                        Success = false,
                        Message = "Invalid reset request"
                    });
                }

                var result = await _userService.ResetPasswordAsync(user.Id, request.Token, request.NewPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset successful for user {Email}", request.Email);
                    return Ok(new ApiResponse<bool>
                    {
                        Success = true,
                        Data = true,
                        Message = "Password reset successful"
                    });
                }

                return BadRequest(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Password reset failed",
                    Errors = result.Errors.Select(e => e.Description).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset");
                return StatusCode(500, new ApiResponse<bool>
                {
                    Success = false,
                    Message = "An error occurred during password reset"
                });
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private string? GetSessionToken()
        {
            return Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        }

        private string GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
