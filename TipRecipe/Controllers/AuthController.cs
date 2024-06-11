using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TipRecipe.Entities;
using TipRecipe.Filters;
using TipRecipe.Models.Dto;
using TipRecipe.Models.HttpExceptions;
using TipRecipe.Services;

namespace TipRecipe.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Authorize("User")]
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
            var userID = User.Claims.Where(claim => claim.Type == ClaimTypes.NameIdentifier).First().Value;
            return Ok(await _userManager.GetUserAsync(userID));
        }

        [AllowAnonymous]
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
                    SameSite = SameSiteMode.Lax
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

        [AllowAnonymous]
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
                    SameSite = SameSiteMode.Lax
                };
                Response.Cookies.Append("jwt", payload.Item2, cookieOptions);
                return Ok(payload.Item1);
            }
            catch (ConflicException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpPost("update")]
        [TypeFilter(typeof(DtoResultFilterAttribute<User, UserDto>))]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto userUpdateDto)
        {
            var userID = User.Claims.Where(claim => claim.Type == ClaimTypes.NameIdentifier).First().Value;
            try
            {
                await _userManager.UpdateProfileAsync(userUpdateDto, userID);
                return CreatedAtAction(nameof(this.Auth), new { }, await _userManager.GetUserAsync(userID));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [AllowAnonymous]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            //await _userManager.SignOut();
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                Expires = DateTime.Now,
                SameSite = SameSiteMode.Lax
            };
            Response.Cookies.Append("jwt", "this is empty", cookieOptions);
            return NoContent();
        }

    }
}
