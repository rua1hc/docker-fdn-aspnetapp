namespace SharedModels
{
    public record CourseEnrolled
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrolledDate { get; set; } = DateTime.Now;
    }
}