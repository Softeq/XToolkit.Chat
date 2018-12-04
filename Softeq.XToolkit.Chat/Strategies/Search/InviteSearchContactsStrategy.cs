// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.WhiteLabel.Interfaces;

namespace Softeq.XToolkit.Chat.Strategies.Search
{
    public class InviteSearchContactsStrategy : ISearchContactsStrategy
    {
        private readonly IChatService _chatService;
        private readonly IViewModelFactoryService _viewModelFactoryService;
        private readonly string _chatId;

        public InviteSearchContactsStrategy(
            IChatService chatService,
            IViewModelFactoryService viewModelFactoryService,
            string chatId)
        {
            _chatService = chatService;
            _viewModelFactoryService = viewModelFactoryService;
            _chatId = chatId;
        }

        public async Task<IList<ChatUserViewModel>> Search(string query, int pageNumber, int pageSize)
        {
            var models = await _chatService.GetContactsForInviteAsync(_chatId, query, pageNumber, pageSize).ConfigureAwait(false);

            if (models == null)
            {
                return new List<ChatUserViewModel>();
            }
            return models.Data.Select(_viewModelFactoryService.ResolveViewModel<ChatUserViewModel, ChatUserModel>).ToList();
        }
    }
}