// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Settings
{
    internal class SettingsResponse
    {
        public Guid Id { get; set; }
        public string RawSettings { get; set; }
        public Guid ChannelId { get; set; }
    }
}
