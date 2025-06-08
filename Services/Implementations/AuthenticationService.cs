using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolManager.Data;
using SchoolManager.DTOs;
using SchoolManager.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SchoolManager.Services.Implementations
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPasswordService _passwordService;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            SchoolManagementDbContext context,
            ILogger<AuthenticationService> logger,
            IConfiguration configuration,
            IPasswordService passwordService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _passwordService = passwordService;
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string email, string password, string? ipAddress = null)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Authentication failed - User not found or inactive: {Email}", email);
                    return new AuthenticationResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid email or password"
                    };
                }

                // Check if account is locked
                if (await IsAccountLockedAsync(user.Id))
                {
                    _logger.LogWarning("Authentication failed - Account locked: {UserId}", user.Id);
                    return new AuthenticationResult
                    {
                        Success = false,
                        ErrorMessage = "Account is locked"
                    };
                }

                // Validate password
                var passwordValid = await _userManager.CheckPasswordAsync(user, password);
                if (!passwordValid)
                {
                    // Increment failed login attempts
                    await IncrementFailedLoginAttemptsAsync(user.Id, ipAddress);

                    _logger.LogWarning("Authentication failed - Invalid password: {UserId}", user.Id);
                    return new AuthenticationResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid email or password"
                    };
                }

                // Check if password is expired
                if (await _passwordService.IsPasswordExpiredAsync(user.Id))
                {
                    _logger.LogInformation("Authentication requires password change: {UserId}", user.Id);
                    return new AuthenticationResult
                    {
                        Success = false,
                        ErrorMessage = "Password has expired. Please change your password.",
                        User = user
                    };
                }

                // Check two-factor authentication
                if (await _userManager.GetTwoFactorEnabledAsync(user))
                {
                    var twoFactorToken = await GenerateTwoFactorTokenAsync(user.Id, "Email");

                    _logger.LogInformation("Two-factor authentication required: {UserId}", user.Id);
                    return new AuthenticationResult
                    {
                        Success = false,
                        RequiresTwoFactor = true,
                        AccessToken = twoFactorToken,
                        User = user,
                        ErrorMessage = "Two-factor authentication required"
                    };
                }

                // Generate tokens
                var accessToken = await GenerateAccessTokenAsync(user);
                var refreshToken = GenerateRefreshToken();

                // Save refresh token
                await SaveRefreshTokenAsync(user.Id, refreshToken, ipAddress);

                // Reset failed login attempts
                await ResetFailedLoginAttemptsAsync(user.Id);

                // Update last login
                user.LastLoginDate = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("User authenticated successfully: {UserId}", user.Id);

                return new AuthenticationResult
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for email: {Email}", email);
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred during authentication"
                };
            }
        }


        public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
        {
            try
            {
                var tokenRecord = await _context.TwoFactorTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == refreshToken &&
                                             !rt.IsUsed &&
                                             rt.ExpiryDate > DateTime.UtcNow);

                if (tokenRecord == null)
                {
                    _logger.LogWarning("Invalid or expired refresh token attempted from IP: {IpAddress}", ipAddress);
                    return new AuthenticationResult
                    {
                        Success = false,
                        ErrorMessage = "Invalid or expired refresh token"
                    };
                }

                var user = tokenRecord.User;
                if (!user.IsActive)
                {
                    _logger.LogWarning("Refresh token used for inactive user: {UserId}", user.Id);
                    return new AuthenticationResult
                    {
                        Success = false,
                        ErrorMessage = "User account is inactive"
                    };
                }

                // Revoke old refresh token
                tokenRecord.IsUsed = true;
                tokenRecord.UsedDate = DateTime.UtcNow;

                // Generate new tokens
                var accessToken = await GenerateAccessTokenAsync(user);
                var newRefreshToken = GenerateRefreshToken();

                // Save new refresh token
                await SaveRefreshTokenAsync(user.Id, newRefreshToken, ipAddress);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Tokens refreshed successfully for user: {UserId}", user.Id);

                return new AuthenticationResult
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                    User = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "An error occurred during token refresh"
                };
            }
        }

        public async Task<bool> LogoutAsync(Guid userId, string? sessionToken = null)
        {
            try
            {
                var query = _context.TwoFactorTokens.Where(rt => rt.UserId == userId);

                if (!string.IsNullOrEmpty(sessionToken))
                {
                    query = query.Where(rt => rt.Token == sessionToken);
                }

                var tokens = await query.ToListAsync();

                foreach (var token in tokens)
                {
                    token.IsUsed = true;
                    token.UsedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("User logged out successfully: {UserId}, Tokens revoked: {TokenCount}",
                    userId, tokens.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> LogoutAllSessionsAsync(Guid userId)
        {
            try
            {
                var tokens = await _context.TwoFactorTokens
                    .Where(rt => rt.UserId == userId && !rt.IsUsed)
                    .ToListAsync();

                foreach (var token in tokens)
                {
                    token.IsUsed = true;
                    token.UsedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("All sessions logged out for user: {UserId}, Tokens revoked: {TokenCount}",
                    userId, tokens.Count);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout all sessions for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<string> GenerateTwoFactorTokenAsync(Guid userId, string tokenType)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    throw new InvalidOperationException("User not found");
                }

                var token = await _userManager.GenerateTwoFactorTokenAsync(user, tokenType);

                _logger.LogInformation("Two-factor token generated for user: {UserId}, Type: {TokenType}",
                    userId, tokenType);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating two-factor token for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ValidateTwoFactorTokenAsync(Guid userId, string token, string tokenType)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return false;
                }

                var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, tokenType, token);

                _logger.LogInformation("Two-factor token validation result: {IsValid} for user: {UserId}",
                    isValid, userId);

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating two-factor token for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> LockUserAccountAsync(Guid userId, string reason, DateTime? lockoutEnd = null)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return false;
                }

                var lockoutEndDate = lockoutEnd ?? DateTime.UtcNow.AddHours(24); // Default 24 hour lockout

                var result = await _userManager.SetLockoutEndDateAsync(user, lockoutEndDate);

                if (result.Succeeded)
                {
                    // Log the lockout reason
                    //await LogSecurityEventAsync(userId, "AccountLocked", reason);

                    _logger.LogWarning("User account locked: {UserId}, Reason: {Reason}, Until: {LockoutEnd}",
                        userId, reason, lockoutEndDate);
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user account: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UnlockUserAccountAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return false;
                }

                var result = await _userManager.SetLockoutEndDateAsync(user, null);

                if (result.Succeeded)
                {
                    //await LogSecurityEventAsync(userId, "AccountUnlocked", "Manual unlock");

                    _logger.LogInformation("User account unlocked: {UserId}", userId);
                }

                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user account: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> IsAccountLockedAsync(Guid userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return false;
                }

                return await _userManager.IsLockedOutAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking account lock status: {UserId}", userId);
                return false;
            }
        }

        // Private helper methods
        private async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new("jti", Guid.NewGuid().ToString())
            };

            // Add user roles as claims
            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetTokenExpirationMinutes()),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private async Task SaveRefreshTokenAsync(Guid userId, string refreshToken, string? ipAddress)
        {
            var tokenRecord = new TwoFactorToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays()),
                CreatedDate = DateTime.UtcNow,
                IpAddress = ipAddress
            };

            _context.TwoFactorTokens.Add(tokenRecord);
            await _context.SaveChangesAsync();
        }

        private async Task IncrementFailedLoginAttemptsAsync(Guid userId, string? ipAddress)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                await _userManager.AccessFailedAsync(user);
                //await LogSecurityEventAsync(userId, "FailedLogin", $"Failed login attempt from IP: {ipAddress}");
            }
        }

        private async Task ResetFailedLoginAttemptsAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
            }
        }

        private async Task LogSecurityEventAsync(Guid userId, string eventType, string details)
        {
            var securityEvent = new AccountLockout
            {
                UserId = userId,
                LockoutReason = details,
                IpAddress = GetCurrentIpAddress()
            };

            _context.AccountLockouts.Add(securityEvent);
            await _context.SaveChangesAsync();
        }

        private int GetTokenExpirationMinutes()
        {
            return _configuration.GetValue<int>("Jwt:ExpirationMinutes", 60);
        }

        private int GetRefreshTokenExpirationDays()
        {
            return _configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
        }

        private string? GetCurrentIpAddress()
        {
            // This would typically be injected through HttpContext
            // For now, returning null - implement based on your architecture
            return null;
        }
    }
}
