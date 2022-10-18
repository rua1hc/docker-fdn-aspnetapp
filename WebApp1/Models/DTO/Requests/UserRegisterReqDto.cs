using System.ComponentModel.DataAnnotations;

namespace WebApp1.Models.DTO.Requests
{
    public class UserRegisterReqDto
    {
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string Password { get; set; } = null!;
        [Required]
        public string Role { get; set; } = null!;
    }
}
