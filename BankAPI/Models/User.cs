using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankAPI.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }  
        public string Email { get; set; }
        [MinLength(8)]
        [Required]
        public string Password { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
    }
}
