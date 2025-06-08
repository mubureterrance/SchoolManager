using SchoolManager.Enums;

namespace SchoolManager.DTOs
{
    public class CreateUserRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; } = "Gaborone";
        public string? State { get; set; }
        public string? Country { get; set; } = "Botswana";
        public List<string> Roles { get; set; } = new();
        public string? Password { get; set; }
        public bool SendWelcomeEmail { get; set; } = true;
        public string FullName => $"{FirstName} {MiddleName} {LastName}".Replace("  ", " ").Trim();
    }
}
