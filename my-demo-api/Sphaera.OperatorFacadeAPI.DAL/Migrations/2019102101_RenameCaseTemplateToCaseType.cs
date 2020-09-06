using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019102101)]
    public class RenameCaseTemplateToCaseType : Migration
    {
        public override void Up()
        {
            Rename
                .Table("case_card_template")
                .To("case_type");

            Rename
                .Column("case_card_template_id")
                .OnTable("case_card")
                .To("case_type_id");
        }

        public override void Down()
        {
            Rename
                .Column("case_type_id")
                .OnTable("case_card")
                .To("case_card_template_id");
            
            Rename.Table("case_type").To("case_card_template");
        }
    }
}
