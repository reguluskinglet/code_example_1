using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019110701)]
    public class AddIsolatedStatus_CallQueueTable : Migration
    {
        public override void Up()
        {
            Alter.Table("call_queue").AddColumn("isolated").AsBoolean().Nullable();
        }

        public override void Down()
        {
            Delete.Column("isolated").FromTable("call_queue");
        }
    }
}
