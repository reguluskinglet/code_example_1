using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020041402)]
    // ReSharper disable once UnusedType.Global
    // ReSharper disable once InconsistentNaming
    public class Add_Css_To_CaseType : Migration
    {
        public override void Up()
        {
            var css = ResourceManager.GetResource("demo.DemoApi.DAL.Resources.caseTemplate.scss");

            Create.Column("css").OnTable("case_type").AsString().Nullable().WithDefaultValue(css);
        }

        public override void Down()
        {
            Delete.Column("css").FromTable("case_type");
        }
    }
}
