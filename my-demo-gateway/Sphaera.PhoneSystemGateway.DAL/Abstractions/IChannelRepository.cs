using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using demo.DemoGateway.DAL.Entities;

namespace demo.DemoGateway.DAL.Abstractions
{
    /// <summary>
    /// Интерфейс репозитория каналов
    /// </summary>
    public interface IChannelRepository
    {
        /// <summary>
        /// Добавление информация о канале телефонного звонка
        /// </summary>
        Task AddChannel(Channel entity);

        /// <summary>
        /// Получение канала телефонного звонка по идентификатору канала
        /// </summary>
        Task<Channel> GetByChannelId(string channelId);

        /// <summary>
        /// Получение каналов по BridgeId
        /// </summary>
        Task<IList<Channel>> GetByBridgeId(string bridgeId);

        /// <summary>
        /// Обновить канал
        /// </summary>
        Task UpdateChannel(Channel channel);
        
        /// <summary>
        /// Удалить канал
        /// </summary>
        Task DeleteChannel(string channelId);

        /// <summary>
        /// Взять каналы по lineId
        /// </summary>
        /// <param name="lineId">Line id</param>
        /// <returns>Каналы</returns>
        Task<IList<Channel>> GetChannelsByLineId(Guid lineId);
        
        /// <summary>
        /// Взять канал по id звонка.
        /// </summary>
        /// <param name="callId"> Id звонка. </param>
        /// <returns></returns>
        Task<Channel> GetChannelByCallId(Guid callId);

        /// <summary>
        /// Получить канал главного в разговоре в линии
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        Task<Channel> GetChannelForMainUser(Guid lineId);

        /// <summary>
        /// Получить канал входящего вызова
        /// </summary>
        /// <param name="lineId"></param>
        /// <returns></returns>
        Task<Channel> GetChannelForIncomingCall(Guid lineId);

        /// <summary>
        /// Получить канал из линии по Extension
        /// </summary>
        /// <param name="lineId"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        Task<Channel> GetChannelInLineByExtension(Guid lineId, string extension);

        /// <summary>
        /// Получить идентификатор главного бриджа в линии
        /// </summary>
        Task<string> GetMainBridgeId(Guid lineId);
    }
}
