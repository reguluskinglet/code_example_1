using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020051201)]
    public class DeleteInboxTable : Migration
    {
        public override void Up()
        {
            Delete.Table("inbox");
            Delete.Table("inbox_user");
        }

        public override void Down()
        {
        }
    }
}
