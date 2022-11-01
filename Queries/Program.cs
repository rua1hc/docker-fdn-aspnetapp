using Queries.Persistence;
using System;

namespace Queries
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var unitOfWork = new UnitOfWork(new PlutoContext()))
            {
                var course = unitOfWork.Courses.Get(1);
                Console.WriteLine("[1]" + course.Name);

                var courses = unitOfWork.Courses.GetCoursesWithAuthors(1, 9);
                foreach (var c in courses)
                {
                    Console.WriteLine("[2]" + c.Name);
                }

                var author = unitOfWork.Authors.GetAuthorWithCourses(1);
                //unitOfWork.Courses.RemoveRange(author.Courses);
                //unitOfWork.Authors.Remove(author);
                foreach (var c in author.Courses)
                {
                    Console.WriteLine("[3]" + c.Name);
                }

                unitOfWork.Complete();
            }
        }
    }
}
  