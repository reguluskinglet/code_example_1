using FluentMigrator;
using demo.DemoApi.DAL.Migrations.Metadata;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019112801)]
    public class AddArtemOperator : Migration
    {
        public override void Up()
        {
            Insert.IntoTable("operator").Row(new
            {
                id = UsersMetadata.UserIdArtemL, 
                extension = UsersMetadata.ExtensionArtemL, 
                user_name = "Артем Лагунков", 
                case_type_id = AddCaseTableAndCaseTemplateTable.CaseTypeId112
            });
            
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = AddInbox.InboxId112, user_id = UsersMetadata.UserIdArtemL });
            Insert.IntoTable("inbox_user")
                .Row(new { inbox_id = AddInbox.InboxIdConnection, user_id = UsersMetadata.UserIdArtemL });
        }

        public override void Down()
        {
            Delete.FromTable("inbox_user").Row(new { user_id = UsersMetadata.UserIdArtemL });
            Delete.FromTable("operator").Row(new { id = UsersMetadata.UserIdArtemL });
        }
    }
}
