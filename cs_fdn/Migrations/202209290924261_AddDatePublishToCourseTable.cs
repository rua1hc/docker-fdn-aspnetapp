namespace cs_fdn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDatePublishToCourseTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Courses", "DatePublish", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Courses", "DatePublish");
        }
    }
}
