using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020072001)]
    public class RemoveSmsGarbage : Migration
    {
        /// <inheritdoc />
        public override void Up()
        {
            Delete.Table("sms_inbox_item");

            Delete
                .Column("inbox_id")
                .FromTable("sms");
        }

        /// <inheritdoc />
        public override void Down()
        {
        }
    }
}