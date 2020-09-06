using FluentMigrator;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020052101)]
    public class Add_DisplayDirection_ToLanguage : Migration
    {
        public override void Up()
        {
            Create.Column("display_direction")
                .OnTable("language")
                .AsString().NotNullable()
                .WithDefaultValue(DisplayDirectionType.Ltr);
        }

        public override void Down()
        {
            Delete.Column("display_direction").FromTable("language");
        }
    }
}