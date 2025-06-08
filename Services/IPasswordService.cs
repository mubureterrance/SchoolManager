namespace SchoolManager.Services
{
    public interface IPasswordService
    {
        Task<bool> ValidatePasswordPolicyAsync(string password);
        Task<bool> IsPasswordReusedAsync(Guid userId, string password);
        Task AddPasswordToHistoryAsync(Guid userId, string passwordHash);
        Task<bool> IsPasswordExpiredAsync(Guid userId);
        Task<string> GenerateRandomPasswordAsync(int length = 12);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
