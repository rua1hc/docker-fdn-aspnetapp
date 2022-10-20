namespace Course_service.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public float Price { get; set; }
        public string? Description { get; set; }
        public ICollection<Enrollment>? Enrollments { get; set; }
    }
}
