using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020010301)]
    public class ChangeInboxSettingsFor101 : Migration
    {
        public override void Up()
        {
            Update.Table("inbox")
                .Set(new { create_case_card = false })
                .Where(new { id = AddInbox.InboxIdResource });
        }

        public override void Down()
        {
        }
    }
}
