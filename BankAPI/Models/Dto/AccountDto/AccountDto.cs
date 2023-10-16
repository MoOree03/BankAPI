using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BankAPI.Models.Dto.AccountDto
{
    public class AccountDto
    {
        public string Email { get; set; }
        public string AccountNumber { get; set; }
        public char AccountType { get; set; } // 'S' for Savings or 'C' for Current
        public int Balance { get; set; }      
        public virtual ICollection<Transaction> Transactions { get; set; }

    }
}
