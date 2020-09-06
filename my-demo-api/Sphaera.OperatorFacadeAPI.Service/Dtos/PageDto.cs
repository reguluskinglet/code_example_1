using System;

namespace demo.DemoApi.Service.Dtos
{
    /// <summary>
    /// Dto с данными об параметрах пагинации
    /// </summary>
    public class PageDto
    {
        /// <summary>
        /// Создание модели
        /// </summary>
        /// <param name="count"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        public PageDto(int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
        }
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
        public bool HasPreviousPage => PageIndex > 1;

        /// <summary>
        /// Указывает, существует ли следующая страница
        /// </summary>
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
