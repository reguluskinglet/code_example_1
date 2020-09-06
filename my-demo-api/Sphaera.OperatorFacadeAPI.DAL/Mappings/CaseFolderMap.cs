using demo.DDD.Mappings;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Mappings
{
    /// <summary>
    /// Маппинг сущности CaseFolderMap на таблицу
    /// </summary>
    public class CaseFolderMap : AggregateRootMap<CaseFolder>
    {
        /// <inheritdoc />
        public CaseFolderMap()
        {
            Table("case_folder");
            Map(x => x.Data, "data").Nullable();
            Map(x => x.Latitude, "latitude").Nullable();
            Map(x => x.Longitude, "longitude").Nullable();
            Map(x => x.Status, "status");

            HasMany(x => x.Cases)
                .Cascade.AllDeleteOrphan().Inverse()
                .Fetch
                .Subselect().KeyColumn("case_folder_id");
            

            References(x => x.Sms)
                .Nullable()
                .Cascade.SaveUpdate()
                .Column("sms_id");

            Not.LazyLoad();
        }
    }
}