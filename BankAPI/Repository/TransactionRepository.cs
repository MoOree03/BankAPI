using BankAPI.Data;
using BankAPI.Models;
using BankAPI.Models.Dto.TransacionDto;
using BankAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace BankAPI.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        public TransactionRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
        }

        public bool CreateTransaction(CreateTransactionDto createTransactionDto)
        {
            switch (createTransactionDto.TransactionType)
            {
                case 'C': // Consult
                    return CreateConsultTransaction(createTransactionDto.OriginAccountId, createTransactionDto.Value);

                case 'R': // Withdraw
                    return CreateWithdrawTransaction(createTransactionDto.OriginAccountId, createTransactionDto.Value);

                case 'D': // Deposit
                    return CreateDepositTransaction(createTransactionDto.OriginAccountId, createTransactionDto.Value);

                case 'A': // Account Open
                    return CreateAccountOpenTransaction(createTransactionDto.OriginAccountId, createTransactionDto.Value);

                case 'T': // Transfer
                    return CreateTransferTransaction(createTransactionDto.OriginAccountId, createTransactionDto.DestinationAccountId, createTransactionDto.Value);

                default:
                    // Handle unsupported transaction type
                    return false;
            }
        }




        public Transaction GetTransaction(int id)
        {
            return _context.Transaction
                .FirstOrDefault(u => u.TransactionId == id);
        }

        public ICollection<Transaction> GetTransactions(int accountId)
        {
            return _context.Transaction
                .Where(e => e.OriginAccountId == accountId)
                .ToList();
        }

        public bool save()
        {
            return _context.SaveChanges() >= 0 ? true : false;
        }

        public bool CreateConsultTransaction(int accountId, decimal value)
        {
            // Check if the account exists and has sufficient balance for the consult
            var account = _context.Account.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null || account.Balance < value)
            {
                return false; // Transaction failed
            }

            // Create a new transaction for the consult
            var transaction = new Transaction
            {
                OriginAccountId = accountId,
                TransactionType = 'C', // 'C' for Consult
                Value = value
            };

            _context.Transaction.Add(transaction);
            account.Balance -= value; // Adjust the balance

            return save(); // Save the transaction and update the account balance
        }
        public bool CreateWithdrawTransaction(int accountId, decimal value)
        {
            // Check if the account exists and has sufficient balance for the withdrawal
            var account = _context.Account.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null || account.Balance < value)
            {
                return false; // Transaction failed
            }

            // Create a new transaction for the withdrawal
            var transaction = new Transaction
            {
                OriginAccountId = accountId,
                TransactionType = 'R', // 'R' for Withdraw
                Value = value
            };

            _context.Transaction.Add(transaction);
            account.Balance -= value; // Adjust the balance

            return save(); // Save the transaction and update the account balance
        }
        public bool CreateDepositTransaction(int accountId, decimal value)
        {
            // Check if the account exists
            var account = _context.Account.FirstOrDefault(a => a.AccountId == accountId);
            if (account == null)
            {
                return false; // Transaction failed
            }

            // Create a new transaction for the deposit
            var transaction = new Transaction
            {
                DestinationAccountId = accountId,
                TransactionType = 'D', // 'D' for Deposit
                Value = value
            };

            _context.Transaction.Add(transaction);
            account.Balance += value; // Adjust the balance

            return save(); // Save the transaction and update the account balance
        }
        public bool CreateAccountOpenTransaction(int accountId, decimal initialBalance)
        {
            // Check if the account already exists
            var account = _context.Account.FirstOrDefault(a => a.AccountId == accountId);
            if (account != null)
            {
                return false; // Transaction failed
            }

            // Create a new account and a transaction for the account open
            var newAccount = new Account
            {
                AccountId = accountId,
                Balance = initialBalance
            };

            var transaction = new Transaction
            {
                OriginAccountId = accountId,
                TransactionType = 'A', // 'A' for Account Open
                Value = initialBalance
            };

            _context.Account.Add(newAccount);
            _context.Transaction.Add(transaction);

            return save(); // Save the account and transaction
        }
        public bool CreateTransferTransaction(int originAccountId, int destinationAccountId, decimal value)
        {
            // Check if both accounts exist and the origin account has sufficient balance for the transfer
            var originAccount = _context.Account.FirstOrDefault(a => a.AccountId == originAccountId);
            var destinationAccount = _context.Account.FirstOrDefault(a => a.AccountId == destinationAccountId);

            if (originAccount == null || destinationAccount == null || originAccount.Balance < value)
            {
                return false; // Transaction failed
            }

            // Create a new transaction for the transfer
            var transaction = new Transaction
            {
                OriginAccountId = originAccountId,
                DestinationAccountId = destinationAccountId,
                TransactionType = 'T', // 'T' for Transfer
                Value = value
            };

            _context.Transaction.Add(transaction);
            originAccount.Balance -= value; // Adjust the balance for the origin account
            destinationAccount.Balance += value; // Adjust the balance for the destination account

            return save(); // Save the transaction and update the account balances
        }

      
    }
}
