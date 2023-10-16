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
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _key;
        public UserRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _key = configuration.GetValue<string>("ApiSettings:SecretKey");
        }

        //public bool DeleteUser(string email)
        //{
        //    _context.User.Remove(_context.User.FirstOrDefault(u => u.Email == email));
        //    return _context.SaveChanges() >= 0 ? true : false;

        //}

        public User GetUser(int id)
        {
            return _context.User.
                 Include(u => u.Accounts)
                .FirstOrDefault(u => u.UserId == id);
        }
        public User GetUser(string email)
        {
            return _context.User.
                 Include(u => u.Accounts).
                FirstOrDefault(u => u.Email.Equals(email));
        }



        public bool existUser(string email)
        {
            var user = _context.User.FirstOrDefault(x => x.Email.Equals(email.Trim()));
            if (user == null) return false;
            return true;
        }


        public async Task<UserLoginResponseDto> Login(UserLoginDto userloginDto)
        {
            var user = _context.User.
                Include(u => u.Accounts).
                FirstOrDefault(
                u => u.Email.Equals(userloginDto.Email.Trim())
                );
            if (user == null || !VerifyPassword(userloginDto.Password, user.Password))
            {
                return new UserLoginResponseDto() { Token = "", User = null };
            }

            UserLoginResponseDto userLoginResponseDto = new UserLoginResponseDto()
            {
                User = user,
                Token = CreateToken(user.Email, user.Accounts.FirstOrDefault().AccountNumber.ToString()),
            };
            return userLoginResponseDto;
        }


        public string CreateToken(string email, string accountNumber)
        {
            var tokenManager = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, email.ToString()),
                        new Claim(ClaimTypes.Role, accountNumber.ToString())
                    }
                ),
                Expires = DateTime.UtcNow.AddMinutes(1),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenManager.CreateToken(tokenDescriptor);
            return tokenManager.WriteToken(token);
        }

        public string CreateRefreshToken(string email, string accountNumber, TimeSpan timeExpiration)
        {
            var tokenManager = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.Name, email.ToString()),
                        new Claim(ClaimTypes.Role, accountNumber.ToString())
                    }
                ),
                Expires = DateTime.UtcNow.Add(timeExpiration),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenManager.CreateToken(tokenDescriptor);
            return tokenManager.WriteToken(token);
        }

        public async Task<User> Register(UserRegisterDto userRegisterDto)
        {
            string hashedPassword = HashPassword(userRegisterDto.Password);
            _context.SaveChanges();

            var newUser = new User()
            {
                Email = userRegisterDto.Email,
                Password = hashedPassword,
                Accounts = new List<Account>
                    {
                        new Account
                        {
                            AccountNumber = GenerateAccountNumber(), // Genera un número de cuenta (debes implementar esta función)
                            AccountType = userRegisterDto.AccountType,
                            Balance = 0
                        }
                    }
            };
            _context.User.Add(newUser);
            await _context.SaveChangesAsync();

            return _context.User
                .FirstOrDefault(u => u.Email.Equals(userRegisterDto.Email));
        }


        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }


        public ClaimsPrincipal validateCookie(string? m3JCookie)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);

            try
            {
                SecurityToken validatedToken;
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false,
                };
                var principal = tokenHandler.ValidateToken(m3JCookie, tokenValidationParameters, out validatedToken);
                return principal;
            }
            catch (SecurityTokenException ex)
            {
                return null;
            }
        }
        private string GenerateAccountNumber()
        {
            Random random = new Random();
            string accountNumber = string.Empty;
            for (int i = 0; i < 20; i++) accountNumber += random.Next(0, 10).ToString();
            return accountNumber;
        }

        public ICollection<User> GetUsers()
        {
            return _context.User.ToList();
        }

        public bool DeleteUser(string email)
        {
            throw new NotImplementedException();
        }


        public string CreateRefreshToken(string email, TimeSpan timeExpiration)
        {
            var tokenManager = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_key);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.Name, email.ToString()),

                    }
                ),
                NotBefore = DateTime.UtcNow,  // Establecer explícitamente el NotBefore
                Expires = DateTime.UtcNow.Add(timeExpiration),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenManager.CreateToken(tokenDescriptor);
            return tokenManager.WriteToken(token);
        }


    }
}
