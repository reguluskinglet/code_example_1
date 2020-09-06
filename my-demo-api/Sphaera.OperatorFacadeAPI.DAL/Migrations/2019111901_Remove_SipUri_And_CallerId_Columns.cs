using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019111901)]
    public class AddParticipantCallerApplicant : Migration
    {
        public override void Up()
        {
            Delete
                .Column("sip_uri")
                .FromTable("operator");
            Delete
                .Column("caller_id")
                .FromTable("line");
        }

        public override void Down()
        {
            Alter
                .Table("line")
                .AddColumn("caller_id")
                .AsString();
            
            Alter
                .Table("operator")
                .AddColumn("sip_uri")
                .AsString().Nullable();
        }
    }
}
