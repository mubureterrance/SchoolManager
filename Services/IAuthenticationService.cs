using SchoolManager.DTOs;

namespace SchoolManager.Services
{
    public interface IAuthenticationService
    {
        Task<AuthenticationResult> AuthenticateAsync(string email, string password, string? ipAddress = null);
        Task<AuthenticationResult> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
        Task<bool> LogoutAsync(Guid userId, string? sessionToken = null);
        Task<bool> LogoutAllSessionsAsync(Guid userId);
        Task<string> GenerateTwoFactorTokenAsync(Guid userId, string tokenType);
        Task<bool> ValidateTwoFactorTokenAsync(Guid userId, string token, string tokenType);
        Task<bool> LockUserAccountAsync(Guid userId, string reason, DateTime? lockoutEnd = null);
        Task<bool> UnlockUserAccountAsync(Guid userId);
        Task<bool> IsAccountLockedAsync(Guid userId);
    }
}
