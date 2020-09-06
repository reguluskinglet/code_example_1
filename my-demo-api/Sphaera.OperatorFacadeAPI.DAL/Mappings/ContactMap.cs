using FluentNHibernate.Mapping;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Mappings
{
    /// <summary>
    /// Маппинг сущности Contact На таблицу "contact"
    /// </summary>
    public class ContactMap : SubclassMap<Contact>
    {
        /// <inheritdoc />
        public ContactMap()
        {
            Table("contact");

            Map(x => x.Extension, "extension");
            Map(x => x.Name, "name").Nullable();
            Map(x => x.Position, "position").Nullable();
            Map(x => x.Organization, "organization").Nullable();
            Map(x => x.ContactRouteName, "contact_route_name").Nullable();

            Not.LazyLoad();
        }
    }
}