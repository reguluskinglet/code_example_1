using demo.DemoApi.Domain.Entities;

namespace demo.DemoApi.Domain.ValueObjects
{
    /// <summary>
    /// Результат добавления звонка.
    /// </summary>
    public class AddItemResult
    {
        /// <summary>
        /// Заявитель или другой участник, который звонит в службу.
        /// </summary>
        public Participant Participant { get; set; }
    }
}
