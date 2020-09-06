using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019122403)]
    public class Add_SortingColumn_To_Inbox : Migration
    {
        public override void Up()
        {
            Create.Column("order_number").OnTable("inbox").AsInt32().WithDefaultValue(999);
            Update.Table("inbox").Set(new { order_number = 1, name = "Вызов 112" }).Where(new { id = AddInbox.InboxId112 });
            Update.Table("inbox").Set(new { order_number = 10 }).Where(new { id = AddInbox.InboxIdConnection });
            Update.Table("inbox").Set(new { order_number = 20, name = "Вызов от начкара" }).Where(new { id = AddInbox.InboxIdResource });
        }

        public override void Down()
        {
            Delete.Column("order_number").FromTable("inbox");
        }
    }
}
