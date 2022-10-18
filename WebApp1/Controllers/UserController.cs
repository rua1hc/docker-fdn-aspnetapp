using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApp1.Config;
using WebApp1.Data;
using WebApp1.Domain;
using WebApp1.Models;
using WebApp1.Models.DTO.Requests;
using WebApp1.Models.DTO.Responses;

namespace WebApp1.Controllers
{
    //todo1 add login
    //todo2 add cors
    //Header Autho..:bearer jwt
    //[author(authSchem = jwtBeardef.authSchem, Role = "Admin")]
    [Route("[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly JwtConfig _jwtConfig;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _apiDbContext;
        public UsersController(UserManager<IdentityUser> userManager,
                                RoleManager<IdentityRole> roleManager,
                                TokenValidationParameters tokenValidationParameters,
                                IOptionsMonitor<JwtConfig> optionsMonitor,
                                IMapper mapper,
        ApplicationDbContext apiDbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenValidationParameters = tokenValidationParameters;
            _jwtConfig = optionsMonitor.CurrentValue;
            _mapper = mapper;
            _apiDbContext = apiDbContext;
        }

        // GET: Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserViewModel>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var usersViewModel = _mapper.Map<List<UserViewModel>>(users);
            return Ok(usersViewModel);
        }

        // GET: Users/email
        [HttpGet("{email}")]
        public async Task<ActionResult> GetUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return NotFound() ;

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return Ok(new UserResponse()
            {
                User = _mapper.Map<UserViewModel>(user),
                Roles = roles,
                Claims = claims
            });
        }

        // POST: Users
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IdentityUser>> AddUser(UserRegisterReqDto user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser != null) return BadRequest(new CustomResponse()
                {
                    Success = false,
                    Errors = new List<string>() { "Email already exist" }
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

                    var createdUser = await _userManager.FindByEmailAsync(user.Email);
                    var result = await _userManager.AddToRoleAsync(createdUser, user.Role);

                    var defaultClaim = new Claim("ClaimPermission", DefaultClaimConfig.Permission[user.Role]);
                    result = await _userManager.AddClaimAsync(createdUser, defaultClaim);

                    //check result.Succeeded?
                    return Ok(await GenerateJwtToken(newUser));
                }

                return new JsonResult(new CustomResponse()
                {
                    Success = false,
                    Errors = isCreated.Errors.Select(x => x.Description).ToList()
                })
                { StatusCode = 500 };
            }

            return BadRequest(new CustomResponse()
            {
                Success = false,
                Errors = new List<string>() { "Invalid payload" }
            });
        }

        // PUT: Users/2
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateUser(UserRegisterReqDto user)
        {
            //if (id != User.Id) return BadRequest();

            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            if (existingUser == null) return NotFound();

            existingUser.Email = user.Email;

            await _userManager.UpdateAsync(existingUser);

            return Ok();
        }

        // DELETE: Users/3
        [HttpDelete("{Id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteUser(string Id)
        {
            var existingUser = await _userManager.FindByIdAsync(Id);
            if (existingUser == null) return NotFound();

            IdentityResult result = await _userManager.DeleteAsync(existingUser);
            if (result.Succeeded) return Ok(existingUser);

            return new JsonResult(new CustomResponse()
            {
                Success = false,
                Errors = result.Errors.Select(x => x.Description).ToList()
            })
            { StatusCode = 500 };
        }

        //////
        [HttpPut]
        [Route("users/{Id}/roles")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateUserRole(string Id, string roles)
        {
            return Ok();
        }

        [HttpPut]
        [Route("users/{Id}/claims")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateUserClaim(string Id, string claims)
        {
            return Ok();
        }



        //////
        [HttpPost]
        [Route("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            if (ModelState.IsValid)
            {
                var res = await VerifyToken(tokenRequest);

                if (res == null)
                {
                    return BadRequest(new CustomResponse()
                    {
                        Errors = new List<string>() { "Invalid tokens" },
                        Success = false
                    });
                }

                return Ok(res);
            }

            return BadRequest(new CustomResponse()
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

                var utcExpiryDate = long.Parse(principal.Claims.First(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                var expDate = UnixTimeStampToDateTime(utcExpiryDate);
                if (expDate > DateTime.UtcNow)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "Cannot refresh this since the token has not expired" },
                        Success = false
                    };
                }

                var storedRefreshToken = await _apiDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);
                if (storedRefreshToken == null)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "refresh token doesnt exist" },
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

                if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "token has expired, user needs to relogin" },
                        Success = false
                    };
                }

                var dbUser = await _userManager.FindByIdAsync(storedRefreshToken.UserId);
                return await GenerateJwtToken(dbUser, storedRefreshToken);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<AuthResult> GenerateJwtToken(IdentityUser user, RefreshToken? storedRefreshToken = null)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

            var claims = await GetValidClaims(user);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            if (storedRefreshToken != null)
            {
                storedRefreshToken.JwtId = token.Id;
                storedRefreshToken.IsUsed = true;

                _apiDbContext.RefreshTokens.Update(storedRefreshToken);
                await _apiDbContext.SaveChangesAsync();

                return new AuthResult()
                {
                    Token = jwtToken,
                    Success = true,
                    RefreshToken = storedRefreshToken.Token
                };
            }
            else
            {
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
        }

        private async Task<List<Claim>> GetValidClaims(IdentityUser user)
        {
            IdentityOptions _options = new IdentityOptions();
            var claims = new List<Claim>
            {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(_options.ClaimsIdentity.UserIdClaimType, user.Id.ToString()),
                new Claim(_options.ClaimsIdentity.UserNameClaimType, user.UserName),
            };

            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            claims.AddRange(userClaims);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (Claim roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }
            return claims;
        }

        public string RandomString(int length)
        {
            var random = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                        .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        //
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


    }

}
