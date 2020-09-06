using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo.DemoGateway.DAL.Entities
{
    /// <summary>
    /// Информация о записываемом бридже или канале
    /// </summary>
    [Table("audio_record")]
    public class AudioRecord
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public AudioRecord()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Идентификатор записи
        /// </summary>
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор вызова
        /// </summary>
        [Column("call_id")]
        public Guid? CallId { get; set; }

        /// <summary>
        /// Идентификатор линии вызова
        /// </summary>
        [Column("line_id")]
        public Guid? LineId { get; set; }

        /// <summary>
        /// Имя файла записи
        /// </summary>
        [Column("file_name")]
        public string FileName { get; set; }

        /// <summary>
        /// Время начала записи
        /// </summary>
        [Column("recording_start_time")]
        public DateTime? RecordingStartTime { get; set; }

        /// <summary>
        /// Время окончания записи
        /// </summary>
        [Column("recording_end_time")]
        public DateTime? RecordingEndTime { get; set; }
    }
}