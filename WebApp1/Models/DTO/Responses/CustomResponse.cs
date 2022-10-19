using System.Security.Claims;
using WebApp1.Config;
using WebApp1.Domain;

namespace WebApp1.Models.DTO.Responses
{
    public class CustomResponse : AuthResult
    {
    }

    class UserTokenMixedResponse
    {
        public UserViewModel User { get; set; } = null!;
        public IList<string> Roles { get; set; } = null!;
        public IList<Claim> Claims { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class UserResponse 
    {
        public UserViewModel User { get; set; } = null!;
        public IList<string> Roles { get; set; } = null!;
        public IList<Claim> Claims { get; set; } = null!;
    }
}
