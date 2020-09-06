using FluentMigrator;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020052102)]
    public class Update_DisplayDirectionForLanguages : Migration
    {
        public override void Up()
        {
            Update.Table("language")
                .Set(new { display_direction = DisplayDirectionType.Rtl })
                .Where(new { code = LanguageCode.AR });
        }

        public override void Down()
        {
        }
    }
}