using System;
using FluentMigrator;
using demo.DemoApi.DAL.Migrations.Metadata;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019112701)]
    public class AddInbox : Migration
    {
        public static Guid InboxId112 = new Guid("d3ee131f-f6b4-4fb7-9205-000000000112");
        public static Guid InboxIdConnection = new Guid("d3ee131f-f6b4-4fb7-9205-000000000100");
        public static Guid InboxIdResource = new Guid("d3ee131f-f6b4-4fb7-9205-000000000101");

        public override void Up()
        {
            Create.Table("inbox")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("name").AsString().NotNullable();

            Create.Column("inbox_id")
                .OnTable("call_queue")
                .AsGuid().Nullable();

            Create.Table("inbox_user")
                .WithColumn("inbox_id").AsGuid().NotNullable()
                .WithColumn("user_id").AsGuid().NotNullable();

            Insert.IntoTable("inbox")
                .Row(new { id = InboxId112, name = "Служба 112" });
            Insert.IntoTable("inbox")
                .Row(new { id = InboxIdConnection, name = "Подключение" });
            Insert.IntoTable("inbox")
                .Row(new { id = InboxIdResource, name = "Вызов от РТП" });

            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdOperator112, extension = UsersMetadata.ExtensionOperator112, user_name = "Оператор 112", case_type_id = AddCaseTableAndCaseTemplateTable.CaseTypeId112 });
            Insert.IntoTable("operator").Row(new { id = UsersMetadata.UserIdFireDepartment3, extension = UsersMetadata.ExtensionFireDepartment3, user_name = "Сотрудник ПЧ", case_type_id = AddUserWithNewCaseType.FireDepartmentCaseTypeId });

            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxId112, user_id = UsersMetadata.UserIdAidarK });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxId112, user_id = UsersMetadata.UserIdIgorM });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxId112, user_id = UsersMetadata.UserIdNickD });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxId112, user_id = UsersMetadata.UserIdNickL });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxId112, user_id = UsersMetadata.UserIdPavelG });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxId112, user_id = UsersMetadata.UserIdSergM });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxId112, user_id = UsersMetadata.UserIdTanyaS });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxId112, user_id = UsersMetadata.UserIdVitalyM });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxId112, user_id = UsersMetadata.UserIdVladimirB });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxId112, user_id = UsersMetadata.UserIdVladimirV });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxId112, user_id = UsersMetadata.UserIdOperator112 });

            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdAidarK });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdIgorM });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdNickD });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdNickL });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdPavelG });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdSergM });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdTanyaS });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdVitalyM });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdVladimirB });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdVladimirV });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdOperator112 });

            // Пожарные
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdFireDepartment1 });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdResource, user_id = UsersMetadata.UserIdFireDepartment1 });

            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdFireDepartment2 });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdResource, user_id = UsersMetadata.UserIdFireDepartment2 });

            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdConnection, user_id = UsersMetadata.UserIdFireDepartment3 });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdResource, user_id = UsersMetadata.UserIdFireDepartment3 });
        }

        public override void Down()
        {
            Delete.Table("inbox_user");

            Delete.Column("inbox_id")
                .FromTable("call_queue");

            Delete.Table("inbox");
        }
    }
}
