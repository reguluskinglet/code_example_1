using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020031801)]
    public class Add_Status_ToCase : Migration
    {
        public override void Up()
        {
            Create.Column("status").OnTable("case_card").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("status").FromTable("case_card");
        }
    }
}