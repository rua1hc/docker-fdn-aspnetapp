namespace SharedModels
{
    public record CourseEnrolled
    {
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public float CoursePrice { get; set; }
        public DateTime EnrolledDate { get; set; }
    }

}