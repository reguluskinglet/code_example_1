using demo.DDD.Mappings;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Domain.Enums;

namespace demo.DemoApi.DAL.Mappings
{
    /// <summary>
    /// Маппинг сущности
    /// </summary>
    public class SmsMap : AggregateRootMap<Sms>
    {
        /// <inheritdoc />
        public SmsMap()
        {
            Table("sms");
            Map(x => x.Status, "status").CustomType<SmsStatus>();
            Map(x => x.ArrivalDateTime, "arrival_datetime").Nullable();
            Map(x => x.AcceptDateTime, "accept_datetime").Nullable();
            Map(x => x.Text, "text").Nullable();
            Map(x => x.LocationMetadata, "location_metadata").Nullable();
            Map(x => x.Timestamp, "timestamp").Nullable();

            References(x => x.Applicant)
                .Nullable()
                .Cascade.SaveUpdate()
                .Column("participant_id");

            Not.LazyLoad();
        }
    }
}
