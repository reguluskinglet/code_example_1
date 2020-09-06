using demo.DDD.Mappings;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Mappings
{
    /// <summary>
    /// Маппинг сущности CaseType на таблицу
    /// </summary>
    public class CaseTypeMap : AggregateRootMap<CaseType>
    {
        /// <inheritdoc />
        public CaseTypeMap()
        {
            Table("case_type");
            Map(x => x.Data, "data");
            Map(x => x.Css, "css");

            HasMany(x => x.Cases)
                .Inverse()
                .Fetch.Subselect().KeyColumn("case_type_id");

            Not.LazyLoad();
        }
    }
}
