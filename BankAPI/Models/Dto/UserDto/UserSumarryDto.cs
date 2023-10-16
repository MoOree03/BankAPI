using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BankAPI.Models.Dto.UserDto
{
    public class UserSumarryDto
    {
        public string Email { get; set; }
        public string Password { get; set; }


    }
}
