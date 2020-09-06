using System;
using FluentMigrator;
using Newtonsoft.Json;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020042902)]
    public class UpdateOperatorTable : Migration
    {
        public override void Up()
        {
            var nameRu = Enum.GetName(typeof(LanguageCode), LanguageCode.RU);
            var defaultSetting = new { defaultLanguage = nameRu, currentLanguage = string.Empty };
            Create.Column("language_settings").OnTable("operator").AsString().Nullable().WithDefaultValue(JsonConvert.SerializeObject(defaultSetting));
        }

        public override void Down()
        {
            Delete.Column("language_settings").FromTable("operator");
        }
    }
}