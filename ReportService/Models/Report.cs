using System.ComponentModel.DataAnnotations;

namespace ReportService.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string CourseName { get; set; } = null!;
        public float TotalPayment { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }
}
