using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BankAPI.Models.Dto.TransacionDto
{
    public class TransactionDto
    {
        public string OriginAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public char TransactionType { get; set; }
        public decimal Value { get; set; }
    }
}
