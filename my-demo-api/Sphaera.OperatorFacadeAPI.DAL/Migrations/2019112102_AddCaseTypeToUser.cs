using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019112102)]
    public class AddCaseTypeToUser : Migration
    {
        public override void Up()
        {
            Alter
                .Table("operator")
                .AddColumn("case_type_id").AsGuid().Nullable();

            Update
                .Table("operator")
                .Set(new { case_type_id = AddCaseTableAndCaseTemplateTable.CaseTypeId112 })
                .AllRows();
        }

        public override void Down()
        {
            Delete
                .Column("case_type_id")
                .FromTable("operator");
        }
    }
}
