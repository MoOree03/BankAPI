using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BankAPI.Models.Dto.TransacionDto
{
    public class CreateTransactionDto
    {
        public string OriginAccountNumber { get; set; }
        public string DestinationAccountNumber { get; set; }
        public string TransactionType { get; set; }
        public string Value { get; set; }

    }
}
