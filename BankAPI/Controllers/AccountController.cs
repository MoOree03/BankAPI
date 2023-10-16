using AutoMapper;
using BankAPI.Models;
using BankAPI.Models.Dto.AccountDto;
using BankAPI.Models.Dto.TransacionDto;
using BankAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;

namespace BankAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRep;
        private readonly IMapper _mapper;
        private ApiAnswer _apiAnswer;
        public AccountController(IAccountRepository accountRepository, IMapper mapper)
        {
            _mapper = mapper;
            _accountRep = accountRepository;
            _apiAnswer = new ApiAnswer();
        }


        [HttpGet("{email}", Name = "GetAccountByEmail")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAccountByEmail(string email)
        {
            var account = _accountRep.GetAccount(email);
            var accountResult = _mapper.Map<AccountDto>(account);
            return Ok(accountResult);
        }

    }
}
