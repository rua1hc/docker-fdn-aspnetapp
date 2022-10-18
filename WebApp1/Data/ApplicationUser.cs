using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApp1.Data
{

    //
    //public class ApplicationUser : IdentityUser
    //{
    //    [Required]
    //    public string firstName { get; set; } = null!;
    //    public string LastName { get; set; } = null!;
    //}

    //JWT
    public class CustomerUser
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class UserConfiguration : IEntityTypeConfiguration<CustomerUser>
    {
        public void Configure(EntityTypeBuilder<CustomerUser> builder)
        {
            builder.HasData(
                new CustomerUser { Id = 1, Email = "test@gmail.com", Password = "1234" },
                new CustomerUser { Id = 2, Email = "abc@gmail.con", Password = "1234" }
            );
        }
    }
}

