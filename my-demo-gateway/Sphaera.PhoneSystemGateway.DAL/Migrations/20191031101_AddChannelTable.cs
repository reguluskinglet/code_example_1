using FluentMigrator;

namespace demo.DemoGateway.DAL.Migrations
{
    [Migration(201910311622)]
    public class AddChannelTable : Migration
    {
        public override void Up()
        {
            Create.Table("channel")
                .WithColumn("channel_id").AsString().PrimaryKey()
                .WithColumn("call_id").AsGuid().NotNullable()
                .WithColumn("bridge_id").AsString().Nullable()
                .WithColumn("line_id").AsGuid().Nullable()
                .WithColumn("extension").AsString().Nullable()
                .WithColumn("role").AsInt32().NotNullable()
                .WithColumn("original_channel_id").AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Table("channel");
        }
    }
}
