// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.Chat.ViewModels;

namespace Softeq.XToolkit.Chat.Strategies.Search
{
    public class CreateChatSearchContactsStrategy : ISearchContactsStrategy
    {
        private readonly ChatManager _chatManager;

        public CreateChatSearchContactsStrategy(ChatManager chatManager)
        {
            _chatManager = chatManager;
        }

        public Task<IList<ChatUserViewModel>> Search(string query, int pageNumber, int pageSize)
        {
            return _chatManager.GetContactsAsync(query, pageNumber, pageSize);
        }
    }
}