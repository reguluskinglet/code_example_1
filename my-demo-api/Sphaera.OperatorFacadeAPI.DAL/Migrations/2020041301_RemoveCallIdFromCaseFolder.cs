using System;
using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020041301)]
    public class RemoveCallIdFromCaseFolder : Migration
    {
        public override void Up()
        {
            Rename
                .Column("call_id")
                .OnTable("case_folder").To("sms_id");
        }

        public override void Down()
        {

        }
    }
}