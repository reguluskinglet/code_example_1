using System;
using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019103001)]
    public class RemoveTransactioIdFromCall : Migration
    {
        public override void Up()
        {
            Delete
                .Column("transaction_id")
                .FromTable("call_queue");
        }

        public override void Down()
        {
            Alter.Table("call_queue")
                .AddColumn("transaction_id").AsString().WithDefaultValue(String.Empty);
        }
    }
}