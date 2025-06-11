using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolManager.Data;
using SchoolManager.Models;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SchoolManager.Services.Implementations
{
    public class PasswordService : IPasswordService
    {
        private readonly SchoolManagementDbContext _context;
        private readonly ILogger<PasswordService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        // Password policy configuration keys
        private const string MinLengthKey = "PasswordPolicy:MinLength";
        private const string RequireUppercaseKey = "PasswordPolicy:RequireUppercase";
        private const string RequireLowercaseKey = "PasswordPolicy:RequireLowercase";
        private const string RequireDigitKey = "PasswordPolicy:RequireDigit";
        private const string RequireSpecialCharKey = "PasswordPolicy:RequireSpecialCharacter";
        private const string MaxHistoryKey = "PasswordPolicy:MaxHistory";
        private const string ExpirationDaysKey = "PasswordPolicy:ExpirationDays";

        public PasswordService(
            SchoolManagementDbContext context,
            ILogger<PasswordService> logger,
            IConfiguration configuration,
            IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }

        public Task<bool> ValidatePasswordPolicyAsync(string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("Password validation failed - Empty password");
                    return Task.FromResult(false);
                }

                var minLength = _configuration.GetValue<int>(MinLengthKey, 12);
                var requireUppercase = _configuration.GetValue<bool>(RequireUppercaseKey, true);
                var requireLowercase = _configuration.GetValue<bool>(RequireLowercaseKey, true);
                var requireDigit = _configuration.GetValue<bool>(RequireDigitKey, true);
                var requireSpecialChar = _configuration.GetValue<bool>(RequireSpecialCharKey, true);

                var validationErrors = new List<string>();

                // Check minimum length
                if (password.Length < minLength)
                {
                    validationErrors.Add($"Password must be at least {minLength} characters long");
                }

                // Check uppercase requirement
                if (requireUppercase && !password.Any(char.IsUpper))
                {
                    validationErrors.Add("Password must contain at least one uppercase letter");
                }

                // Check lowercase requirement
                if (requireLowercase && !password.Any(char.IsLower))
                {
                    validationErrors.Add("Password must contain at least one lowercase letter");
                }

                // Check digit requirement
                if (requireDigit && !password.Any(char.IsDigit))
                {
                    validationErrors.Add("Password must contain at least one digit");
                }

                // Check special character requirement
                if (requireSpecialChar && !HasSpecialCharacter(password))
                {
                    validationErrors.Add("Password must contain at least one special character");
                }

                // Check for common weak passwords
                if (IsCommonPassword(password))
                {
                    validationErrors.Add("Password is too common and easily guessable");
                }

                // Check for sequential or repeated characters
                if (HasSequentialOrRepeatedCharacters(password))
                {
                    validationErrors.Add("Password cannot contain sequential or repeated characters");
                }

                if (validationErrors.Any())
                {
                    _logger.LogInformation("Password validation failed: {Errors}", string.Join(", ", validationErrors));
                    return Task.FromResult(false);
                }

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating password policy");
                return Task.FromResult(false);
            }
        }

        public async Task<bool> IsPasswordReusedAsync(Guid userId, string password)
        {
            try
            {
                var maxHistory = _configuration.GetValue<int>(MaxHistoryKey, 12);

                var passwordHistory = await _context.PasswordHistory
                    .Where(ph => ph.UserId == userId)
                    .OrderByDescending(ph => ph.CreatedDate)
                    .Take(maxHistory)
                    .Select(ph => ph.PasswordHash)
                    .ToListAsync();

                var hashedPassword = HashPassword(password);

                foreach (var historicalHash in passwordHistory)
                {
                    if (VerifyPassword(password, historicalHash))
                    {
                        _logger.LogInformation("Password reuse detected for user: {UserId}", userId);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password reuse for user: {UserId}", userId);
                return false;
            }
        }

        public async Task AddPasswordToHistoryAsync(Guid userId, string passwordHash)
        {
            try
            {
                var maxHistory = _configuration.GetValue<int>(MaxHistoryKey, 12);

                // Add new password to history
                var passwordHistory = new PasswordHistory
                {
                    UserId = userId,
                    PasswordHash = passwordHash,
                    CreatedDate = DateTime.UtcNow
                };

                _context.PasswordHistory.Add(passwordHistory);

                // Remove old password history entries if we exceed the limit
                var oldEntries = await _context.PasswordHistory
                    .Where(ph => ph.UserId == userId)
                    .OrderByDescending(ph => ph.CreatedDate)
                    .Skip(maxHistory)
                    .ToListAsync();

                if (oldEntries.Any())
                {
                    _context.PasswordHistory.RemoveRange(oldEntries);
                    _logger.LogInformation("Removed {Count} old password history entries for user: {UserId}",
                        oldEntries.Count, userId);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Added password to history for user: {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding password to history for user: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> IsPasswordExpiredAsync(Guid userId)
        {
            try
            {
                var expirationDays = _configuration.GetValue<int>(ExpirationDaysKey, 90);

                if (expirationDays <= 0)
                {
                    return false; // Password expiration is disabled
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user?.LastPasswordChangeDate == null)
                {
                    // If no password change date is recorded, consider it expired
                    return true;
                }

                var expirationDate = user.LastPasswordChangeDate.Value.AddDays(expirationDays);
                var isExpired = DateTime.UtcNow > expirationDate;

                if (isExpired)
                {
                    _logger.LogInformation("Password expired for user: {UserId}, Last changed: {LastChanged}",
                        userId, user.LastPasswordChangeDate);
                }

                return isExpired;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password expiration for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<string> GenerateRandomPasswordAsync(int length = 12)
        {
            try
            {
                if (length < 8)
                {
                    length = 12; // Ensure minimum secure length
                }

                const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
                const string digitChars = "0123456789";
                const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

                var password = new StringBuilder();
                using var rng = RandomNumberGenerator.Create();

                // Ensure at least one character from each required category
                password.Append(GetRandomCharacter(uppercaseChars, rng));
                password.Append(GetRandomCharacter(lowercaseChars, rng));
                password.Append(GetRandomCharacter(digitChars, rng));
                password.Append(GetRandomCharacter(specialChars, rng));

                // Fill the rest with random characters from all categories
                var allChars = uppercaseChars + lowercaseChars + digitChars + specialChars;
                for (int i = 4; i < length; i++)
                {
                    password.Append(GetRandomCharacter(allChars, rng));
                }

                // Shuffle the password to avoid predictable patterns
                var passwordArray = password.ToString().ToCharArray();
                for (int i = passwordArray.Length - 1; i > 0; i--)
                {
                    var j = GetRandomInt(0, i + 1, rng);
                    (passwordArray[i], passwordArray[j]) = (passwordArray[j], passwordArray[i]);
                }

                var generatedPassword = new string(passwordArray);

                // Validate the generated password meets policy
                if (!await ValidatePasswordPolicyAsync(generatedPassword))
                {
                    _logger.LogWarning("Generated password failed policy validation, regenerating");
                    return await GenerateRandomPasswordAsync(length);
                }

                _logger.LogInformation("Random password generated with length: {Length}", length);
                return generatedPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating random password");
                throw;
            }
        }

        public string HashPassword(string password)
        {
            try
            {
                // Using a dummy user for hashing - the user object isn't used in the hashing process
                var dummyUser = new ApplicationUser();
                return _passwordHasher.HashPassword(dummyUser, password);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hashing password");
                throw;
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                var dummyUser = new ApplicationUser();
                var result = _passwordHasher.VerifyHashedPassword(dummyUser, hash, password);
                return result == PasswordVerificationResult.Success ||
                       result == PasswordVerificationResult.SuccessRehashNeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying password");
                return false;
            }
        }

        // Private helper methods
        private bool HasSpecialCharacter(string password)
        {
            var specialCharPattern = @"[!@#$%^&*()_+\-=\[\]{}|;:,.<>?]";
            return Regex.IsMatch(password, specialCharPattern);
        }

        private bool IsCommonPassword(string password)
        {
            // List of common passwords to reject
            var commonPasswords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "password", "123456", "password123", "admin", "qwerty", "letmein",
                "welcome", "monkey", "1234567890", "abc123", "Password1", "12345678",
                "password1", "123456789", "welcome123", "Password123"
            };

            return commonPasswords.Contains(password);
        }

        private bool HasSequentialOrRepeatedCharacters(string password)
        {
            // Check for repeated characters (more than 2 consecutive)
            for (int i = 0; i < password.Length - 2; i++)
            {
                if (password[i] == password[i + 1] && password[i + 1] == password[i + 2])
                {
                    return true;
                }
            }

            // Check for sequential characters (like 123 or abc)
            for (int i = 0; i < password.Length - 2; i++)
            {
                var char1 = password[i];
                var char2 = password[i + 1];
                var char3 = password[i + 2];

                if ((char2 == char1 + 1 && char3 == char2 + 1) ||
                    (char2 == char1 - 1 && char3 == char2 - 1))
                {
                    return true;
                }
            }

            return false;
        }

        private char GetRandomCharacter(string characters, RandomNumberGenerator rng)
        {
            var randomIndex = GetRandomInt(0, characters.Length, rng);
            return characters[randomIndex];
        }

        private int GetRandomInt(int minValue, int maxValue, RandomNumberGenerator rng)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToUInt32(bytes, 0);
            return (int)(value % (uint)(maxValue - minValue)) + minValue;
        }
    }

}

