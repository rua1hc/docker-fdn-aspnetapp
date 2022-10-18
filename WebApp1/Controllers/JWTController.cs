using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApp1.Config;

namespace WebApp1.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class JWTController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public IActionResult Private()
        {
            var list = new[]
            {
                new { Code = 1, Name = "restricted " },
                new { Code = 2, Name = "login first" }
            }.ToList();

            return Ok(list);
        }

        [HttpGet]
        public IActionResult Public()
        {
            var list = new[]
            {
                new { Code = 1, Name = "public test " },
                new { Code = 2, Name = "okla..." }
            }.ToList();

            return Ok(list);
        }

    }



}
