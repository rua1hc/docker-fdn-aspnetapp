//dotnet - ef migrations script --output migrateDb4Docker.sql --context NetCourse --idempotent

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Course_service.Models
{
    public class NetCourse : DbContext
    {
        public NetCourse(DbContextOptions<NetCourse> options) : base(options) { }

        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<Enrollment> Enrollments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments);

            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(e => e.Code).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(255);
                entity.Property(e => e.Price).IsRequired();
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.Property(e => e.EnrolledDate).IsRequired();
            });
        }
    }
}
