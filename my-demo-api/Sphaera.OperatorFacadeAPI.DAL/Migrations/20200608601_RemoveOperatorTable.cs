using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(20200608601)]
    public class RemoveOperatorTable : Migration
    {
        /// <inheritdoc />
        public override void Up()
        {
            Delete.Table("operator");
        }

        /// <inheritdoc />
        public override void Down()
        {
        }
    }
}
