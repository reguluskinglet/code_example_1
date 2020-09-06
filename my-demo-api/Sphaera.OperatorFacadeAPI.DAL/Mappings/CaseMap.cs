using FluentNHibernate;
using demo.DDD.Mappings;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Mappings
{
    /// <summary>
    /// Маппинг сущности Case на таблицу
    /// </summary>
    public class CaseMap : BaseEntityMap<Case>
    {
        /// <inheritdoc />
        public CaseMap() : base()
        {
            Table("case_card");
            Id(x => x.Id, "id").GeneratedBy.Assigned();
            Map(x => x.Created, "created");
            Map(x => x.Updated, "updated");
            Map(x => x.ActivatedPlanInstructions, "activated_plan_instructions").Nullable();
            Map(Reveal.Member<Case>("_status"), "status").Nullable();
            Map(x => x.IndexValue, "index_value").Nullable();


            HasMany(x => x.CaseUsers)
                .Cascade.All().Inverse()
                .Fetch
                .Subselect().KeyColumn("case_id");

            References(x => x.Type)
                .Column("case_type_id");

            References(x => x.CaseFolder)
                .Cascade.SaveUpdate()
                .Column("case_folder_id");

            Not.LazyLoad();
        }
    }
}
