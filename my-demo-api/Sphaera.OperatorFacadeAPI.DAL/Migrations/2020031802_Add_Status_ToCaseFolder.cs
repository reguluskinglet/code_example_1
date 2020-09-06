using FluentMigrator;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020031802)]
    public class Add_Status_ToCaseFolder : Migration
    {
        public override void Up()
        {
            Create.Column("status")
                .OnTable("case_folder")
                .AsString().NotNullable()
                .WithDefaultValue(CaseFolderStatus.Closed);
        }

        public override void Down()
        {
            Delete.Column("status").FromTable("case_folder");
        }
    }
}