using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(
            IAuthenticationService authService,
            UserManager<ApplicationUser> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }

        [HttpPost("login")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _authService.AuthenticateAsync(request.Email, request.Password, ipAddress);

                if (result == null)
                    return Unauthorized(new { message = "Invalid credentials" });

                return Ok(new
                {
                    Token = result.AccessToken,
                    RefreshToken = result.RefreshToken,
                    ExpiresIn = result.ExpiryDate
                });
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, new { message = "An error occurred during authentication" });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

            if (result == null)
                return Unauthorized(new { message = "Invalid token" });

            return Ok(new
            {
                Token = result.AccessToken,
                RefreshToken = result.RefreshToken,
                ExpiresIn = result.ExpiryDate
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userIdGuid))
                return BadRequest(new { message = "Invalid user context" });

            var sessionToken = User.FindFirstValue("sessionToken");
            await _authService.LogoutAsync(userIdGuid, sessionToken);

            // Clear any cookies if using them
            Response.Cookies.Delete("refreshToken");

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpPost("two-factor/generate")]
        [Authorize]
        public async Task<IActionResult> GenerateTwoFactorToken([FromQuery] string tokenType)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return BadRequest();

            var token = await _authService.GenerateTwoFactorTokenAsync(userIdGuid, tokenType);
            return Ok(new { Token = token });
        }

        [HttpPost("two-factor/validate")]
        [Authorize]
        public async Task<IActionResult> ValidateTwoFactorToken([FromBody] ValidateTokenRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var userIdGuid))
                return BadRequest();

            var isValid = await _authService.ValidateTwoFactorTokenAsync(userIdGuid, request.Token, request.TokenType);
            return Ok(new { IsValid = isValid });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // Implementation for password reset
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            // Implementation for password reset
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Implementation for user registration
        }
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }

    public class ValidateTokenRequest
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string TokenType { get; set; }
    }
}
