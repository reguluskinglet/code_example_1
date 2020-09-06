using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019121301)]
    public class ChangeInboxCardSettings : Migration
    {
        public override void Up()
        {
            Update.Table("inbox").Set(new { create_case_card = true }).AllRows();
        }

        public override void Down()
        {
            Update.Table("inbox").Set(new { create_case_card = true }).Where(new { id = AddInbox.InboxId112 });
        }
    }
}