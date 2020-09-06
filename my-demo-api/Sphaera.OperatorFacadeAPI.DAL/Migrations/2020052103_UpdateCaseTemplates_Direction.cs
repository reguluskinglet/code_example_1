using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020052103)]
    public class UpdateCaseTemplates_Direction : UpdateCaseTemplatesMigration
    {
        public override void Up()
        {
            base.Up();
            var css = ResourceManager.GetResource("demo.DemoApi.DAL.Resources.caseTemplate.scss");

            Update.Table("case_type").Set(new { css }).AllRows();
        }
    }
}
