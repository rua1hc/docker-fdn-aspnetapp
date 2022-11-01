namespace cs_fdn.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAuthorToCourseTable : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.TagCourses", newName: "CoursesTags");
            DropIndex("dbo.Courses", new[] { "Author_Id" });
            RenameColumn(table: "dbo.Courses", name: "Author_Id", newName: "AuthorId");
            RenameColumn(table: "dbo.CoursesTags", name: "Tag_Id", newName: "TagId");
            RenameColumn(table: "dbo.CoursesTags", name: "Course_Id", newName: "CourseId");
            RenameIndex(table: "dbo.CoursesTags", name: "IX_Course_Id", newName: "IX_CourseId");
            RenameIndex(table: "dbo.CoursesTags", name: "IX_Tag_Id", newName: "IX_TagId");
            DropPrimaryKey("dbo.CoursesTags");
            AlterColumn("dbo.Courses", "Name", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.Courses", "AuthorId", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.CoursesTags", new[] { "CourseId", "TagId" });
            CreateIndex("dbo.Courses", "AuthorId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Courses", new[] { "AuthorId" });
            DropPrimaryKey("dbo.CoursesTags");
            AlterColumn("dbo.Courses", "AuthorId", c => c.Int());
            AlterColumn("dbo.Courses", "Name", c => c.String());
            AddPrimaryKey("dbo.CoursesTags", new[] { "Tag_Id", "Course_Id" });
            RenameIndex(table: "dbo.CoursesTags", name: "IX_TagId", newName: "IX_Tag_Id");
            RenameIndex(table: "dbo.CoursesTags", name: "IX_CourseId", newName: "IX_Course_Id");
            RenameColumn(table: "dbo.CoursesTags", name: "CourseId", newName: "Course_Id");
            RenameColumn(table: "dbo.CoursesTags", name: "TagId", newName: "Tag_Id");
            RenameColumn(table: "dbo.Courses", name: "AuthorId", newName: "Author_Id");
            CreateIndex("dbo.Courses", "Author_Id");
            RenameTable(name: "dbo.CoursesTags", newName: "TagCourses");
        }
    }
}
