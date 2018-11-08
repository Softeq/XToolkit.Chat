// Developed by Softeq Development Corporation
// http://www.softeq.com

namespace Softeq.XToolkit.Chat.SignalRClient.DTOs.Channel
{
    internal class UpdateChannelRequest : BaseRequest
    {
        public string ChannelId { get; set; }
        public string Name { get; set; }
        public string Topic { get; set; }
        public string WelcomeMessage { get; set; }
        public string PhotoUrl { get; set; }
    }
}
