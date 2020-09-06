using System;
using FluentMigrator;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020052001)]
    public class AddArabicLanguage : UpdateLanguageTemplatesMigration
    {
        public override void Up()
        {
            Insert.IntoTable("language").Row(new { id = Guid.NewGuid(), code = LanguageCode.AR, name = "عرب" });
            base.Up();
        }
    }
}
