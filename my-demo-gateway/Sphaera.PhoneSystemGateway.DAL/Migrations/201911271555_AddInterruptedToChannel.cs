using FluentMigrator;

namespace demo.DemoGateway.DAL.Migrations
{
    [Migration(201911271555)]
    public class AddInterruptedToChannel : Migration
    {
        public override void Up()
        {
            Create.Column("interrupted").OnTable("channel").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.Column("interrupted").FromTable("channel");
        }
    }
}
