using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    public class UpdateCaseCssMigration : Migration
    {
        public override void Up()
        {
            var cssForCaseTemplate = ResourceManager.GetResource("demo.DemoApi.DAL.Resources.caseTemplate.scss");

            Update.Table("case_type")
                .Set(new { css = cssForCaseTemplate })
                .AllRows();
        }

        public override void Down()
        {
        }
    }
}
