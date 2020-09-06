using FluentMigrator;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020031902)]
    public class AddSmsTable : Migration
    {
        public override void Up()
        {
            Create.Table("sms")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("participant_id").AsGuid()
                .WithColumn("status").AsInt32().Nullable()
                .WithColumn("text").AsString().Nullable()
                .WithColumn("arrival_datetime").AsDateTime().Nullable()
                .WithColumn("accept_datetime").AsDateTime().Nullable()
                .WithColumn("inbox_id").AsGuid().Nullable();
            
            Create.Column("type").OnTable("inbox").AsString(30).WithDefaultValue("Calls");

            Delete.Column("sms_text").FromTable("call_queue");
            Delete.Column("is_sms").FromTable("call_queue");
            
            Update.Table("inbox").Set(new
            { 
                type = "Calls"
            }).Where( new
            { 
                id = AddInbox.InboxId112
            });
            
            Update.Table("inbox").Set(new
            { 
                type = "Calls"
            }).Where( new
            { 
                id = AddInbox.InboxIdConnection
            });
            
            Update.Table("inbox").Set(new
            { 
                type = "Calls"
            }).Where( new
            { 
                id = AddInbox.InboxIdResource
            });
            
            Update.Table("inbox").Set(new
            { 
                type = "Sms"
            }).Where( new
            { 
                id = AddSmsQueue.InboxIdSms
            });
        }
        
        public override void Down()
        {
            Delete.Table("sms");
        }
    }
}
