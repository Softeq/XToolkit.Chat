using System.Collections.Generic;

namespace Softeq.XToolkit.Chat.Models
{
    public class CacheUpdatedResults
    {
        public string ChatId { get; set; }
        public IList<ChatMessageModel> NewMessages { get; set; }
        public IList<ChatMessageModel> UpdatedMessages { get; set; }
        public IList<string> DeletedMessagesIds { get; set; }
    }
}
