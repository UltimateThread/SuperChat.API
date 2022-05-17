using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SuperChat.API.Models;
using SuperChat.API.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace SuperChat.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly SuperChatContext _ctx;

        public AuthController(IConfiguration configuration, IUserService userService, SuperChatContext ctx)
        {
            _configuration = configuration;
            _userService = userService;
            _ctx = ctx;
        }

        [HttpGet, Authorize]
        public ActionResult<string> GetMe()
        {
            var userName = _userService.GetMyName();
            return Ok(userName);
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(RegisterDTO registerDTO)
        {
            // Check if the email address already exists in the DB
            // If it does then the user is already registered
            try
            {
                User existingUser = _ctx.Users.FirstOrDefault(x => x.Email == registerDTO.Email);
                if (existingUser != null)
                {
                    return BadRequest("User Already Exists");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error Checking for Existing User");
            }

            User newUser;
            try
            {
                CreatePasswordHash(registerDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);

                newUser = new User()
                {
                    Email = registerDTO.Email,
                    FirstName = registerDTO.FirstName,
                    LastName = registerDTO.LastName,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt
                };

                _ctx.Users.Add(newUser);
                await _ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("Error Creating User");
            }

            if (newUser == null)
            {
                return BadRequest("Error Creating User");
            }

            try
            {
                string token = CreateToken(newUser);

                UserDTO userDTO = new UserDTO()
                {
                    Email = newUser.Email,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    Password = "",
                    JWTToken = token
                };

                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                return BadRequest("Error Creating JWT Token");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDTO loginDTO)
        {
            if (loginDTO == null)
            {
                return BadRequest("UserDTO Object Malformed");
            }

            User? user = _ctx.Users.FirstOrDefault(x => x.Email == loginDTO.Email);
            if (user == null)
            {
                return BadRequest($"User with Email Address '{loginDTO.Email}' Does Not Exist");
            }

            if (!VerifyPasswordHash(loginDTO.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Invalid password.");
            }

            try
            {
                string token = CreateToken(user);

                UserDTO userDTO = new UserDTO()
                {
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Password = "",
                    JWTToken = token
                };

                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                return BadRequest("Error Creating JWT Token");
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Globals.SecurityPassword));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
