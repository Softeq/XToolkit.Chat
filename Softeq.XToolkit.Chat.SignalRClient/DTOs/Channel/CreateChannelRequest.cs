// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Channel
{
    internal class CreateChannelRequest : BaseRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ChannelTypeDto Type { get; set; }
        public string WelcomeMessage { get; set; }
        public IList<string> AllowedMembers { get; set; }
        public string PhotoUrl { get; set; }
    }
}