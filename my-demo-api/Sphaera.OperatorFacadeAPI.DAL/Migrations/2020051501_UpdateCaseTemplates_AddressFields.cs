using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020051501)]
    public class UpdateCaseTemplates_AddressFields : UpdateCaseTemplatesMigration
    {
        public override void Up()
        {
            base.Up();
            var css = ResourceManager.GetResource("demo.DemoApi.DAL.Resources.caseTemplate.scss");

            Update.Table("case_type").Set(new { css }).AllRows();
        }
    }
}
