// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.Models.Queries;
using Softeq.XToolkit.WhiteLabel.Model;

namespace Softeq.XToolkit.Chat.Strategies.Search
{
    public class InviteSearchContactsStrategy : ISearchContactsStrategy
    {
        private readonly IChatService _chatService;
        private readonly string _chatId;

        public InviteSearchContactsStrategy(
            IChatService chatService,
            string chatId)
        {
            _chatService = chatService;
            _chatId = chatId;
        }

        public Task<PagingModel<ChatUserModel>> Search(string query, int pageNumber, int pageSize)
        {
            return _chatService.GetContactsForInviteAsync(new ContactsQuery
            {
                ChannelId = _chatId,
                NameFilter = query,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
    }
}