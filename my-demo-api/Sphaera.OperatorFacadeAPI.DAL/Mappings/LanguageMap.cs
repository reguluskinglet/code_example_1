using FluentNHibernate.Mapping;
using NHibernate.Type;
using demo.DDD.Mappings;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.DAL.Mappings
{
    /// <summary>
    /// Маппинг сущности
    /// </summary>
    public class LanguageMap : AggregateRootMap<Language>
    {
        /// <inheritdoc />
        public LanguageMap()
        {
            Table("language");
            Map(x => x.Code, "code").CustomType<GenericEnumMapper<LanguageCode>>();
            Map(x => x.Name, "name").Nullable();
            Map(x => x.Data, "data").Nullable();
            Map(x => x.DisplayDirection, "display_direction");

            Not.LazyLoad();
        }
    }
}
