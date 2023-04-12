using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Domain.Entities;
using UserManagement.Domain.RequestDto;

namespace UserManagement.Domain.Repository
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<IEnumerable<IdentityError>> Register(RegisterDto userDto);
        Task<AuthResponseDto> Login(LoginDto loginDto);
    }
}
