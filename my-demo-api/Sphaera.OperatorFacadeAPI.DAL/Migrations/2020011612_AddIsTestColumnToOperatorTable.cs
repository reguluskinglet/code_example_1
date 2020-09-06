using System;
using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020011612)]
    public class AddIsTestColumnToOperatorTable : Migration
    {
        public override void Up()
        {
            Create.Column("is_test").OnTable("operator").AsBoolean().WithDefaultValue(false);
            CreateTestOperatorsList();
        }

        public override void Down()
        {
            Delete.Column("is_test").FromTable("operator");
        }

        private void CreateTestOperatorsList()
        {
            for (var i = 8000; i <= 9000; i++)
            {
                var userId = Guid.NewGuid();
                Insert.IntoTable("operator").Row(new
                {
                    id = userId,
                    extension = i.ToString(), 
                    user_name = $"Тестовый оператор {i}",
                    case_type_id = AddCaseTableAndCaseTemplateTable.CaseTypeId112,
                    is_test = true
                });
                
                Insert.IntoTable("inbox_user")
                    .Row(new { inbox_id = AddInbox.InboxId112, user_id = userId });
            }
        }
    }
}
