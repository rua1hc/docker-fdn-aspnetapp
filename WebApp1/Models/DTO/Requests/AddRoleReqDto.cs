using System.ComponentModel.DataAnnotations;

namespace WebApp1.Models.DTO.Requests
{
    public class AddRoleReqDto
    {
        [Required]
        public string Role { get; set; } = null!;
    }
}
