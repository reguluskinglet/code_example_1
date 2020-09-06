using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019082901)]
    public class AddActiveLineTable : Migration
    {
        public override void Up()
        {
            Create.Table("active_line")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("caller_id").AsString()
                .WithColumn("create_datetime").AsDateTime();
        }

        public override void Down()
        {
            Delete.Table("active_line");
        }
    }
}
