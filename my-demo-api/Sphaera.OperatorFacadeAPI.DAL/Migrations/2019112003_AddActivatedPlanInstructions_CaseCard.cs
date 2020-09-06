using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019112003)]
    public class AddActivatedPlanInstructions_CaseCard : Migration
    {
        public override void Up()
        {
            Alter.Table("case_card")
                .AddColumn("activated_plan_instructions")
                .AsString()
                .Nullable();
        }

        public override void Down()
        {
            Delete.Column("activated_plan_instructions")
                .FromTable("case_card");
        }
    }
}
