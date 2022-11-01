namespace cs_fdn.Migrations
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<cs_fdn.Naruto>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(cs_fdn.Naruto context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.
            #region Add Tags
            var tags = new Dictionary<string, Tag>
            {
                {"c#", new Tag {Id = 1, Name = "c#"}},
                {"angularjs", new Tag {Id = 2, Name = "angularjs"}},
                {"javascript", new Tag {Id = 3, Name = "javascript"}},
                {"nodejs", new Tag {Id = 4, Name = "nodejs"}},
                {"oop", new Tag {Id = 5, Name = "oop"}},
                {"linq", new Tag {Id = 6, Name = "linq"}},
            };

            foreach (var tag in tags.Values)
                context.Tags.AddOrUpdate(t => t.Id, tag);
            #endregion

            #region Add Authors
            var authors = new List<Author>
            {
                new Author
                {
                    Id = 2,
                    Name = "Mosh Hamedani"
                },
                new Author
                {
                    Id = 3,
                    Name = "John Smith"
                }
            };

            foreach (var author in authors)
                context.Authors.AddOrUpdate(a => a.Id, author);
            #endregion

            #region Add Courses
            var courses = new List<Course>
            {
                new Course
                {
                    Id = 3,
                    Name = "C# Basics",
                    AuthorId = 1,
                    Description = "Description for C# Basics",
                    Level = CourseLevel.Beginner,
                    Tags = new Collection<Tag>()
                    {
                        tags["c#"]
                    }
                },
                new Course
                {
                    Id = 4,
                    Name = "Learn JavaScript Through Visual Studio 2013",
                    AuthorId = 1,
                    Description = "Description Learn Javascript",
                    Level = CourseLevel.Advance,
                    Tags = new Collection<Tag>()
                    {
                        tags["javascript"]
                    }
                }
            };

            foreach (var course in courses)
                context.Courses.AddOrUpdate(c => c.Id, course);
            #endregion
        }
    }
}
