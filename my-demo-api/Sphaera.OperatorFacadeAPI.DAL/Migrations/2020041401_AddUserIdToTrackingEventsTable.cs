using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020041401)]
    public class AddUserIdToTrackingEventsTable : Migration
    {
        public override void Up()
        {
            Create.Column("user_id").OnTable("tracking_event").AsGuid().Nullable();
        }

        public override void Down()
        {
            Delete.Column("user_id").FromTable("tracking_event");
        }
    }
}