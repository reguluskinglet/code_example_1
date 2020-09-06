
namespace demo.DemoApi.Service.Dtos.Contact
{
    /// <summary>
    /// Dto с данными об параметрах пагинации
    /// </summary>
    public class PageViewModel
    {
        /// <summary>
        /// Номер текущей страницы
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Общее количество страниц
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Общее количество элементов
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Указывает, существует ли предыдущая страница
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Указывает, существует ли следующая страница
        /// </summary>
        public bool HasNextPage { get; set; }
    }
}
