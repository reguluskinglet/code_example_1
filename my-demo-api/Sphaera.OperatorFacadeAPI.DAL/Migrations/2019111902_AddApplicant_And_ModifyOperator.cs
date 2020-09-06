using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019111902)]
    // ReSharper disable once InconsistentNaming
    public class AddApplicant_And_ModifyOperator : Migration
    {
        public override void Up()
        {
            Delete.Column("applicant_id").FromTable("call_queue");
            Delete.Column("applicant_extension").FromTable("call_queue");

            Create.Table("applicant")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("extension").AsString().Nullable();
            
            Rename.Column("operator_id").OnTable("call_queue").To("participant_id");
        }

        public override void Down()
        {
            Create.Column("applicant_extension").OnTable("call_queue").AsString();
            Create.Column("applicant_id").OnTable("call_queue").AsString();
            
            Delete.Table("applicant");

            Rename.Column("participant_id").OnTable("call_queue").To("operator_id");
        }
    }
}
