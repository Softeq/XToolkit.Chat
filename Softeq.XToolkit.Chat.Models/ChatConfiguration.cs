// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.Models.Interfaces;

namespace Softeq.XToolkit.Chat.Models
{
    public class ChatConfiguration : IChatConfiguration
    {
        public string BaseUrl { get; }

        public string ApiUrl => BaseUrl + "/api";

        public ChatConfiguration(string baseUrl)
        {
            BaseUrl = baseUrl;
        }
    }
}
