using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core.Tokenizer;
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginDto loginDto)
        {
            try
            {
                string jwtToken = await _userManager.SignIn(loginDto);
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                };
                Response.Cookies.Append("jwt", jwtToken, cookieOptions);
                return Ok();
            }
            catch(NotFoundException ex)
            {
                return NotFound();
            }
            catch(ValidationException ex)
            {
                return Unauthorized();
            }
            
        }

    }
}
