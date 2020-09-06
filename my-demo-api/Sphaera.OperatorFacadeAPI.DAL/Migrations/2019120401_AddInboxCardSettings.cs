using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019120401)]
    public class AddInboxCardSettings : Migration
    {
        public override void Up()
        {
            Alter.Table("inbox").AddColumn("create_case_card").AsBoolean().WithDefaultValue(false);
            Update.Table("inbox").Set(new { create_case_card = true }).Where(new { id = AddInbox.InboxId112 });
        }

        public override void Down()
        {
            Delete.Column("create_case_card").FromTable("inbox");
        }
    }
}