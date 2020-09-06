using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020033102)]
    public class Add_CallerId_ToCallQueue : Migration
    {
        public override void Up()
        {
            Create.Column("caller_id").OnTable("call_queue").AsGuid().Nullable();
        }

        public override void Down()
        {
            Delete.Column("caller_id").FromTable("call_queue");
        }
    }
}