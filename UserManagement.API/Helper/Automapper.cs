using AutoMapper;
using UserManagement.Domain.Entities;
using UserManagement.Domain.RequestDto;

namespace UserMan.API.Helper
{
    public class Automapper : Profile
    {
        public Automapper()
        {
            CreateMap<User,UserDto>().ReverseMap();
        }
    }
}
