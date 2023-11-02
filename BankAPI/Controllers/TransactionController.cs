﻿using AutoMapper;
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
        public async Task<IActionResult> CreateTransaction([FromBody] JObject encryptedDataWrapper)
        {
            string encryptedData = encryptedDataWrapper["data"].ToString();
            try
            {
                var decryptedData = _transactionRep.Decrypt(encryptedData);


                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringToDecimalConverter());

                var newTransaction = JsonConvert.DeserializeObject<CreateTransactionDto>(decryptedData, settings);



                if (newTransaction == null || !ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (!await _transactionRep.CreateTransaction(newTransaction))
                {
                    ModelState.AddModelError("", $"Something failed saving the transaction -> {newTransaction.TransactionType}");
                    return StatusCode(500, ModelState);
                }

                return Ok(new
                {
                    Message = "Transaction created successfully",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
          
        }

        public class StringToDecimalConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(decimal);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return Decimal.Parse(reader.Value.ToString());
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }
        }

    }
}
