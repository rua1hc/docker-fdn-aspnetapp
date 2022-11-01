using cs_fdn.EntityConfig;
using System.Collections.Generic;
using System.Data.Entity;

namespace cs_fdn
{
    public class Naruto : DbContext
    {
        // Your context has been configured to use a 'Model1' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'cs_fdn.Model1' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'Model1' 
        // connection string in the application configuration file.
        public Naruto()
            : base("name=DefaultConnection")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Course>()
            //    .Property(p => p.Name).IsRequired().HasMaxLength(255);

            //modelBuilder.Entity<Course>()
            //    .HasRequired(c => c.Author)
            //    .WithMany(a => a.Courses)
            //    .HasForeignKey(c => c.AuthorId)
            //    .WillCascadeOnDelete(false);

            //modelBuilder.Entity<Course>()
            //    .HasMany(c => c.Tags)
            //    .WithMany(t => t.Courses)
            //    .Map(m =>
            //    {
            //        m.ToTable("CoursesTags");
            //        m.MapLeftKey("CourseId");
            //        m.MapRightKey("TagId");
            //    });

            //base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations.Add(new CourseConfig());
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Author> Authors { get; set; }
        public virtual DbSet<Tag> Tags { get; set; }
        //public virtual DbSet<Category> Categories { get; set; }
    }

    //public class Category
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}

    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Author Author { get; set; }
        public int AuthorId { get; set; }
        public string Description { get; set; }
        //public Category Category { get; set; }
        //public DateTime? DatePublish { get; set; }
        public CourseLevel Level { get; set; }
        public IList<Tag> Tags { get; set; }
    }
    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Course> Courses { get; set; }
    }
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Course> Courses { get; set; }
    }

    public enum CourseLevel
    {
        Beginner = 1,
        Intermediate = 2,
        Advance = 3,
    }
}