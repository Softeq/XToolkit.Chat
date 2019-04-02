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

        Task<bool> CreateChatAsync(string chatName, IList<string> participantsIds, string imagePath);
        Task EditChatAsync(ChatSummaryModel chatSummary);
        Task CloseChatAsync(string chatId);
        Task LeaveChatAsync(string chatId);
        Task MuteChatAsync(string chatId);
        Task UnMuteChatAsync(string chatId);

        ChatSummaryViewModel GetChatById(string chatId);

        Task InviteMembersAsync(string chatId, IList<string> participantsIds);
        Task<IList<ChatUserViewModel>> GetChatMembersAsync(string chatId);

        Task<ChatSummaryViewModel> FindOrCreateDirectChatAsync(string id);

        void RefreshChatsListOnBackgroundAsync();
    }
}