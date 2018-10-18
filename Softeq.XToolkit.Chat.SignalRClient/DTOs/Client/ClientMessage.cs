// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.SignalRClient.DTOs;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Client
{
    internal class ClientMessage : BaseRequest
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public Guid RoomId { get; set; }
    }
}
