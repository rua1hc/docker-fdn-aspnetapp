using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebApp1.Config;
using WebApp1.Data;
using WebApp1.Domain;
using WebApp1.Models;
using WebApp1.Models.DTO.Requests;
using WebApp1.Models.DTO.Responses;

namespace WebApp1.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly JwtConfig _jwtConfig; 
        private readonly ApplicationDbContext _apiDbContext;
        public UsersController(UserManager<IdentityUser> userManager,
                                RoleManager<IdentityRole> roleManager,
                                TokenValidationParameters tokenValidationParameters,
                                IOptionsMonitor<JwtConfig> optionsMonitor,
                                ApplicationDbContext apiDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenValidationParameters = tokenValidationParameters;
            _jwtConfig = optionsMonitor.CurrentValue;
            _apiDbContext = apiDbContext;
        }

        // GET: Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<IdentityUser>>> GetUsers()
        {
            return await _userManager.Users.ToListAsync();
        }

        // GET: Users/1
        [HttpGet("{Id}")]
        public async Task<ActionResult<IdentityUser>> GetUser(string Id)
        {
            var User = await _userManager.FindByIdAsync(Id);
            return User == null ? NotFound() : Ok(User);
        }

        // POST: Users
        [HttpPost]
        public async Task<ActionResult<IdentityUser>> AddUser(UserRegisterReqDto user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);

                if (existingUser != null) return BadRequest(new RegistrationResponse()
                    {
                        Success = false,
                        Errors = new List<string>(){ "Email already exist" }
                    });

                var newUser = new IdentityUser() { Email = user.Email, UserName = user.Email };
                var isCreated = await _userManager.CreateAsync(newUser, user.Password);
                if (isCreated.Succeeded)
                {
                    //var jwtToken = GenerateJwtToken(newUser);
                    //return Ok(new RegistrationResponse()
                    //        {
                    //            Success = true,
                    //            Token = jwtToken
                    //        });

                    //await _userManager.AddToRoleAsync(newUser, user.Role);

                    return Ok(await GenerateJwtToken(newUser));
                }

                return new JsonResult(new RegistrationResponse(){
                    Success = false, 
                    Errors = isCreated.Errors.Select(x => x.Description).ToList()
                }) { StatusCode = 500 };
            }

            return BadRequest(new RegistrationResponse()
            {
                Success = false,
                Errors = new List<string>(){ "Invalid payload" }
            });
        }

        //// PUT: User/2
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateUser(int id, User User)
        //{
        //    if (id != User.Id) return BadRequest();

        //    var UserDb = await _context.Users.FindAsync(id);
        //    if (UserDb == null) return NotFound();

        //    UserDb.FirstName = User.FirstName;
        //    UserDb.LastName = User.LastName;
        //    UserDb.Email = User.Email;
        //    UserDb.Phone = User.Phone;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException) when (!UserExists(id))
        //    {
        //        return NotFound();
        //    }

        //    return NoContent();
        //}

        // DELETE: Users/3
        [HttpDelete("{Id}")]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            var existingUser = await _userManager.FindByIdAsync(Id);
            if (existingUser == null) return NotFound();

            IdentityResult result = await _userManager.DeleteAsync(existingUser);
            if (result.Succeeded) return Ok(existingUser);

            return new JsonResult(new RegistrationResponse() {
                Success = false, 
                Errors = result.Errors.Select(x => x.Description).ToList() 
            }) { StatusCode = 500 };
        }


        //
        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (ModelState.IsValid)
            {
                var res = await VerifyToken(tokenRequest);

                if (res == null)
                {
                    return BadRequest(new RegistrationResponse()
                    {
                        Errors = new List<string>() { "Invalid tokens" },
                        Success = false
                    });
                }

                return Ok(res);
            }

            return BadRequest(new RegistrationResponse()
            {
                Errors = new List<string>() { "Invalid payload" },
                Success = false
            });
        }

        private async Task<AuthResult> VerifyToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = jwtTokenHandler.ValidateToken(tokenRequest.Token, _tokenValidationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);

                    if (result == false)
                    {
                        return null;
                    }
                }

                //var utcExpiryDate = long.Parse(principal.Claims.First(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                //var expDate = UnixTimeStampToDateTime(utcExpiryDate);
                //if (expDate > DateTime.UtcNow)
                //{
                //    return new AuthResult()
                //    {
                //        Errors = new List<string>() { "Cannot refresh this since the token has not expired" },
                //        Success = false
                //    };
                //}

                var storedRefreshToken = await _apiDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);
                if (storedRefreshToken == null)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "refresh token doesnt exist" },
                        Success = false
                    };
                }

                if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "token has expired, user needs to relogin" },
                        Success = false
                    };
                }

                if (storedRefreshToken.IsUsed)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "token has been used" },
                        Success = false
                    };
                }

                if (storedRefreshToken.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "token has been revoked" },
                        Success = false
                    };
                }

                var jti = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                // check the id that the recieved token has against the id saved in the db
                if (storedRefreshToken.JwtId != jti)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "the token doenst mateched the saved token" },
                        Success = false
                    };
                }

                storedRefreshToken.IsUsed = true;
                _apiDbContext.RefreshTokens.Update(storedRefreshToken);
                await _apiDbContext.SaveChangesAsync();

                var dbUser = await _userManager.FindByIdAsync(storedRefreshToken.UserId);
                return await GenerateJwtToken(dbUser);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        private async Task<AuthResult> GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    //new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    // the JTI is used for refresh token
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                                                            SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                IsUsed = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                IsRevoked = false,
                Token = RandomString(25) + Guid.NewGuid()
            };

            await _apiDbContext.RefreshTokens.AddAsync(refreshToken);
            await _apiDbContext.SaveChangesAsync();

            return new AuthResult()
            {
                Token = jwtToken,
                Success = true,
                RefreshToken = refreshToken.Token
            };
        }


        public string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                        .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public string BuildToken(Claim[] claims)
        {

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.Secret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                    notBefore: DateTime.Now,
                    claims: claims,
                    //expires: DateTime.Now.AddMinutes(_jwtConfig.accessTokenExpiration),
                    signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string BuildRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }

}
