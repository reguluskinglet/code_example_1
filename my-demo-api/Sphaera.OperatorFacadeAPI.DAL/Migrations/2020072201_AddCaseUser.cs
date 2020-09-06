using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020072201)]
    public class AddCaseUser : Migration
    {
        /// <inheritdoc />
        public override void Up()
        {
            Create.Table("case_user")
                .WithColumn("id").AsGuid().PrimaryKey()
                .WithColumn("case_id").AsGuid().Nullable()
                .WithColumn("user_id").AsGuid().Nullable();
        }

        /// <inheritdoc />
        public override void Down()
        {
            Delete.Table("case_user");
        }
    }
}