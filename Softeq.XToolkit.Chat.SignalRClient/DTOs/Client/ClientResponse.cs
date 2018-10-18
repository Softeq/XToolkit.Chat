// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Client
{
    internal class ClientResponse
    {
        public Guid Id { get; set; }
        public string ConnectionClientId { get; set; }
        public string UserName { get; set; }
        public string SaasUserId { get; set; }
    }
}
