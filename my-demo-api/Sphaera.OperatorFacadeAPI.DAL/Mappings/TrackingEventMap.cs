using FluentNHibernate.Mapping;
using demo.DDD;

namespace demo.DemoApi.DAL.Mappings
{
    /// <summary>
    /// Маппинг сущности
    /// </summary>
    public class TrackingEventMap : ClassMap<TrackingEvent>
    {
        /// <inheritdoc />
        public TrackingEventMap()
        {
            Table("tracking_event");
            Id(x => x.Id, "id").GeneratedBy.Assigned();
            Map(x => x.Version, "version");
            Map(x => x.TransactionId, "transaction_id");
            Map(x => x.EntityId, "entity_id");
            Map(x => x.AggreagateId, "aggregate_id");
            Map(x => x.TransactionDate, "transaction_date");
            Map(x => x.EntityName, "entity_name");
            Map(x => x.OperationType, "operation_type").CustomType<EventType>();
            Map(x => x.Data, "data");
            Map(x => x.UserId, "user_id").Nullable();

            Not.LazyLoad();
        }
    }
}