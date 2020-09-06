using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019112001)]
    public class CreateCaseFolderAndLinks : Migration
    {
        public override void Up()
        {
            Create
                .Table("case_folder")
                .WithColumn("id").AsGuid().PrimaryKey();

            Alter
                .Table("case_card")
                .AddColumn("case_folder_id").AsGuid().Nullable();
            
            Alter
                .Table("line")
                .AddColumn("case_folder_id").AsGuid().Nullable();
            
            Delete
                .Column("case_card_id")
                .FromTable("line");
        }

        public override void Down()
        {
            Delete.Table("case_folder");

            Delete.Column("case_folder_id").FromTable("case_card");
        }
    }
}
