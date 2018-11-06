// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;

namespace Softeq.XToolkit.Chat.HttpClient.Dtos
{
    internal class CreateChatDto
    {
        public string Name { get; set; }
        public string Topic { get; set; }
        public string WelcomeMessage { get; set; }
        public int Type { get; set; } = 1;
        public IList<string> AllowedMembers { get; set; }
        public string SaasUserId { get; set; }
    }
}
