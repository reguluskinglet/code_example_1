using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020031901)]
    public class AddContact : Migration
    {
        public override void Up()
        {
            Create.Table("contact")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("extension").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Table("contact");
        }
    }
}