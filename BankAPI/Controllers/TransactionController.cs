﻿using AutoMapper;
using BankAPI.Models;
using BankAPI.Models.Dto.TransacionDto;
using BankAPI.Models.Dto.UserDto;
using BankAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
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


        [HttpGet("{accountID:int}", Name = "GetTransactionByAccount")]
        [ResponseCache(CacheProfileName = "DefaultCache")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult GetTransactionByAccount(int accountID)
        {
            var listTransaction = _transactionRep.GetTransactions(accountID);
            var listTransactionDto = new List<TransactionDto>();
            if (listTransaction == null)
            {
                return BadRequest("There are not transactions");
            }
            foreach (var transaction in listTransaction)
            {
                listTransactionDto.Add(_mapper.Map<TransactionDto>(transaction));
            }
            return Ok(listTransactionDto);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(TransactionDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateTransaction([FromBody] CreateTransactionDto newTransaction)
        {
            if (!ModelState.IsValid || newTransaction == null)
            {
                return BadRequest(ModelState);
            }

            if (!_transactionRep.CreateTransaction(newTransaction))
            {
                ModelState.AddModelError("", $"Something failed saving the transaction -> {newTransaction.TransactionType}");
                return StatusCode(500, ModelState);
            }
            return Ok(new
            {
                Message = "Transaction created successfully",

            });
        }

    }
}
