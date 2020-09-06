using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019112502)]
    public class MoveCaseDataToCaseFolder: Migration
    {
        public override void Up()
        {
            Delete.Column("data").FromTable("case_card");

            Create.Column("data").OnTable("case_folder").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Column("data").FromTable("case_folder");

            Create.Column("data").OnTable("case_card");
        }
    }
}
