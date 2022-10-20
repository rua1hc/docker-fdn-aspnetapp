
namespace Course_service.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime EnrolledDate { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }

    }
}
