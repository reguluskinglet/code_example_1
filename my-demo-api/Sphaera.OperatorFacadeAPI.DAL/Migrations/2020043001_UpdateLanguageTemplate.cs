using System;
using FluentMigrator;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020043001)]
    public class UpdateLanguageTemplate : UpdateLanguageTemplatesMigration
    {
        public override void Up()
        {
            Insert.IntoTable("language").Row(new { id = Guid.NewGuid(), code = LanguageCode.EN, name = "English" });
            Insert.IntoTable("language").Row(new { id = Guid.NewGuid(), code = LanguageCode.RU, name = "Русский" });
            base.Up();
        }
    }
}
