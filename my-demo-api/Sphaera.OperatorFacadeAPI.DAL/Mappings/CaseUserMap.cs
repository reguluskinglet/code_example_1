using demo.DDD.Mappings;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Mappings
{
    /// <summary>
    /// Маппинг кейс юхера
    /// </summary>
    public class CaseUserMap : BaseEntityMap<CaseUser>
    {
        /// <inheritdoc />
        public CaseUserMap() : base()
        {
            Table("case_user");
            Id(x => x.Id, "id").GeneratedBy.Assigned();
            Map(x => x.UserId, "user_id");

            References(x => x.Case)
                .Cascade.SaveUpdate()
                .Column("case_id");

            Not.LazyLoad();
        }
    }
}
