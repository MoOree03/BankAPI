using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BankAPI.Models.Dto.UserDto
{
    public class UserRegisterDto
    {
        [Required, MinLength(8)]
        public string Email { get; set; }
        [MinLength(8)]
        [Required]
        public string Password { get; set; }
        [Required]
        public char AccountType { get; set; }
    }
}
