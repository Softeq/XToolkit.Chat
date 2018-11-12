// Developed by Softeq Development Corporation
// http://www.softeq.com

using Softeq.XToolkit.Chat.Models.Interfaces;

namespace Softeq.XToolkit.Chat.Models
{
    public class ChatConfig : IChatConfig
    {
        public string BaseUrl { get; }

        public string ApiUrl => BaseUrl + "/api";

        public string BlobUrl { get; }

        public ChatConfig(string baseUrl, string blobUrl)
        {
            BaseUrl = baseUrl;
            BlobUrl = blobUrl;
        }
    }
}
