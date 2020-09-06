using System;
using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020050301)]
    public class AddOutgoingInbox : Migration
    {
        private static readonly Guid outgoingInboxId = new Guid("d3ee131f-f6b4-4fb7-9205-000000001000");

        public override void Up()
        {
            Create.Column("is_outgoing").OnTable("inbox").AsBoolean().NotNullable().WithDefaultValue(false);
            Insert.IntoTable("inbox")
                .Row(new
                {
                    id = outgoingInboxId,
                    name = "Исходящие вызовы",
                    order_number = 1000,
                    create_case_card = false,
                    is_outgoing = true
                });
        }

        public override void Down()
        {
            Delete.Column("is_outgoing").FromTable("inbox");
        }
    }
}
