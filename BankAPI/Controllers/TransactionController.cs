using AutoMapper;
using BankAPI.Models;
using BankAPI.Models.Dto.TransacionDto;
using BankAPI.Models.Dto.UserDto;
using BankAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace BankAPI.Controllers
{
    [Route("api/transaction")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRep;
        private readonly IMapper _mapper;
        private ApiAnswer _apiAnswer;
        public TransactionController(ITransactionRepository transactionRepository, IMapper mapper)
        {
            _mapper = mapper;
            _transactionRep = transactionRepository;
            _apiAnswer = new ApiAnswer();
        }


        [HttpGet("{accountNumber}", Name = "GetTransactionByAccount")]
        [ResponseCache(CacheProfileName = "DefaultCache")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetTransactionByAccount(string accountNumber)
        {
            var listTransaction = _transactionRep.GetTransactions(accountNumber);
            if (listTransaction.Count == 0)
            {
                return BadRequest("There are not transactions");
            }
            var listTransactionDto = _mapper.Map<List<TransactionDto>>(listTransaction);
            return Ok(listTransactionDto);
        }


        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto encryptedDataWrapper)
        {
            if (encryptedDataWrapper == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Decrypt the DTO values
                DecryptDtoValues(encryptedDataWrapper);

                bool isTransactionCreated = await _transactionRep.CreateTransaction(encryptedDataWrapper);
                if (!isTransactionCreated)
                {
                    ModelState.AddModelError("", $"Something failed saving the transaction -> {encryptedDataWrapper.TransactionType}");
                    return StatusCode(500, ModelState);
                }

                return Ok(new
                {
                    Message = "Transaction created successfully",
                });
            }
            catch (Exception ex)
            {
                // Logging the exception would be a good idea here for further debugging
                return BadRequest(ex.Message);
            }
        }

        private void DecryptDtoValues(CreateTransactionDto dto)
        {
            dto.OriginAccountNumber = CleanValue(_transactionRep.Decrypt(dto.OriginAccountNumber));
            dto.TransactionType = CleanValue(_transactionRep.Decrypt(dto.TransactionType));
            dto.DestinationAccountNumber = CleanValue(_transactionRep.Decrypt(dto.DestinationAccountNumber));
            dto.Value = CleanValue(_transactionRep.Decrypt(dto.Value));
        }
        private string CleanValue(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Remover las comillas adicionales
            string cleaned = input.Trim('"');

            // Remover los caracteres de nueva línea o retorno de carro
            cleaned = cleaned.Replace("\n", "").Replace("\r", "");

            return cleaned;
        }

    }
}
