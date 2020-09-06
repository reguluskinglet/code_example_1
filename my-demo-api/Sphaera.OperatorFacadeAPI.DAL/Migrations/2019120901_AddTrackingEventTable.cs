using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019120901)]
    public class AddTrackingEventTable : Migration
    {
        public override void Up()
        {
            Create.Table("tracking_event")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("version").AsString()
                .WithColumn("transaction_id").AsGuid()
                .WithColumn("aggregate_id").AsGuid()
                .WithColumn("entity_id").AsGuid()
                .WithColumn("transaction_date").AsDateTime()
                .WithColumn("entity_name").AsString()
                .WithColumn("operation_type").AsInt32()
                .WithColumn("data").AsString();
        }

        public override void Down()
        {
            Delete.Table("tracking_event");
        }
    }
}