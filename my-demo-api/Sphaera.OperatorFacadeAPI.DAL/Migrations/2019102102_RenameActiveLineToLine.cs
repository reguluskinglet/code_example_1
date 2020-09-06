using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019102102)]
    public class RenameActiveLineToLine: Migration
    {
        public override void Up()
        {
            Rename
                .Table("active_line")
                .To("line");
            
            Rename
                .Column("active_line_id")
                .OnTable("call_queue")
                .To("line_id");
        }

        public override void Down()
        {
            Rename
                .Column("line_id")
                .OnTable("call_queue")
                .To("active_line_id");

            Rename
                .Table("line")
                .To("active_line");
        }
    }
}
