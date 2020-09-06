using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020032501)]
    public class Add_IndexValue_To_Case : Migration
    {
        public override void Up()
        {
            Create.Column("index_value").OnTable("case_card").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("index_value").FromTable("case_card");
        }
    }
}
