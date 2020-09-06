using FluentMigrator;
using demo.DemoApi.DAL.Migrations.Metadata;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019082801)]
    public class AddOperatorTable : Migration
    {
        /// <inheritdoc />
        public override void Up()
        {
            Create.Table("operator")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("extension").AsString()
                .WithColumn("user_name").AsString().Nullable()
                .WithColumn("sip_uri").AsString().Nullable()
                .WithColumn("preferences").AsString().Nullable()
                .WithColumn("is_active").AsBoolean().WithDefaultValue(false);

            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdNickD, extension = UsersMetadata.ExtensionNickD, user_name = "Никита Домбровский" });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdNickL, extension = UsersMetadata.ExtensionNickL, user_name = "Никита Ляпин" });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdPavelG, extension = UsersMetadata.ExtensionPavelG, user_name = "Павел Горбенко" });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdAidarK, extension = UsersMetadata.ExtensionAidarK, user_name = "Айдар Камалов" });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdSergM, extension = UsersMetadata.ExtensionSergM, user_name = "Сергей Мартишин" });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdTanyaS, extension = UsersMetadata.ExtensionTanyaS, user_name = "Татьяна Сахарова" });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdVladimirB, extension = UsersMetadata.ExtensionVladimirB, user_name = "Владимир Богаченко" });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdIgorM, extension = UsersMetadata.ExtensionIgorM, user_name = "Игорь Медюха" });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdVitalyM, extension = UsersMetadata.ExtensionVitalyM, user_name = "Виталий Медюха" });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdVladimirV, extension = UsersMetadata.ExtensionVladimirV, user_name = "Владимир Варзов" });
        }

        /// <inheritdoc />
        public override void Down()
        {
            Delete.Table("operator");
        }
    }
}
