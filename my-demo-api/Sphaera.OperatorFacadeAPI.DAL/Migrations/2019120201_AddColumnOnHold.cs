using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019120201)]
    public class AddColumnOnHold : Migration
    {
        
        public override void Up()
        {
            Alter.Table("call_queue")
                .AddColumn("on_hold")
                .AsBoolean()
                .WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete
                .Column("on_hold")
                .FromTable("call_queue");
        }
    }
}
