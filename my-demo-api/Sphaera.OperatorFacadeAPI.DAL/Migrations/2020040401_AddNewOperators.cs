using FluentMigrator;
using demo.DemoApi.DAL.Migrations.Metadata;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020040401)]
    public class AddNewOperators : Migration
    {
        public override void Up()
        {
            const string UserPreferences = "{\"contactsWindow\":{\"minSize\":{\"width\":400,\"height\":600},\"maxSize\":{\"width\":1200}}}";

            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdAnastasiaB, extension = UsersMetadata.ExtensionAnastasiaB, user_name = "Анастасия Брахнова", preferences = UserPreferences, case_type_id = AddCaseTableAndCaseTemplateTable.CaseTypeId112 });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdDmitriyG, extension = UsersMetadata.ExtensionDmitriyG, user_name = "Дмитрий Гаврилов", preferences = UserPreferences, case_type_id = AddCaseTableAndCaseTemplateTable.CaseTypeId112 });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdDenisS, extension = UsersMetadata.ExtensionDenisS, user_name = "Денис Савлевич", preferences = UserPreferences, case_type_id = AddCaseTableAndCaseTemplateTable.CaseTypeId112 });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdAlexanderZ, extension = UsersMetadata.ExtensionAlexanderZ, user_name = "Александр Журавлев", preferences = UserPreferences, case_type_id = AddCaseTableAndCaseTemplateTable.CaseTypeId112 });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdAlexanderI, extension = UsersMetadata.ExtensionAlexanderI, user_name = "Александр Имайкин", preferences = UserPreferences, case_type_id = AddCaseTableAndCaseTemplateTable.CaseTypeId112 });


            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddInbox.InboxId112, user_id = UsersMetadata.UserIdAnastasiaB });
            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddInbox.InboxIdConnection, user_id = UsersMetadata.UserIdAnastasiaB });
            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddSmsQueue.InboxIdSms, user_id = UsersMetadata.UserIdAnastasiaB });

            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddInbox.InboxId112, user_id = UsersMetadata.UserIdDmitriyG });
            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddInbox.InboxIdConnection, user_id = UsersMetadata.UserIdDmitriyG });
            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddSmsQueue.InboxIdSms, user_id = UsersMetadata.UserIdDmitriyG });

            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddInbox.InboxId112, user_id = UsersMetadata.UserIdDenisS });
            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddInbox.InboxIdConnection, user_id = UsersMetadata.UserIdDenisS });
            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddSmsQueue.InboxIdSms, user_id = UsersMetadata.UserIdDenisS });

            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddInbox.InboxId112, user_id = UsersMetadata.UserIdAlexanderZ });
            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddInbox.InboxIdConnection, user_id = UsersMetadata.UserIdAlexanderZ });
            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddSmsQueue.InboxIdSms, user_id = UsersMetadata.UserIdAlexanderZ });

            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddInbox.InboxId112, user_id = UsersMetadata.UserIdAlexanderI });
            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddInbox.InboxIdConnection, user_id = UsersMetadata.UserIdAlexanderI });
            Insert.IntoTable("inbox_user").Row(new { inbox_id = AddSmsQueue.InboxIdSms, user_id = UsersMetadata.UserIdAlexanderI });
        }

        public override void Down()
        {
            Delete.FromTable("inbox_user").Row(new { user_id = UsersMetadata.UserIdAnastasiaB });
            Delete.FromTable("operator").Row(new { id = UsersMetadata.UserIdAnastasiaB });

            Delete.FromTable("inbox_user").Row(new { user_id = UsersMetadata.UserIdDmitriyG });
            Delete.FromTable("operator").Row(new { id = UsersMetadata.UserIdDmitriyG });

            Delete.FromTable("inbox_user").Row(new { user_id = UsersMetadata.UserIdDenisS });
            Delete.FromTable("operator").Row(new { id = UsersMetadata.UserIdDenisS });

            Delete.FromTable("inbox_user").Row(new { user_id = UsersMetadata.UserIdAlexanderZ });
            Delete.FromTable("operator").Row(new { id = UsersMetadata.UserIdAlexanderZ });

            Delete.FromTable("inbox_user").Row(new { user_id = UsersMetadata.UserIdAlexanderI });
            Delete.FromTable("operator").Row(new { id = UsersMetadata.UserIdAlexanderI });
        }
    }
}
