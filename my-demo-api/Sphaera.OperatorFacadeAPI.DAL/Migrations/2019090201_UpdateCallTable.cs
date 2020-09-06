using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019090201)]
    public class UpdateCallTable : Migration
    {
        public override void Up()
        {
            Delete
                .Column("operator_extension")
                .Column("operator_user_name")
                .Column("operator_sip_uri")
                .FromTable("call_queue");

            Alter.Table("call_queue")
                .AlterColumn("operator_id").AsGuid().Nullable()
                .AddColumn("active_line_id").AsGuid().Nullable()
                .AddColumn("operator_connection_mode").AsInt32().Nullable();
        }

        public override void Down()
        {
            Delete
                .Column("active_line_id")
                .Column("operator_connection_mode")
                .FromTable("call_queue");

            Alter.Table("call_queue")
                .AddColumn("operator_extension").AsString().Nullable()
                .AddColumn("operator_user_name").AsString().Nullable()
                .AddColumn("operator_sip_uri").AsString().Nullable()
                .AlterColumn("operator_id").AsString().Nullable();
        }
    }
}
