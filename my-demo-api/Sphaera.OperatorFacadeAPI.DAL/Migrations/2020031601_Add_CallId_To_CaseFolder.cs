using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020031601)]
    public class Add_CallId_To_CaseFolder: Migration
    {
        public override void Up()
        {
            Create.Column("call_id").OnTable("case_folder").AsGuid().Nullable();
        }

        public override void Down()
        {
            Delete.Column("call_id").FromTable("case_folder");
        }
    }
}