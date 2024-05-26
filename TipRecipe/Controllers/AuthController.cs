using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using TipRecipe.Entities;
using TipRecipe.Filters;
using TipRecipe.Models.Dto;
using TipRecipe.Models.HttpExceptions;
using TipRecipe.Services;

namespace TipRecipe.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager _userManager;

        public AuthController(UserManager userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        [HttpGet]
        [TypeFilter(typeof(DtoResultFilterAttribute<User, UserDto>))]
        public async Task<IActionResult> Auth()
        {
            User user = new User();
            user.UserName = "operationddd";
            user.Email = "operationddd@gmail.com";
            user.UserRoles.Add(new UserRole(RoleType.USER));
            return Ok(user);
        }

        [HttpPost("login")]
        [TypeFilter(typeof(DtoResultFilterAttribute<User,UserDto>))]
        public async Task<IActionResult> Login([FromBody]UserLoginDto loginDto)
        {
            try
            {
                (User, string, DateTime) payload = await _userManager.SignIn(loginDto);
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    Expires = payload.Item3,
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Append("jwt", payload.Item2, cookieOptions);
                return Ok(payload.Item1);
            }
            catch(NotFoundException)
            {
                return NotFound();
            }
            catch(ValidationException)
            {
                return Unauthorized();
            }
        }

        [HttpPost("register")]
        [TypeFilter(typeof(DtoResultFilterAttribute<User, UserDto>))]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto userRegisterDto)
        {
            try
            {
                (User, string, DateTime) payload = await _userManager.SignUp(userRegisterDto);
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    Expires = payload.Item3,
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Append("jwt", payload.Item2, cookieOptions);
                return Ok(payload.Item1);
            }
            catch (ConflicException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await _userManager.SignOut();
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                Expires = DateTime.Now,
                SameSite = SameSiteMode.Strict
            };
            Response.Cookies.Append("jwt", "this is empty", cookieOptions);
            return NoContent();
        }

    }
}
