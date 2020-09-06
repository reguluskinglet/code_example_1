using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020033103)]
    public class UpdateSmsTable : Migration
    {
        public override void Up()
        {
            Create.Column("location_metadata").OnTable("sms").AsString().Nullable();
            Create.Column("timestamp").OnTable("sms").AsString().Nullable();
        }
        
        public override void Down()
        {
            Delete.Column("location_metadata").FromTable("sms");
            Delete.Column("timestamp").FromTable("sms");
        }
    }
}
