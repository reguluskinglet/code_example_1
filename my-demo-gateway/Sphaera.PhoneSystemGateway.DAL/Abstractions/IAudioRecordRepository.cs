using System;
using System.Threading.Tasks;
using demo.DemoGateway.DAL.Entities;

namespace demo.DemoGateway.DAL.Abstractions
{
    /// <summary>
    /// Интерфейс репозитория записываемых бриджей и каналов
    /// </summary>
    public interface IAudioRecordRepository
    {
        /// <summary>
        /// Добавление информации о записи разговора
        /// </summary>
        Task AddAudioRecord(AudioRecord entity);

        /// <summary>
        /// Получение записи по имени
        /// </summary>
        Task<AudioRecord> GetRecordByName(string recordName);

        /// <summary>
        /// Получение информации о записываемом канале по CallId
        /// </summary>
        Task<AudioRecord> GetRecordByCallId(Guid callId);

        /// <summary>
        /// Обновить запись
        /// </summary>
        Task UpdateRecord(AudioRecord audioRecord);
    }
}
