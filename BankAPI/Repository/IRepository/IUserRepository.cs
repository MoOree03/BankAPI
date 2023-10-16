using BankAPI.Models;
using BankAPI.Models.Dto.UserDto;
using BankAPI.Models;
using System.Security.Claims;

namespace BankAPI.Repository.IRepository
{
    public interface IUserRepository
    {
        ICollection<User> GetUsers();
        User GetUser(int id);
        User GetUser(string email);
        bool existUser(string email);
        Task<UserLoginResponseDto> Login(UserLoginDto userloginDto);
        Task<User> Register(UserRegisterDto userRegisterDto);
        bool DeleteUser (string email);       
        string CreateToken(string email, string codeRole);
        string CreateRefreshToken(string email, string accountNumber, TimeSpan timeExpiration);
        ClaimsPrincipal validateCookie(string? m3JCookie);
    }
}
