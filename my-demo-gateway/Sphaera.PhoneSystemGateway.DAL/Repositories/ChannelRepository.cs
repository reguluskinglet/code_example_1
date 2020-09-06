using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.DAL.Entities;
using demo.DemoGateway.DAL.Repositories.Base;

namespace demo.DemoGateway.DAL.Repositories
{
    /// <summary>
    /// Репозиторий каналов телефонных звонков
    /// </summary>
    public class ChannelRepository : BaseRepository, IChannelRepository
    {
        /// <inheritdoc />
        public ChannelRepository(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Добавление информация о канале телефонного звонка
        /// </summary>
        public async Task AddChannel(Channel entity)
        {
            var sql =
                "INSERT INTO channel (channel_id, call_id, bridge_id, line_id, extension, role, original_channel_id, interrupted) " +
                "VALUES (@ChannelId, @CallId, @BridgeId, @LineId, @Extension, @Role, @OriginalChannelId, @Interrupted);";

            using (var connection = OpenConnection())
            {
                await connection.ExecuteAsync(sql, entity);
            }
        }

        /// <summary>
        /// Получение канала телефонного звонка по идентификатору канала
        /// </summary>
        public async Task<Channel> GetByChannelId(string channelId)
        {
            var sql = "SELECT * FROM channel WHERE channel_id = @ChannelId";

            using (var connection = OpenConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<Channel>(sql, new { ChannelId = channelId });
            }
        }

        /// <summary>
        /// Получение каналов телефонного звонка по идентификатору звонка
        /// </summary>
        public async Task<Channel> GetChannelByCallId(Guid callId)
        {
            var sql = "SELECT * FROM channel WHERE call_id = @CallId AND role != @SnoopRole AND role != @SpeakSnoopRole";

            using (var connection = OpenConnection())
            {
                return (await connection.QueryAsync<Channel>(sql,
                        new
                        {
                            CallId = callId,
                            SnoopRole = (int)ChannelRoleType.SnoopChannel,
                            SpeakSnoopRole = (int)ChannelRoleType.SpeakSnoopChannel
                        }))
                    .SingleOrDefault();
            }
        }

        /// <summary>
        /// Получить список каналов, входящих в бридж
        /// </summary>
        public async Task<IList<Channel>> GetByBridgeId(string bridgeId)
        {
            var sql = "SELECT * FROM channel WHERE bridge_id = @BridgeId";

            using (var connection = OpenConnection())
            {
                return (await connection.QueryAsync<Channel>(sql, new { BridgeId = bridgeId })).ToList();
            }
        }

        /// <summary>
        /// Обновить канал
        /// </summary>
        public async Task UpdateChannel(Channel channel)
        {
            var sql =
                "UPDATE channel SET call_id = @CallId, bridge_id = @BridgeId, line_id = @LineId, extension = @Extension, role = @Role, original_channel_id = @OriginalChannelId, interrupted = @Interrupted " +
                "WHERE channel_id = @ChannelId";

            using (var connection = OpenConnection())
            {
                await connection.ExecuteAsync(sql, channel);
            }
        }

        /// <summary>
        /// Удаление канала телефонного звонка по идентификатору канала
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public async Task DeleteChannel(string channelId)
        {
            var sql = "DELETE FROM channel WHERE channel_id = @ChannelId";

            using (var connection = OpenConnection())
            {
                await connection.ExecuteAsync(sql, new { ChannelId = channelId });
            }
        }

        /// <summary>
        /// Возвращает список каналов по идентификатору активной линии
        /// </summary>
        public async Task<IList<Channel>> GetChannelsByLineId(Guid lineId)
        {
            var sql = "SELECT * FROM channel WHERE line_id = @LineId";

            using (var connection = OpenConnection())
            {
                return (await connection.QueryAsync<Channel>(sql, new { LineId = lineId })).ToList();
            }
        }

        /// <summary>
        /// Получить канал главного в разговоре в линии
        /// </summary>
        public async Task<Channel> GetChannelForMainUser(Guid lineId)
        {
            var sql = "SELECT * FROM channel WHERE line_id = @LineId AND role = @Role";

            using (var connection = OpenConnection())
            {
                return (await connection.QueryAsync<Channel>(sql, new { LineId = lineId, Role = (int)ChannelRoleType.MainUser })).SingleOrDefault();
            }
        }

        /// <summary>
        /// Получить канал входящего вызова (например, канал заявителя или другого участника, позвонившего в службу)
        /// </summary>
        public async Task<Channel> GetChannelForIncomingCall(Guid lineId)
        {
            var sql = "SELECT * FROM channel WHERE line_id = @LineId AND role = @Role";

            using (var connection = OpenConnection())
            {
                return (await connection.QueryAsync<Channel>(sql, new { LineId = lineId, Role = (int)ChannelRoleType.ExternalChannel })).SingleOrDefault();
            }
        }

        /// <summary>
        /// Получить канал в линии по Extension
        /// </summary>
        public async Task<Channel> GetChannelInLineByExtension(Guid lineId, string extension)
        {
            var sql = "SELECT * FROM channel WHERE line_id = @LineId AND extension = @Extension AND role != @SnoopRole AND role != @SpeakSnoopRole";

            using (var connection = OpenConnection())
            {
                return (await connection.QueryAsync<Channel>(sql,
                        new
                        {
                            LineId = lineId,
                            Extension = extension,
                            SnoopRole = (int)ChannelRoleType.SnoopChannel,
                            SpeakSnoopRole = (int)ChannelRoleType.SpeakSnoopChannel
                        }))
                    .SingleOrDefault();
            }
        }

        /// <summary>
        /// Получить идентификатор главного бриджа в линии
        /// </summary>
        public async Task<string> GetMainBridgeId(Guid lineId)
        {
            var channelsInLine = await GetChannelsByLineId(lineId);
            var channelInMainBridge = channelsInLine.FirstOrDefault(x => x.Role == ChannelRoleType.ExternalChannel
                                                                         || x.Role == ChannelRoleType.MainUser
                                                                         || x.Role == ChannelRoleType.Conference);

            return channelInMainBridge?.BridgeId;
        }
    }
}