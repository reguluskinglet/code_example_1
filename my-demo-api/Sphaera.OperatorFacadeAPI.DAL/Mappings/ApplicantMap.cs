using FluentNHibernate.Mapping;
using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.DAL.Mappings
{
    /// <summary>
    /// Маппинг сущности Applicant На таблицу "applicant"
    /// </summary>
    public class ApplicantMap: SubclassMap<Applicant>
    {
        /// <inheritdoc />
        public ApplicantMap()
        {
            Table("applicant");

            Map(x => x.Extension, "extension");
            
            Not.LazyLoad();
        }
        
    }
}
