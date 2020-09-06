using FluentMigrator;

namespace demo.DemoApi.DAL.Migrations
{
    [Migration(2020032401)]
    public class Add_MinMax_ContactsWindowSize : Migration
    {
        public override void Up()
        {
            var user1001Preferences = "{\"gisWindow\":{\"minSize\":{\"width\":800,\"height\":600},\"maxSize\":{\"width\":1600,\"height\":900}},\"operatorWindow\":{\"minSize\":{\"width\":1000,\"height\":500},\"maxSize\":{\"width\":1500,\"height\":800}},\"contactsWindow\":{\"minSize\":{\"width\":400,\"height\":600},\"maxSize\":{\"width\":1200}}}";
            var user666Preferences = "{\"gisWindow\":{\"minSize\":{\"width\":666,\"height\":666},\"maxSize\":{\"width\":666,\"height\":666}},\"operatorWindow\":{\"minSize\":{\"width\":666,\"height\":666},\"maxSize\":{\"width\":666,\"height\":666}},\"contactsWindow\":{\"minSize\":{\"width\":400,\"height\":600},\"maxSize\":{\"width\":1200}}}";
            var userPreferences = "{\"contactsWindow\":{\"minSize\":{\"width\":400,\"height\":600},\"maxSize\":{\"width\":1200}}}";

            Update.Table("operator")
                .Set(new { preferences = userPreferences })
                .AllRows();

            Update.Table("operator")
                .Set(new { preferences = user1001Preferences })
                .Where(new { extension = "1001" });
            
            Update.Table("operator")
                .Set(new { preferences = user666Preferences })
                .Where(new { extension = "666" });
        }

        public override void Down()
        {
        }
    }
}
