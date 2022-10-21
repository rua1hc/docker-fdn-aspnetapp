using System.ComponentModel.DataAnnotations;

namespace WebApp1.Models.DTO.Requests
{
    public class TokenRequestDto
    {
        [Required]
        public string AccessToken { get; set; } = null!;
        [Required]
        public string RefreshToken { get; set; } = null!;
    }
}
