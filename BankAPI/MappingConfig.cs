using AutoMapper;
using BankAPI.Data;
using BankAPI.Models;
using BankAPI.Models.Dto.AccountDto;
using BankAPI.Models.Dto.TransacionDto;
using BankAPI.Models.Dto.UserDto;
using System.Transactions;

namespace BankAPI
{
    public class MappingConfig : Profile
    {
        private readonly ApplicationDbContext _context;
        public MappingConfig()
        {

            CreateMap<User, UserRegisterDto>().ReverseMap();
            CreateMap<User, UserSumarryDto>().ReverseMap();
            CreateMap<BankAPI.Models.Transaction, TransactionDto>().ReverseMap();
            // Create a mapping between Account and AccountDto
            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email)).ReverseMap(); // Map the Email property

            
        }

    }
}
