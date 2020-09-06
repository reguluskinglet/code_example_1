using System.Collections.Generic;
using AsterNET.ARI.Models;

namespace demo.DemoGateway.Dtos
{
    /// <summary>
    /// Информация о бридже Asterisk с каналами
    /// </summary>
    public class BridgeDto
    {
        /// <summary>
        /// Id бриджа
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Список каналов бриджа
        /// </summary>
        public List<string> Channels { get; set; }

        /// <summary>
        /// Сделать Dto модель из Bridge
        /// </summary>
        public static BridgeDto MapFromBridge(Bridge bridge)
        {
            return new BridgeDto
            {
                Id = bridge.Id,
                Channels = new List<string>(bridge.Channels)
            };
        }
    }
}
