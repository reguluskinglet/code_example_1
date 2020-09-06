using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020031001)]
    public class AddSmsFieldsToCalls : Migration
    {
        public override void Up()
        {
            Create.Column("sms_text").OnTable("call_queue").AsString().Nullable();
            Create.Column("is_sms").OnTable("call_queue").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.Column("sms_text").FromTable("call_queue");
            Delete.Column("is_sms").FromTable("call_queue");
        }
    }
}
