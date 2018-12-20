using System.Collections.Generic;

namespace Softeq.XToolkit.Chat.Models.Exceptions
{
    public class ChatValidationException : ChatException
    {
        public ChatValidationException(IEnumerable<string> errors)
            : base("Chat validation exception")
        {
            Errors = new List<string>(errors);
        }

        public IList<string> Errors { get; }
    }
}
