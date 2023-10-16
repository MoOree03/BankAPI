using BankAPI.Models;
using BankAPI.Models.Dto.UserDto;
using BankAPI.Models;
using System.Security.Claims;

namespace BankAPI.Repository.IRepository
{
    public interface IAccountRepository
    {
        Account GetAccount(string email);       
    }
}
