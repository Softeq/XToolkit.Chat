using Softeq.XToolkit.Chat.ViewModels;

namespace Softeq.XToolkit.Chat.Messages
{
    public class OpenNewChatMessage
    {
        public OpenNewChatMessage(ChatSummaryViewModel viewModel)
        {
            Data = viewModel;
        }

        public ChatSummaryViewModel Data { get; }
    }
}
