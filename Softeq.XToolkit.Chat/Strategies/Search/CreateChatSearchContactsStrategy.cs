// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.WhiteLabel.Model;

namespace Softeq.XToolkit.Chat.Strategies.Search
{
    public class CreateChatSearchContactsStrategy : ISearchContactsStrategy
    {
        private readonly IChatService _chatService;

        public CreateChatSearchContactsStrategy(IChatService chatService)
        {
            _chatService = chatService;
        }

        public Task<PagingModel<ChatUserModel>> Search(string query, int pageNumber, int pageSize)
        {
            return _chatService.GetContactsAsync(query, pageNumber, pageSize);
        }
    }
}