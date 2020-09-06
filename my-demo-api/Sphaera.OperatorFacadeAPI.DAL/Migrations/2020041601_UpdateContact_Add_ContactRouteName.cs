using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020041601)]
    public class UpdateContact_Add_ContactRouteName : Migration
    {
        public override void Up()
        {
            Alter.Table("contact")
                .AddColumn("contact_route_name").AsString().Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("contact_route_name")
                .FromTable("contact");
        }
    }
}
