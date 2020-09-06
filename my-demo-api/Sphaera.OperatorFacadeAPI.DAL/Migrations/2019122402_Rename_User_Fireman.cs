using FluentMigrator;
using demo.DemoApi.DAL.Migrations.Metadata;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2019122402)]
    public class Rename_User_Fireman : Migration
    {
        public override void Up()
        {
            Update.Table("operator")
                .Set(new { user_name = "Диспетчер ПЧ" })
                .Where(new { id = UsersMetadata.UserIdFireDepartment3 });
        }

        public override void Down()
        {
        }
    }
}
