using Microsoft.EntityFrameworkCore;

//dotnet - ef migrations script --output initDb.sql --context DotNetTraining --idempotent
public class Net_Member : DbContext
{
    public Net_Member(DbContextOptions<Net_Member> options)
            : base(options) { }

    public virtual DbSet<User> Users { get; set; } = null!;

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    optionsBuilder.UseMySQL("server=localhost;database=DotNetTraining;user=root;password=240690");
    //}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.Entity<User>().HasKey(e => e.Id);
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.UserName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(255);
            entity.Property(e => e.LastName).HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(255);
            entity.Property(e => e.Balance).HasDefaultValue(0);
        });
    }
}
