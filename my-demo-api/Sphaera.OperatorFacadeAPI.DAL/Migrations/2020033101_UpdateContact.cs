using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020033101)]
    public class UpdateContact : Migration
    {
        public override void Up()
        {
            Alter.Table("contact")
                .AddColumn("name").AsString().Nullable()
                .AddColumn("position").AsString().Nullable()
                .AddColumn("organization").AsString().Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("name")
                .Column("position")
                .Column("organization")
                .FromTable("contact");
        }
    }
}
