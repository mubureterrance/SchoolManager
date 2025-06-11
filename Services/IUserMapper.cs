using SchoolManager.DTOs;
using SchoolManager.Models;

namespace SchoolManager.Services
{
    public interface IUserMapper
    {
        UserDto MapToUserDto(ApplicationUser user);
    }
}
