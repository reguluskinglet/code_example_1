using System;
using FluentMigrator;
using demo.DemoApi.DAL.Migrations.Metadata;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019112501)]
    public class AddUserWithNewCaseType : Migration
    {
        public static string FireDepartmentCaseTypeId => "99c543c5-0dd7-4ebc-a87d-000000000002";

        public override void Up()
        {
            var caseTemplate = ResourceManager.GetResource("demo.DemoApi.DAL.Resources.caseTemplate_FireDepartment.json");
            var caseTestTemplate = ResourceManager.GetResource("demo.DemoApi.DAL.Resources.caseTemplate_112.json");
            Insert.IntoTable("case_type").Row(new { id = FireDepartmentCaseTypeId, data = caseTemplate });

            Insert
                .IntoTable("operator")
                .Row(new
                {
                    id = UsersMetadata.UserIdFireDepartment1,
                    extension = UsersMetadata.ExtensionFireDepartment1,
                    user_name = "Господин Пожарный 1",
                    case_type_id = FireDepartmentCaseTypeId
                });

            Insert
                .IntoTable("operator")
                .Row(new
                {
                    id = UsersMetadata.UserIdFireDepartment2,
                    extension = UsersMetadata.ExtensionFireDepartment2,
                    user_name = "Господин Пожарный 2",
                    case_type_id = FireDepartmentCaseTypeId
                });
            Update.Table("case_type").Set(new {data = caseTestTemplate}).Where(new {id = "6a9f90c4-2b7e-4ec3-a38f-000000000112"});
        }

        public override void Down()
        {
            Delete.FromTable("operator").Row(new { id = UsersMetadata.UserIdFireDepartment1 });
            Delete.FromTable("operator").Row(new { id = UsersMetadata.UserIdFireDepartment2 });

            Delete.FromTable("case_type").Row(new { id = new Guid(FireDepartmentCaseTypeId) });
        }
    }
}
