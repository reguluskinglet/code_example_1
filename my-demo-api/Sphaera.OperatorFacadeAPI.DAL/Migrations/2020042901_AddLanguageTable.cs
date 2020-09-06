using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020042901)]
    public class AddLanguageTableAndLanguageTemplate : Migration
    {
        public override void Up()
        {
            Create.Table("language")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("code").AsString()
                .WithColumn("name").AsString().Nullable()
                .WithColumn("data").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Table("language");
        }
    }
}