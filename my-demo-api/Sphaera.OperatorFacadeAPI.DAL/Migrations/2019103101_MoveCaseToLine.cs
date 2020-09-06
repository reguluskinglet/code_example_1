using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019103101)]
    public class MoveCaseToLine: Migration
    {
        public override void Up()
        {
            Alter
                .Table("line")
                .AddColumn("case_card_id")
                .AsGuid()
                .Nullable();

            Delete
                .Column("case_card_id")
                .FromTable("call_queue");


        }

        public override void Down()
        {

            Delete
                .Column("case_card_id")
                .FromTable("line");
            
            Alter.Table("call_queue")
                .AddColumn("case_card_id")
                .AsGuid()
                .Nullable();

        }
    }
}
