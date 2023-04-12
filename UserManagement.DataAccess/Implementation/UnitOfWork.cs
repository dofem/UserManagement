using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using UserMan.DataAccess.Implementation;
using UserManagement.DataAccess.Data;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Repository;

namespace UserManagement.DataAccess.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private IUserRepository _userRepository;

        public UnitOfWork(ApplicationDbContext context, UserManager<User> userManager, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
            _configuration = configuration;
        }

        public IUserRepository User
        {
            get
            {
                if (_userRepository == null)
                {
                    _userRepository = new UserRepository(_context, _mapper,_userManager, _configuration);
                }

                return _userRepository;
            }
        }

        public int Save()
        {
            return _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }


}
