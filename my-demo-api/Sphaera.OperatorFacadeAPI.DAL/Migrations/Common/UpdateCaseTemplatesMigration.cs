using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    public class UpdateCaseTemplatesMigration : Migration
    {
        public override void Up()
        {
            var caseTemplateFireDepartment = ResourceManager.GetResource("demo.DemoApi.DAL.Resources.caseTemplate_FireDepartment.json");
            var caseTemplate112 = ResourceManager.GetResource("demo.DemoApi.DAL.Resources.caseTemplate_112.json");

            Update.Table("case_type")
                .Set(new { data = caseTemplate112 })
                .Where(new { id = AddCaseTableAndCaseTemplateTable.CaseTypeId112 });

            Update.Table("case_type")
                .Set(new { data = caseTemplateFireDepartment })
                .Where(new { id = AddUserWithNewCaseType.FireDepartmentCaseTypeId });
        }

        public override void Down()
        {
        }
    }
}
