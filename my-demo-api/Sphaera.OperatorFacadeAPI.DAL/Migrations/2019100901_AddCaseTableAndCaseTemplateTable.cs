using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019100901)]
    public class AddCaseTableAndCaseTemplateTable : Migration
    {
        public static string CaseTypeId112 => "6a9f90c4-2b7e-4ec3-a38f-000000000112";
        
        public override void Up()
        {
            Create.Table("case_card")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("created").AsDateTime()
                .WithColumn("updated").AsDateTime()
                .WithColumn("data").AsString().Nullable()
                .WithColumn("case_card_template_id").AsGuid();

            Alter.Table("call_queue")
                .AddColumn("case_card_id").AsGuid().Nullable();

            Create.Table("case_card_template")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("data").AsString();

            var caseTestTemplate = ResourceManager.GetResource("demo.DemoApi.DAL.Resources.caseTemplate_112.json");
            
            Insert
                .IntoTable("case_card_template")
                .Row(new { id = CaseTypeId112, data = caseTestTemplate });
        }

        public override void Down()
        {
            Delete.Table("case_card");

            Delete.Table("case_card_template");
        }
    }
}
