using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;
using TipRecipe.DbContexts;
using TipRecipe.Entities;
using Microsoft.Identity.Client;
using TipRecipe.Models.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using TipRecipe.Models.HttpExceptions;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace TipRecipe.Services
{
    public class UserManager
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserManager(ApplicationDbContext applicationDbContext, IConfiguration configuration)
        {
            _context = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<string> SignIn(LoginDto loginDto)
        {
            User? user = _context.Users.Where(
                u => u.Email == loginDto.Email 
                || u.UserName == loginDto.Email)
                .Include(u => u.UserRoles)
                .FirstOrDefault();
            if(user == null)
            {
                throw new NotFoundException();
            }
            if (!VerifyPassword(loginDto.Password, user.Password))
            {
                throw new ValidationException();
            }
            var securityKey = new SymmetricSecurityKey(
                Convert.FromBase64String(_configuration["Jwt:Key"]?? throw new ArgumentNullException("JWT config")));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claimForToken = this.GetUserClaims(user);
            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claimForToken,
                DateTime.UtcNow,
                DateTime.UtcNow.AddHours(1),
                signingCredentials);
            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return tokenToReturn;
        }

        public async Task SignOut()
        {
            //logout
        }

        private IEnumerable<Claim> GetUserClaims(User user)
        {
            List<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserID));
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.AddRange(this.GetUserRoleClaims(user));
            return claims;
        }

        private IEnumerable<Claim> GetUserRoleClaims(User user)
        {
            List<Claim> claims = new List<Claim>();
            foreach (var role in user.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.ToString()));
            }
            return claims;
        }

        private bool VerifyPassword(string password, string hash)
        {
            return true;
            //return BCrypt.Net.BCrypt.Verify(password, hash);
        }

    }
}
