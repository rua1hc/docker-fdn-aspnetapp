using System.ComponentModel.DataAnnotations;

namespace WebApp1.Models.DTO.Requests
{
    public class UserRegisterReqDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
    }
}
