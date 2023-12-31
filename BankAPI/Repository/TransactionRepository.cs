﻿using BankAPI.Data;
using BankAPI.Models;
using BankAPI.Models.Dto.TransacionDto;
using BankAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BankAPI.Repository
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public TransactionRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> CreateTransaction(CreateTransactionDto createTransactionDto)
        {

                int accountDestinationId = (await _context.Account.FirstOrDefaultAsync(e => e.AccountNumber == createTransactionDto.DestinationAccountNumber)).AccountId;
            int accountOriginId = (createTransactionDto.OriginAccountNumber == "-1") ? accountDestinationId : (await _context.Account.FirstOrDefaultAsync(e => e.AccountNumber == createTransactionDto.OriginAccountNumber)).AccountId;

            switch (createTransactionDto.TransactionType[0])
            {

                case 'R': // Withdraw
                    return CreateWithdrawTransaction(accountOriginId, Decimal.Parse(createTransactionDto.Value));

                case 'D': // Deposit
                    return CreateDepositTransaction(accountOriginId, Decimal.Parse(createTransactionDto.Value));

                case 'T': // Transfer
                    return CreateTransferTransaction(accountOriginId, accountDestinationId, Decimal.Parse(createTransactionDto.Value));

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

        public ICollection<TransactionDto> GetTransactions(string accountNumber)
        {
            int accountId = _context.Account.FirstOrDefault(e => e.AccountNumber == accountNumber).AccountId;
            if (accountId == default(int)) { return null; } // default(int) is 0 for int.

            var transactions = _context.Transaction
                .Include(t => t.OriginAccount)
                .Include(t => t.DestinationAccount)
                .Where(e => e.OriginAccountId == accountId || e.DestinationAccountId == accountId)
                .Select(t => new TransactionDto
                {
                    OriginAccountNumber = t.OriginAccount.AccountNumber,
                    DestinationAccountNumber = t.DestinationAccount.AccountNumber,
                    TransactionType = t.TransactionType,
                    Value = t.Value
                })
                .ToList();
            CreateConsultTransaction(accountId);
            return transactions;
        }



        public bool save()
        {
            return _context.SaveChanges() >= 0 ? true : false;
        }

        public bool CreateConsultTransaction(int accountId)
        {
           
            // Create a new transaction for the consult
            var transaction = new Transaction
            {
                OriginAccountId = accountId,
                DestinationAccountId = accountId,
                TransactionType = 'C', // 'C' for Consult
                Value = 0
            };

            _context.Transaction.Add(transaction);

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
                DestinationAccountId = accountId,
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
                OriginAccountId = accountId,
                TransactionType = 'D', // 'D' for Deposit
                Value = value
            };

            _context.Transaction.Add(transaction);
            account.Balance += value; // Adjust the balance

            return save(); // Save the transaction and update the account balance
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

        public string Decrypt(string encryptedText)
        {
            byte[] initVectorBytes = Encoding.UTF8.GetBytes("pemgail9uzpgzl88"); 
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF");

            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
            {
                aes.KeySize = 256;
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.IV = initVectorBytes;
                aes.Key = keyBytes;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                    memoryStream.Close();
                    cryptoStream.Close();

                    return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                }
            }
        }
    }
}
