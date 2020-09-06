using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020070201)]
    public class CreateTableWithCallLock : Migration
    {
        public override void Up()
        {
            Create.Table("call_lock")
                .WithColumn("id").AsGuid().PrimaryKey();
        }

        public override void Down()
        {
            Delete.Table("call_lock");
        }
    }
}
