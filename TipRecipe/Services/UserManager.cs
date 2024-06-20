using System.Data;
using System.Security.Claims;
using TipRecipe.DbContexts;
using TipRecipe.Entities;
using TipRecipe.Models.Dto;
using TipRecipe.Models.HttpExceptions;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace TipRecipe.Services
{
    public class UserManager
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserManager(ApplicationDbContext applicationDbContext, IConfiguration configuration, IMapper mapper)
        {
            _context = applicationDbContext ?? throw new ArgumentNullException(nameof(applicationDbContext));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<User> GetUserAsync(string userId)
        {
            User? user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null)
            {
                throw new NotFoundException();
            }
            return user;
        }

        public async Task UpdateProfileAsync(UserUpdateDto userUpdateDto, string userID)
        {
           
            User? user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserID == userID);
            if (user is null)
            {
                throw new NotFoundException();
            }
            user.UserName = userUpdateDto.Username;
            if(userUpdateDto.NewPassword != "")
            {
                user.Password = HashPassword(userUpdateDto.NewPassword);
            }
            if (await _context.SaveChangesAsync() <= 0)
            {
                throw new Exception("Failed to update user profile");
            }
            
        }

        public async Task<(User, string, DateTime)> SignIn(UserLoginDto loginDto)
        {
            User? user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email 
                || u.UserName == loginDto.Email);
            if(user == null)
            {
                throw new NotFoundException();
            }
            if (!VerifyPassword(loginDto.Password, user.Password!))
            {
                throw new ValidationException();
            }
            return GenerateToken(user);
        }

        public async Task<(User, string, DateTime)> SignUp(UserRegisterDto userRegisterDto)
        {
            User? user = await _context.Users.Where(u => u.Email == userRegisterDto.Email).FirstOrDefaultAsync();
            if (user != null)
            {
                throw new ConflicException("Email already exist!");
            }
            user = _mapper.Map<User>(userRegisterDto);
            user.Password = HashPassword(userRegisterDto.Password);
            user.UserRoles = new List<UserRole>
            {
                new UserRole(RoleType.USER)
            };
            _context.Users.Add(user);
            if (await _context.SaveChangesAsync() <= 0)
            {
                throw new Exception("Failed to register user");
            }
            return GenerateToken(user);
        }

        public async Task SignOut()
        {
            await Task.Delay(1000);
            //logout
        }

        private (User, string, DateTime) GenerateToken(User currentUser)
        {
            var securityKey = new SymmetricSecurityKey(
                Convert.FromBase64String(_configuration["Jwt:Key"] ?? throw new ArgumentNullException("JWT config")));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claimForToken = this.GetUserClaims(currentUser);
            DateTime createdTime = DateTime.UtcNow;
            DateTime expriedTime = createdTime.AddHours(12);
            var jwtSecurityToken = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claimForToken,
                createdTime,
                expriedTime,
                signingCredentials);
            var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return (currentUser, tokenToReturn, expriedTime);
        }

        private IEnumerable<Claim> GetUserClaims(User user)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserID));
            claims.Add(new Claim(ClaimTypes.Email, user.Email!));
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

        private string HashPassword(string password)
        {
            // Generate a 128-bit salt using a sequence of
            // cryptographically strong random bytes.
            byte[] salt = RandomNumberGenerator.GetBytes(128 / 8);
            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }

        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            var parts = storedPasswordHash.Split('.');
            if (parts.Length != 2)
            {
                throw new FormatException("Unexpected password format");
            }

            var salt = Convert.FromBase64String(parts[0]);
            var storedHash = parts[1];

            var enteredHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: enteredPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));

            return storedHash == enteredHash;
        }

    }
}
