using BankAPI.Models;
using BankAPI.Models.Dto.UserDto;
using BankAPI.Models;
using System.Security.Claims;
using BankAPI.Models.Dto.TransacionDto;

namespace BankAPI.Repository.IRepository
{
    public interface ITransactionRepository
    {
        Transaction GetTransaction(int id);
        ICollection<Transaction> GetTransactions(int accountID);
        bool CreateTransaction(CreateTransactionDto newTransaction);
        bool save();
    }
}
