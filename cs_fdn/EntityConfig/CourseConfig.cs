using System.Data.Entity.ModelConfiguration;

namespace cs_fdn.EntityConfig
{
    internal class CourseConfig : EntityTypeConfiguration<Course>
    {
        public CourseConfig()
        {
            //ToTable("tbl_name");

            //HasKey(c => c.Id);

            Property(p => p.Name).IsRequired().HasMaxLength(255);

            HasRequired(c => c.Author)
            .WithMany(a => a.Courses)
            .HasForeignKey(c => c.AuthorId)
            .WillCascadeOnDelete(false);

            HasMany(c => c.Tags)
            .WithMany(t => t.Courses)
            .Map(m =>
            {
                m.ToTable("CoursesTags");
                m.MapLeftKey("CourseId");
                m.MapRightKey("TagId");
            });
        }
    }
}
