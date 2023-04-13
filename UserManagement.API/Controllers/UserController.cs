using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Net;
using UserManagement.API.Dto;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Repository;
using UserManagement.Domain.RequestDto;

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
        public ActionResult<ApiResponse> Get(string id)
        {
            try
            {
                var user = _unitOfWork.User.Get(id);
                if (user == null)
                {
                    return new ApiResponse { StatusCode = HttpStatusCode.NotFound, Messages = "No User Found" };
                }
                var theuser = _mapper.Map<UserDto>(user);
                return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = theuser };
            }
            catch (Exception ex) 
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Messages = ex.Message, IsSuccess = false };
            }
        }


        [HttpPost]
        [Route("Register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Register([FromBody] RegisterDto registerDto)
        {
          
            try
            {
                var errors = await _unitOfWork.User.Register(registerDto);

                if (errors.Any())
                {
                    foreach (var error in errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return BadRequest(ModelState);
                }

                return Ok();
            }
            catch (System.Exception ex)
            {
                return Problem($"something went wrong in the {nameof(Register)},Please contact Support", statusCode: 500);
            }

        }

    
        
        [HttpPost]
        [Route("login")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {

            try
            {
                var authResponse = await _unitOfWork.User.Login(loginDto);

                if (authResponse == null)
                {
                    return Unauthorized();
                }

                return Ok(authResponse);
            }
            catch (System.Exception)
            {
                return Problem($"Something went wrong in the {nameof(Login)},Please contact Support", statusCode: 500);
            }

        }


        //[HttpPost]
        //public ActionResult<ApiResponse> CreateUser(UserDto user)
        //{
        //    try
        //    {
        //        if (user == null)
        //        {
        //            return new ApiResponse { StatusCode = HttpStatusCode.BadRequest, Messages = "Invalid Operation" };
        //        }
        //        var newUser = _mapper.Map<User>(user);
        //        _unitOfWork.User.Add(newUser);
        //        return new ApiResponse { StatusCode = HttpStatusCode.Created, IsSuccess = true, Messages = "User Profile Created Successfully" };
        //    }
        //    catch (Exception ex) 
        //    {
        //        return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, Messages = ex.Message, IsSuccess = false };
        //    }
        //}




        [HttpPut]
        [Route("UpdateAUserProfile")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApiResponse> UpdateUser(UserDto user)
        {
            if (user == null)
            {
                return new ApiResponse { IsSuccess = false , StatusCode = HttpStatusCode.BadRequest};
            }
            var userTobeUpdated =  _unitOfWork.User.Get(user.Id);
            if (userTobeUpdated == null)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.NotFound, Messages = "No User Found" };
            }
            var updatedUser = _mapper.Map<User>(userTobeUpdated);
            _unitOfWork.User.Update(updatedUser);
            return Ok();
        }





        [HttpDelete]
        [Route("DeleteAUserProfile")]
        [Authorize(Roles ="Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<ApiResponse> DeleteUser(string id)
        {
            var user = _unitOfWork.User.Get(id);
            if (user == null) 
            {
                return new ApiResponse { StatusCode = HttpStatusCode.NotFound, Messages = "No User Found" };
            }
             _unitOfWork.User.Delete(id);
             return new ApiResponse { IsSuccess=true,StatusCode = HttpStatusCode.OK , Messages = "User Account Deleted Successfully" };

        }




        [HttpGet("FindUser")]
        [Authorize(Roles ="Administrator")]
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
                    var theusers = _mapper.Map<UserDto>(users);
                    return new ApiResponse { StatusCode = HttpStatusCode.OK, IsSuccess = true, Result = theusers };
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse { StatusCode = HttpStatusCode.InternalServerError, IsSuccess = false, Messages = ex.Message };
            }
        }


    }
}
