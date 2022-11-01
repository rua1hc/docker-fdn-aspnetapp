namespace cs_fdn.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RenameTitleNameInCourseTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Courses", "Name", c => c.String(nullable: false));
            Sql("UPDATE Courses SET Name=Title");
            DropColumn("dbo.Courses", "Title");

            //RenameColumn("Courses", "Title", "Name");
        }

        public override void Down()
        {
            AddColumn("dbo.Courses", "Title", c => c.String(nullable: false));
            Sql("UPDATE Courses SET Title=Name");
            DropColumn("dbo.Courses", "Name");

            //RenameColumn("Courses", "Name", "Title");
        }
    }
}
