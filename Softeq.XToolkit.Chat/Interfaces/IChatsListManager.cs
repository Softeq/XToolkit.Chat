using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Collections;

namespace Softeq.XToolkit.Chat.Interfaces
{
    public interface IChatsListManager
    {
        ObservableRangeCollection<ChatSummaryViewModel> ChatsCollection { get; }

        Task CreateChatAsync(string chatName, IList<string> participantsIds, string imagePath);
        Task EditChatAsync(ChatSummaryModel chatSummary);
        Task CloseChatAsync(string chatId);
        Task LeaveChatAsync(string chatId);

        Task InviteMembersAsync(string chatId, IList<string> participantsIds);
        Task<IList<ChatUserViewModel>> GetChatMembersAsync(string chatId);

        void RefreshChatsListOnBackgroundAsync();
    }
}