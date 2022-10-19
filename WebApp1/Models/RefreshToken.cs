using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp1.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!; // Linked to the Identity UserId
        public string Token { get; set; } = null!;
        public string JwtId { get; set; } = null!; // Map refreshToken with jwtId
        public bool IsUsed { get; set; } // generate a new Jwt token with the same refresh token?
        public bool IsRevoked { get; set; } // if it has been revoke for security reasons
        public DateTime AddedDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; } = null!;
    }
}
