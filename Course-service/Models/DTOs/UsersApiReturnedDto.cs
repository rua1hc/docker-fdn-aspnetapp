using System.Text.Json.Serialization;

namespace Course_service.Models.DTOs
{
    public class UsersApiReturnedDto
    {
        //public int id { get; set; }
        //public string userName { get; set; }
        //public string firstName { get; set; }
        //public string lastName { get; set; }
        //public string email { get; set; }
        //public string phone { get; set; }
        //public int balance { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; } = null!;

        [JsonPropertyName("firstName")]
        public string? FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string? LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("phone")]
        public string? Phone { get; set; }

        [JsonPropertyName("balance")]
        public int Balance { get; set; }
    }
}
