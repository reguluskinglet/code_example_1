using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019111101)]
    public class ChangeMutePlace_FromOperatorToCall : Migration
    {
        public override void Up()
        {
            Delete
                .Column("is_muted")
                .FromTable("operator");
            Alter.Table("call_queue")
                .AddColumn("is_muted")
                .AsBoolean()
                .WithDefaultValue(false);
        }

        public override void Down()
        {
            Alter.Table("operator")
                .AddColumn("is_muted")
                .AsBoolean()
                .WithDefaultValue(false);
            Delete
                .Column("is_muted")
                .FromTable("call_queue");
        }
    }
}
