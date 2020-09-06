using System;
using FluentMigrator;
using demo.DemoApi.DAL.Migrations.Metadata;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020030601)]
    public class AddSmsQueue : Migration
    {
        /// <summary>
        /// id sms очереди
        /// </summary>
        public static Guid InboxIdSms = new Guid("d3ee131f-f6b4-4fb7-9205-000000000102");

        public override void Up()
        {
            Insert.IntoTable("inbox")
                .Row(new { id = InboxIdSms, name = "SMS", order_number = 2});
            
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdSms, user_id = UsersMetadata.UserIdAidarK });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdSms, user_id = UsersMetadata.UserIdIgorM });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdSms, user_id = UsersMetadata.UserIdNickD });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdSms, user_id = UsersMetadata.UserIdNickL });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdSms, user_id = UsersMetadata.UserIdPavelG });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdSms, user_id = UsersMetadata.UserIdSergM });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdSms, user_id = UsersMetadata.UserIdTanyaS });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdSms, user_id = UsersMetadata.UserIdVitalyM });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdSms, user_id = UsersMetadata.UserIdVladimirB });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdSms, user_id = UsersMetadata.UserIdVladimirV });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdSms, user_id = UsersMetadata.UserIdOperator112 });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = InboxIdSms, user_id = UsersMetadata.UserIdArtemL });
        }

        public override void Down()
        {
            Delete.FromTable("inbox").Row(new { inbox_id = InboxIdSms });
            
            Delete.FromTable("inbox_user").Row(new { inbox_id = InboxIdSms });
        }
    }
}
