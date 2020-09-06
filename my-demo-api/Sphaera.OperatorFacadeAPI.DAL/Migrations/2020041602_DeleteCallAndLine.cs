using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020041602)]
    public class DeleteCallAndLine : Migration
    {
        public override void Up()
        {
            Delete.Table("line");
            Delete.Table("call_queue");
        }

        public override void Down()
        {
            Create.Table("line")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("create_datetime").AsDateTime()
                .WithColumn("case_folder_id").AsGuid().Nullable();

            Create.Table("call_queue")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("participant_id").AsGuid().Nullable()
                .WithColumn("status").AsInt32()
                .WithColumn("canceled").AsBoolean()
                .WithColumn("arrival_datetime").AsDateTime().Nullable()
                .WithColumn("line_id").AsGuid().Nullable()
                .WithColumn("operator_connection_mode").AsInt32().Nullable()
                .WithColumn("accept_datetime").AsDateTime().Nullable()
                .WithColumn("isolated").AsBoolean().Nullable()
                .WithColumn("is_muted").AsBoolean().WithDefaultValue(false)
                .WithColumn("inbox_id").AsGuid().Nullable()
                .WithColumn("on_hold").AsBoolean().WithDefaultValue(false)
                .WithColumn("group_call_id").AsGuid().Nullable()
                .WithColumn("caller_id").AsGuid().Nullable();
        }
    }
}
