using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankAPI.Models
{
    public class Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransactionId { get; set; }

        public int OriginAccountId { get; set; }
        [ForeignKey("OriginAccountId")]
        public virtual Account OriginAccount { get; set; }

        public int DestinationAccountId { get; set; }
        [ForeignKey("DestinationAccountId")]
        public virtual Account DestinationAccount { get; set; }
            
        [Required]
        public char TransactionType { get; set; } // 'C' for Consult, 'R' for Withdraw, 'D' for Deposit, 'A' for Account Open, 'T' for Transfer

        [Required]
        public decimal Value { get; set; }
    }
}