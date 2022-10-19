namespace WebApp1.Data
{
    public static class DefaultClaimConfig
    {
        public static readonly Dictionary<string, string> Permission = new Dictionary<string, string>
        {
            { "Admin", "Admin.Manage.Full" },
            { "Staff", "Staff.Assigment.Class" },
            { "Member", "Member.Enrollment.Course" }
        };
    }
    public class ApplicationClaimTypes
    {
        //public static List<String> AppClaimTypes = new List<String>()
        //{
        //    "Full",
        //    "Admin.Manage.ObjectIndex",
        //    "Staff.Assigment.Class",
        //    "Member.Enrollment.Course",
        //};
    }
}
