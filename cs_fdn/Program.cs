
using System;
using System.Data.Entity;
using System.Linq;

namespace cs_fdn
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            //=====*=====*=====
            var context = new Naruto();

            //lazy - eager - explicit loading
            var eagerLoading = context.Courses.Include(c => c.Author).ToList();
            //context.Courses.Include(c => c.Author)
            //    .Include(c => c.Tags.Select(t => t.Name))
            //    .Include(c=>c.Cover);
            foreach (var course in eagerLoading) { Console.WriteLine("Name - Author: ", course.Name, course.Author); }

            var explicitLoading1 = context.Authors.Single(a => a.Id == 1);
            context.Courses.Where(c => c.AuthorId == explicitLoading1.Id)
                .Load();

            var explicitLoading2 = context.Authors.ToList();
            var authorsId = explicitLoading2.Select(a => a.Id);
            context.Courses.Where(c => authorsId.Contains(c.Id) && c.Level == CourseLevel.Beginner)
                .Load();

            //Ext method
            var tags = context.Courses
                .Where(c => c.Name.Contains("C#"))
                .OrderByDescending(c => c.Name)
                .ThenByDescending(c => c.Level)
                .SelectMany(c => c.Tags)
                .Distinct();
            foreach (var t in tags)
                Console.WriteLine(t.Name);

            var ext_group = context.Courses.GroupBy(c => c.Level);

            var ext_join = context.Courses.Join(context.Authors,
                                c => c.AuthorId,
                                a => a.Id,
                                (course, author) => new
                                { CourseName = course.Name, AuthorName = author.Name });
            var ext_groupJoin = context.Authors.GroupJoin(context.Courses,
                                    a => a.Id,
                                    c => c.AuthorId,
                                    (author, course) => new
                                    { Author = author, Courses = course.Count() });

            var ext_partition = context.Courses.Skip(10).Take(10);
            var ext_element = context.Courses.OrderBy(c => c.Level)
                                            .FirstOrDefault(c => c.AuthorId == 1);
            var ext_quatify = context.Courses.All(c => c.Level != CourseLevel.Beginner);
            //context.Courses.Last()   .Single()   .Any() .Count() .Min() .Max()  .Average()  .Sum()

            //LINQ syntax
            var query = from c in context.Courses
                        where c.Name.Contains("C#")
                        select c;
            foreach (var course in query)
                Console.WriteLine(course.Name);

            var query_test = from c in context.Courses
                             where c.Level == CourseLevel.Beginner
                             orderby c.Name, c.Level descending
                             select new { Course = c.Name, AuthorName = c.Author.Name };
            var query_group = from c in context.Courses
                              group c by c.Level into g
                              select new { Level = g.Key, Courses = g };
            var query_join = from c in context.Courses
                             join a in context.Authors on c.AuthorId equals a.Id
                             select new { CourseName = c.Name, AuthorName = a.Name };
            var query_groupJoin = from a in context.Authors
                                  join c in context.Courses on a.Id equals c.AuthorId into g
                                  select new { Author = a.Name, Courses = g.Count() };


            //=====*=====*=====
            var videoEncoder = new VideoEncoder();
            var video1 = new Video() { Title = "video 1" };
            //Interface
            videoEncoder.AddChannel(new MailChannel());
            videoEncoder.AddChannel(new FileChannel(@"D:\8_repo\_temp\cs_fdn\log.txt"));
            videoEncoder.VideoPlaying(video1);

            //EventHandler
            var textService = new TextService();
            var mailService = new MailSerice();
            videoEncoder.VideoEncodedEH += textService.OnVideoEncoded;
            videoEncoder.VideoEncodedEH += mailService.OnVideoEncoded;

            videoEncoder.Encode(video1);

            //Delegate
            var filter = new VideoFilters();
            VideoEncoder.VideoFilterHandler filterHandler = new VideoFilters().ApplyBrightness;
            filterHandler += filter.ApplyContrast;
            videoEncoder.Filter(video1, filterHandler);

            //Action<>
            Action<Video> filterAction = filter.ApplyBrightness;
            filterAction += filter.ApplyContrast;
            videoEncoder.FilterAction(video1, filterAction);
        }
    }

}
