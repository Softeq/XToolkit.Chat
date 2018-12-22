using System.Collections.Generic;

namespace Softeq.XToolkit.Chat.Models.Exceptions
{
    public class ChatValidationException : ChatException
    {
        public ChatValidationException(IEnumerable<string> errors)
            : base(string.Concat("Chat Validation Exception: ", string.Join("\n", errors)))
        {
            Errors = new List<string>(errors);
        }

        public IList<string> Errors { get; }
    }
}
