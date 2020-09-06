using FluentMigrator;

namespace demo.DemoGateway.DAL.Migrations
{
    [Migration(2020061801)]
    public class AddBridgeAudioRecordTable : Migration
    {
        public override void Up()
        {
            if (Schema.Table("audio_record").Exists())
            {
                return;
            }

            Create.Table("audio_record")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("bridge_id").AsString().Nullable()
                .WithColumn("channel_id").AsString().Nullable()
                .WithColumn("file_name").AsString()
                .WithColumn("line_id").AsGuid().Nullable()
                .WithColumn("record_date").AsDateTime().Nullable();
        }

        public override void Down()
        {
            Delete.Table("audio_record");
        }
    }
}
