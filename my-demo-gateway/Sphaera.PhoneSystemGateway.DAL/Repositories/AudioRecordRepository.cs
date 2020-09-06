using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using demo.DemoGateway.DAL.Abstractions;
using demo.DemoGateway.DAL.Entities;
using demo.DemoGateway.DAL.Repositories.Base;

namespace demo.DemoGateway.DAL.Repositories
{
    /// <summary>
    /// Репозиторий информации о записываемых бриджах или каналах
    /// </summary>
    public class AudioRecordRepository : BaseRepository, IAudioRecordRepository
    {
        /// <inheritdoc />
        public AudioRecordRepository(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Добавление информации о записи разговора
        /// </summary>
        public async Task AddAudioRecord(AudioRecord entity)
        {
            var sql =
                "INSERT INTO audio_record (id, call_id, line_id, file_name, recording_start_time, recording_end_time) " +
                "VALUES (@Id, @CallId, @LineId, @FileName, @RecordingStartTime, @RecordingEndTime);";

            using (var connection = OpenConnection())
            {
                await connection.ExecuteAsync(sql, entity);
            }
        }

        /// <summary>
        /// Получение записи по имени
        /// </summary>
        public async Task<AudioRecord> GetRecordByName(string fileName)
        {
            var sql = "SELECT * FROM audio_record WHERE file_name = @FileName";

            using (var connection = OpenConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<AudioRecord>(sql, new { FileName = fileName });
            }
        }

        /// <summary>
        /// Получение информации о записываемом канале по CallId
        /// </summary>
        public async Task<AudioRecord> GetRecordByCallId(Guid callId)
        {
            var sql = "SELECT * FROM audio_record WHERE call_id = @CallId";

            using (var connection = OpenConnection())
            {
                return await connection.QuerySingleOrDefaultAsync<AudioRecord>(sql, new { CallId = callId });
            }
        }

        /// <summary>
        /// Обновить запись
        /// </summary>
        public async Task UpdateRecord(AudioRecord audioRecord)
        {
            var sql =
                "UPDATE audio_record SET recording_start_time = @RecordingStartTime, recording_end_time = @RecordingEndTime, line_id = @LineId " +
                "WHERE id = @Id";

            using (var connection = OpenConnection())
            {
                await connection.ExecuteAsync(sql, audioRecord);
            }
        }
    }
}