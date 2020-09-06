using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019121201)]
    public class AddLocationCoordinates : Migration
    {
        public override void Up()
        {
            Alter.Table("case_card")
                .AddColumn("latitude")
                .AsFloat()
                .Nullable();
            Alter.Table("case_card")
                .AddColumn("longitude")
                .AsFloat()
                .Nullable();
        }

        public override void Down()
        {
            Delete.Column("latitude")
                .FromTable("case_card");

            Delete.Column("longitude")
                .FromTable("case_card");
        }
    }
}
