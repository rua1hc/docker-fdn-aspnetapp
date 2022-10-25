
namespace Course_service.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime EnrolledDate { get; set; } = DateTime.Now;
        public int CourseId { get; set; }
        public Course? Course { get; set; }

    }


    //public static class CourseEndpoints
    //{
    //    public static void MapCourseEndpoints(this IEndpointRouteBuilder routes)
    //    {
    //        routes.MapGet("/api/Course", () =>
    //        {
    //            return new[] { new Course() };
    //        })
    //        .WithName("GetAllCourses");

    //        routes.MapGet("/api/Course/{id}", (int id) =>
    //        {
    //            //return new Course { ID = id };
    //        })
    //        .WithName("GetCourseById");

    //        routes.MapPut("/api/Course/{id}", (int id, Course input) =>
    //        {
    //            return Results.NoContent();
    //        })
    //        .WithName("UpdateCourse");

    //        routes.MapPost("/api/Course/", (Course model) =>
    //        {
    //            //return Results.Created($"/Courses/{model.ID}", model);
    //        })
    //        .WithName("CreateCourse");

    //        routes.MapDelete("/api/Course/{id}", (int id) =>
    //        {
    //            //return Results.Ok(new Course { ID = id });
    //        })
    //        .WithName("DeleteCourse");
    //    }
    //}
}
