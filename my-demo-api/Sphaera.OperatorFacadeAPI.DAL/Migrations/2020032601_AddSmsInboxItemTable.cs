using FluentMigrator;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020032601)]
    public class AddSmsInboxItemTable : Migration
    {
        public override void Up()
        {
            Create.Table("sms_inbox_item")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("sms_id").AsGuid().Nullable();
        }
        
        public override void Down()
        {
            Delete.Table("sms_inbox_item");
        }
    }
}
