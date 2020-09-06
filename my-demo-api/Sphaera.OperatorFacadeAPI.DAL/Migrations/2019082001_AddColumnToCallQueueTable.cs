using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019082001)]
    public class AddColumnDateTimeStart_CallQueueTable : Migration
    {
        public override void Up()
        {
            Alter.Table("call_queue").AddColumn("arrival_datetime").AsDateTime().Nullable();
        }

        public override void Down()
        {
            Delete.Column("arrival_datetime").FromTable("call_queue");
        }
    }
}
