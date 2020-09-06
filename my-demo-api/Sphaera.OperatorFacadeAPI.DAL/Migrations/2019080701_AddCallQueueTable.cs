using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019080701)]
    public class AddCallQueueTable : Migration
    {
        public override void Up()
        {
            Create.Table("call_queue")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("transaction_id").AsString()
                .WithColumn("operator_id").AsGuid().Nullable()
                .WithColumn("operator_extension").AsString().Nullable()
                .WithColumn("operator_user_name").AsString().Nullable()
                .WithColumn("operator_sip_uri").AsString().Nullable()
                .WithColumn("applicant_id").AsString()
                .WithColumn("applicant_extension").AsString()
                .WithColumn("status").AsInt32()
                .WithColumn("cancelled").AsBoolean();
        }

        public override void Down()
        {
            Delete.Table("call_queue");
        }
    }
}
