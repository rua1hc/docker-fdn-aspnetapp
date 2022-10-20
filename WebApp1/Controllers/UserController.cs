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

        [HttpPost]
        [Route("Token")]
        public async Task<IActionResult> Login(UserLoginReqDto user)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(user.Email);
                if (existingUser == null) return BadRequest(new CustomResponse()
                {
                    Success = false,
                    Errors = new List<string>() { "Invalid authentication request" }
                });

                var isCorrect = await _userManager.CheckPasswordAsync(existingUser, user.Password);
                if (isCorrect)
                {
                    var storedRefreshToken = await _apiDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == existingUser.Id);
                    //if (storedRefreshToken == null)
                    var result = await GenerateJwtToken(existingUser, storedRefreshToken);

                    return Ok(new TokenResponse()
                    {
                        AccessToken = result.AccessToken,
                        RefreshToken = result.RefreshToken
                    });
                }
                else
                {
                    return BadRequest(new CustomResponse()
                    {
                        Success = false,
                        Errors = new List<string>() { "Invalid authentication request" }
                    });
                }
            }

            return BadRequest(new CustomResponse()
            {
                Success = false,
                Errors = new List<string>() { "Invalid payload" }
            });
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
            if (user == null) return NotFound();

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
        [Authorize(Policy = "AdminRole")]
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
                    var createdUser = await _userManager.FindByEmailAsync(user.Email);
                    var result = await _userManager.AddToRoleAsync(createdUser, user.Role);

                    var defaultClaim = new Claim("Permission", DefaultClaimConfig.Permission[user.Role]);
                    result = await _userManager.AddClaimAsync(createdUser, defaultClaim);

                    var jwtGenerated = await GenerateJwtToken(newUser);
                    if (!jwtGenerated.Success) return Ok(new UserResponse()
                    {
                        User = _mapper.Map<UserViewModel>(createdUser),
                        Roles = new List<string> { user.Role },
                        Claims = new List<Claim> { defaultClaim }
                    });

                    return Ok(new UserTokenMixedResponse()
                    {
                        User = _mapper.Map<UserViewModel>(createdUser),
                        Roles = new List<string> { user.Role },
                        Claims = new List<Claim> { defaultClaim },
                        AccessToken = jwtGenerated.AccessToken,
                        RefreshToken = jwtGenerated.RefreshToken
                    });
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

        // PUT: Users/email
        [HttpPut("{email}")]
        [Authorize(Policy = "AdminRole")]
        public async Task<IActionResult> UpdateUser(string email, UserRegisterReqDto user)
        {
            if (email != user.Email) return BadRequest();

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null) return NotFound();

            existingUser.Email = user.Email;

            await _userManager.UpdateAsync(existingUser);

            return Ok();
        }

        // DELETE: Users/3
        [HttpDelete("{email}")]
        [Authorize(Policy = "AdminRole")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
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
        [Route("{email}/roles")]
        public async Task<IActionResult> UpdateUserRole(string email, AddRoleReqDto roleReq)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null) return NotFound();

            var existingRole = await _roleManager.FindByNameAsync(roleReq.Role);
            if (existingRole == null) return NotFound();

            var result = await _userManager.AddToRoleAsync(existingUser, roleReq.Role);

            return result.Succeeded ? Ok() : BadRequest(new { error = result.Errors });
        }

        [HttpPut]
        [Route("{email}/claims")]
        //[Authorize(Policy = "AdminRole")]
        public async Task<IActionResult> UpdateUserClaim(string email,[FromBody] Dictionary<string, string> claims)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser == null) return NotFound();

            //var existingClaims = await _userManager.GetClaimsAsync(existingUser);

            var claimList = new List<Claim>();
            foreach (var claim in claims)
            {
                claimList.Add(new Claim(claim.Key, claim.Value));
            }
            var result =await _userManager.AddClaimsAsync(existingUser, claimList);

            return result.Succeeded ? Ok() : BadRequest(new { error = result.Errors });
        }

        [HttpPost]
        [Route("revoke")]
        public async Task<AuthResult> RevokeTokenHandler(TokenRequestDto tokenRequest)
        {
            if (ModelState.IsValid)
            {
                var storedRefreshToken = await _apiDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);
                if (storedRefreshToken == null) return (new AuthResult()
                {
                    Errors = new List<string>() { "Refresh token doesnt exist" },
                    Success = false
                });

                storedRefreshToken.IsRevoked = true;
                _apiDbContext.RefreshTokens.Update(storedRefreshToken);
                await _apiDbContext.SaveChangesAsync();

                return (new CustomResponse() { Success = true });
            }

            return (new CustomResponse()
            {
                Success = false,
                Errors = new List<string>() { "Invalid payload" }
            });
        }


        //////
        [HttpPost]
        [Route("RefreshToken")]
        [Authorize(Policy = "AdminRole")]
        public async Task<IActionResult> RefreshTokenHandler(TokenRequestDto tokenRequest)
        {
            if (ModelState.IsValid)
            {
                var res = await VerifyToken(tokenRequest);

                if (res == null)
                {
                    return BadRequest(new CustomResponse()
                    {
                        Errors = new List<string>() { "Invalid tokens!" },
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

        private async Task<AuthResult?> VerifyToken(TokenRequestDto tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = jwtTokenHandler.ValidateToken(tokenRequest.AccessToken,
                                    _tokenValidationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                                    StringComparison.InvariantCultureIgnoreCase);

                    if (result == false) return null;
                }

                //cmt out to forbid renew access token before exp. time
                //var utcExpiryDate = long.Parse(principal.Claims.First(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
                //var expDate = UnixTimeStampToDateTime(utcExpiryDate);
                //if (expDate > DateTime.UtcNow)
                //{
                //    return new AuthResult()
                //    {
                //        Errors = new List<string>() { "Cannot refresh since the token has not expired" },
                //        Success = false
                //    };
                //}

                var storedRefreshToken = await _apiDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);
                if (storedRefreshToken == null)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "Refresh token doesnt exist" },
                        Success = false
                    };
                }

                //if (storedRefreshToken.IsUsed)
                //{
                //    return new AuthResult()
                //    {
                //        Errors = new List<string>() { "Token has been used" },
                //        Success = false
                //    };
                //}

                if (storedRefreshToken.IsRevoked)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "Token has been revoked" },
                        Success = false
                    };
                }

                var JtiClaim = principal.Claims.SingleOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti);
                if (JtiClaim == null) return null;

                if (storedRefreshToken.JwtId != JtiClaim.Value)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "Token Id doenst matched" },
                        Success = false
                    };
                }

                if (DateTime.UtcNow > storedRefreshToken.ExpiryDate)
                {
                    return new AuthResult()
                    {
                        Errors = new List<string>() { "Token expired, pls. relogin" },
                        Success = false
                    };
                }

                var dbUser = await _userManager.FindByIdAsync(storedRefreshToken.UserId);
                return await GenerateJwtToken(dbUser, storedRefreshToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Invalid tokens: {ex.Message}");
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
                Expires = DateTime.UtcNow.AddHours(1),
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
                    AccessToken = jwtToken,
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
                    AccessToken = jwtToken,
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
