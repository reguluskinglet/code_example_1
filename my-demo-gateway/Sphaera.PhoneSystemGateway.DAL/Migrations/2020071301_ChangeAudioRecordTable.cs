using FluentMigrator;

namespace demo.DemoGateway.DAL.Migrations
{
    [Migration(2020071301)]
    public class ChangeAudioRecordTable : Migration
    {
        public override void Up()
        {
            Delete.Column("record_date").FromTable("audio_record");
            Delete.Column("channel_id").FromTable("audio_record");
            Delete.Column("bridge_id").FromTable("audio_record");

            Create.Column("call_id").OnTable("audio_record").AsGuid().Nullable();
            Create.Column("recording_start_time").OnTable("audio_record").AsDateTime2().Nullable();
            Create.Column("recording_end_time").OnTable("audio_record").AsDateTime2().Nullable();
        }

        public override void Down()
        {
            Create.Column("channel_id").OnTable("audio_record").AsString().Nullable();
            Create.Column("bridge_id").OnTable("audio_record").AsString().Nullable();
            Create.Column("record_date").OnTable("audio_record").AsDateTime().Nullable();

            Delete.Column("call_id").FromTable("audio_record");
            Delete.Column("recording_start_time").FromTable("audio_record");
            Delete.Column("recording_end_time").FromTable("audio_record");
        }
    }
}