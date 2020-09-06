using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using demo.FunctionalExtensions;
using demo.DemoApi.Service.Dtos.Line;

namespace demo.DemoApi.Service.ApplicationServices.Abstractions
{
    /// <summary>
    /// Сервис для работы с Line
    /// </summary>
    public interface ILineService
    {
        /// <summary>
        /// Получить все Line для User
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<Result<List<LineViewDto>>> GetUserLines(Guid userId);

        /// <summary>
        /// Получить линии по caseFolderId
        /// </summary>
        /// <param name="caseFolderId"></param>
        /// <returns></returns>
        Task<Result<List<LineByCaseFolderViewDto>>> GetLinesByCaseFolderId(Guid caseFolderId);
    }
}
