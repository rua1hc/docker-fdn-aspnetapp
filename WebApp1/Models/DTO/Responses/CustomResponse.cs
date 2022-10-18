using System.Security.Claims;
using WebApp1.Config;
using WebApp1.Domain;

namespace WebApp1.Models.DTO.Responses
{
    public class CustomResponse : AuthResult
    {
    }

    public class UserResponse 
    {
        public UserViewModel User { get; set; }
        public IList<string> Roles { get; set; }
        public IList<Claim> Claims { get; set; }
    }
}
