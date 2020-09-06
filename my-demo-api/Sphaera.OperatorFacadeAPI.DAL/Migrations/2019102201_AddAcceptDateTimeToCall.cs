using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019102201)]
    public class AddAcceptDateTimeToCall : Migration
    {
        public override void Up()
        {
            Alter.Table("call_queue")
                .AddColumn("accept_datetime").AsDateTime()
                .Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("accept_datetime")
                .FromTable("call_queue");
        }
    }
}
