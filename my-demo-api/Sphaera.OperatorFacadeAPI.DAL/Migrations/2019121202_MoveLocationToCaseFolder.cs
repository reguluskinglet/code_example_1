using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019121202)]
    public class MoveLocationToCaseFolder : Migration
    {
        public override void Up()
        {
            Delete.Column("latitude")
                .FromTable("case_card");

            Delete.Column("longitude")
                .FromTable("case_card");

            Alter.Table("case_folder")
                .AddColumn("latitude")
                .AsDouble()
                .Nullable();
            Alter.Table("case_folder")
                .AddColumn("longitude")
                .AsDouble()
                .Nullable();
        }

        public override void Down()
        {
            Delete.Column("latitude")
                .FromTable("case_folder");

            Delete.Column("longitude")
                .FromTable("case_folder");

            Alter.Table("case_card")
                .AddColumn("latitude")
                .AsFloat()
                .Nullable();
            Alter.Table("case_card")
                .AddColumn("longitude")
                .AsFloat()
                .Nullable();
        }
    }
}
