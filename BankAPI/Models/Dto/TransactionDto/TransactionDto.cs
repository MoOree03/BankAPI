using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BankAPI.Models.Dto.TransacionDto
{
    public class TransactionDto
    {

        public int TransactionId { get; set; }
        public int OriginAccountId { get; set; }
        public int DestinationAccountId { get; set; }
        public char TransactionType { get; set; }
        public decimal Value { get; set; }
    }
}
