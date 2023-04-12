using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using UserMan.API.Dto;
using UserMan.DataAccess.Implementation;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Repository;

namespace UserMan.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        private readonly IDistributedCache _distributedCache;

        public UserController(IUnitOfWork unitOfWork,IMapper mapper, IDistributedCache distributedCache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        [Route("GetAllUsers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Get()
        {
            try
            {
                var cacheKey = "usersList";
                var userList = new List<UserDto>();
                var serializedUserList = await _distributedCache.GetStringAsync(cacheKey);
                if (serializedUserList == null)
                {
                    var users = _unitOfWork.User.GetAll();
                    if (users == null || !users.Any())
                    {
                        return new ApiResponse { StatusCode = HttpStatusCode.NotFound, Messages = "No User Found" };
                    }

                    userList = _mapper.Map<IEnumerable<UserDto>>(users).ToList();
                    serializedUserList = JsonConvert.SerializeObject(userList);
                    var options = new DistributedCacheEntryOptions()
                        .SetAbsoluteExpiration(DateTime.Now.AddMinutes(10))
                        .SetSlidingExpiration(TimeSpan.FromMinutes(2));
                    await _distributedCache.SetStringAsync(cacheKey, serializedUserList, options);
                }
                else
                {
                    userList = JsonConvert.DeserializeObject<List<UserDto>>(serializedUserList);
                }

                return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = userList };
            }
            catch (Exception ex)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Messages = ex.Message, IsSuccess = false };
            }
        }


        [HttpGet]
        [Route("GetAUser")]
        public ActionResult<ApiResponse> Get(int id)
        {
            try
            {
                var user = _unitOfWork.User.Get(id);
                if (user == null)
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.NotFound, Messages = "No User Found" };
                }
                return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = user };
            }
            catch (Exception ex) 
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Messages = ex.Message, IsSuccess = false };
            }
        }


        [HttpPost]
        public ActionResult<ApiResponse> CreateUser(UserDto user)
        {
            try
            {
                if (user == null)
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.BadRequest, Messages = "Invalid Operation" };
                }
                var newUser = _mapper.Map<User>(user);
                _unitOfWork.User.Add(newUser);
                return new ApiResponse { StatusCode = HttpStatusCode.Created, IsSuccess = true, Messages = "User Profile Created Successfully" };
            }
            catch (Exception ex) 
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Messages = ex.Message, IsSuccess = false };
            }
        }

        [HttpPut]
        public IActionResult UpdateUser(UserDto user)
        {
            if (user == null)
            {
                return BadRequest();
            }
            var updatedUser = _mapper.Map<User>(user);
            _unitOfWork.User.Update(updatedUser);
            return Ok();
        }

        [HttpDelete]
        public IActionResult DeleteUser(int id)
        {
             _unitOfWork.User.Delete(id);
             return Ok();

        }


        [HttpGet("FindUser")]
        public ActionResult<ApiResponse> FindUsers(int? age, string? gender, string? maritalStatus, string? location)
        {
            try
            {
                var users = _unitOfWork.User.Find(u =>
                    (!age.HasValue || u.Age == age.Value) &&
                    (string.IsNullOrEmpty(gender) || u.Gender.ToLower() == gender.ToLower()) &&
                    (string.IsNullOrEmpty(maritalStatus) || u.MaritalStatus.ToLower() == maritalStatus.ToLower()) &&
                    (string.IsNullOrEmpty(location) || u.Location.ToLower() == location.ToLower())).ToList();
                if (users.Count == 0)
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.NotFound, Messages = "No User Found", IsSuccess = false };
                }
                else
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = users };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, IsSuccess = false, Messages = ex.Message };
            }
        }


    }
}
