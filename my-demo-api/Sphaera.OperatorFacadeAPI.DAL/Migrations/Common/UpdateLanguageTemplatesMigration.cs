using FluentMigrator;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.DAL.Migrations
{
    public class UpdateLanguageTemplatesMigration : Migration
    {
        public override void Up()
        {
            var languageTemplateEN = ResourceManager.GetResource("demo.DemoApi.DAL.Resources.languageTemplateEN.json");
            var languageTemplateRU = ResourceManager.GetResource("demo.DemoApi.DAL.Resources.languageTemplateRU.json");
            var languageTemplateAR = ResourceManager.GetResource("demo.DemoApi.DAL.Resources.languageTemplateAR.json");

            Update.Table("language")
                .Set(new { data = languageTemplateRU })
                .Where(new { code = LanguageCode.RU });

            Update.Table("language")
                .Set(new { data = languageTemplateEN })
                .Where(new { code = LanguageCode.EN });

            Update.Table("language")
                .Set(new { data = languageTemplateAR })
                .Where(new { code = LanguageCode.AR });
        }

        public override void Down()
        {
        }
    }
}
