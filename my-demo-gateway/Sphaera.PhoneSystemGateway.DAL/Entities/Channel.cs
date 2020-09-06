using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace demo.DemoGateway.DAL.Entities
{
    /// <summary>
    /// Информация о канале телефонного звонка
    /// </summary>
    [Table("channel")]
    public class Channel
    {
        /// <summary>
        /// Идентификатор канала
        /// </summary>
        [Column("channel_id")]
        public string ChannelId { get; set; }

        /// <summary>
        /// Идентификатор звонка (будет совпадать с идентификатором в БД CTI сервиса) 
        /// </summary>
        [Column("call_id")]
        public Guid CallId { get; set; }

        /// <summary>
        /// Идентификатор моста
        /// </summary>
        [Column("bridge_id")]
        public string BridgeId { get; set; }

        /// <summary>
        /// Идентификатор активной линии (будет совпадать с идентификатором в БД CTI сервиса)
        /// </summary>
        [Column("line_id")]
        public Guid? LineId { get; set; }

        /// <summary>
        /// Телефонный номер канала
        /// </summary>
        [Column("extension")]
        public string Extension { get; set; }

        /// <summary>
        /// Кому звонят
        /// </summary>
        [Column("role")]
        public ChannelRoleType Role { get; set; }

        /// <summary>
        /// Идентификатор исходного канала, с которого была сделана snoop-копия 
        /// </summary>
        [Column("original_channel_id")]
        public string OriginalChannelId { get; set; }

        /// <summary>
        /// Звонок был непредвиденно прерван и ожидает новых подключений
        /// </summary>
        [Column("interrupted")]
        public bool Interrupted { get; set; }
    }
}