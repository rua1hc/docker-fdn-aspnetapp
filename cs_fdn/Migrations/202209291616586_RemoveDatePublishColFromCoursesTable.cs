namespace cs_fdn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveDatePublishColFromCoursesTable : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Courses", "DatePublish");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Courses", "DatePublish", c => c.DateTime());
        }
    }
}
