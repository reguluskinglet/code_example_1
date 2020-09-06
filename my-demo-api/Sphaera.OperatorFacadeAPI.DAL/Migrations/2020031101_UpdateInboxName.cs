using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020031101)]
    public class UpdateInboxName : Migration
    {
        public override void Up()
        {
            Update.Table("inbox")
                .Set(new { name = "Вызов от РТП" })
                .Where(new { id = AddInbox.InboxIdResource });
        }
        
        public override void Down()
        {
        }
    }
}
