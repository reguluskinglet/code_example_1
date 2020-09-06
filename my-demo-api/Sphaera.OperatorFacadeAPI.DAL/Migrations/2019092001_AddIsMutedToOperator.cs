using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019092001)]
    public class AddIsMutedToOperator : Migration
    {
        public override void Up()
        {
            Alter.Table("operator")
                .AddColumn("is_muted").AsBoolean().WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete
                .Column("is_muted")
                .FromTable("operator");
        }
    }
}