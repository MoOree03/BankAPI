using BankAPI.Data;
using BankAPI.Models;
using BankAPI.Models.Dto.UserDto;
using BankAPI.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BankAPI.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly ApplicationDbContext _context;
        public AccountRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
        }

        public Account GetAccount(string email)
        {
           return _context.Account
                 .Include(a => a.Transactions)
                 .Include(a => a.User)
                 .FirstOrDefault(a => a.User.Email == email);
        }
    }
}
