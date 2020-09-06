using demo.DDD.Mappings;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Mappings
{
    /// <summary>
    /// Маппинг сущности Participant (Участник разговора)
    /// </summary>
    public class ParticipantMap : AggregateRootMap<Participant>
    {
        /// <inheritdoc />
        public ParticipantMap()
        {
            Id(x => x.Id, "id").GeneratedBy.Assigned();
            
            UseUnionSubclassForInheritanceMapping();
            
            Not.LazyLoad();
        }
    }
}
