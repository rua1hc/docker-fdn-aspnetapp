using Microsoft.EntityFrameworkCore;

public class DotNetTraining : DbContext
{
    public DotNetTraining(DbContextOptions<DotNetTraining> options)
            : base(options) { }

    public virtual DbSet<Contact> Contacts { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    optionsBuilder.UseMySQL("server=localhost;database=DotNetTraining;user=root;password=240690");
    //}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.LastName).HasMaxLength(255);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(255);

            //Id(int, primaryKey, auto increment)
            //FirstName(varchar(255), not null)
            //LastName(varchar(255), null)
            //Email(varchar(255), not null, unique)
            //Phone(varchar(255), null)
        });
    }
}
