using SchoolManager.DTOs;
using SchoolManager.Models;

namespace SchoolManager.Services.Implementations
{
    public class UserMapper : IUserMapper
    {
        public UserDto MapToUserDto(ApplicationUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

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
                Roles = new List<string>(),         // Can be filled in from the service later
                Permissions = new List<string>()    // Same here
            };
        }
    }

}
