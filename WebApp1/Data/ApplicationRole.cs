using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace WebApp1.Data
{
    public static class DefaultClaimConfig
    {
        public static readonly Dictionary<string, string> Permission = new Dictionary<string, string>
        {
            { "Admin", "Admin.Manage.Full" },
            { "Staff", "Staff.Assigment.Class" },
            { "Member", "Member.Enrollment.Course" }
        };
    }

    public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole
                {
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole 
                {
                    Name = "Staff",
                    NormalizedName = "STAFF"
                },
                new IdentityRole
                {
                    Name = "Member",
                    NormalizedName = "MEMBER"
                }
            );
        }
    }
}

