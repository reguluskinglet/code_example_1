using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019122001)]
    public class AddGroupCallIdToCall: Migration
    {
        public override void Up()
        {
            Create.Column("group_call_id").OnTable("call_queue").AsGuid().Nullable();
        }

        public override void Down()
        {
            Delete.Column("group_call_id").FromTable("call_queue");
        }
    }
}
