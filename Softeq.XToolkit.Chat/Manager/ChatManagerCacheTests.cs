// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using System.Linq;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Interfaces;

namespace Softeq.XToolkit.Chat.Manager
{
    public static class ChatManagerCacheTests
    {
        public static void UpdateCollectionTest()
        {
            var viewModelFactoryService = Dependencies.Container.Resolve<IViewModelFactoryService>();

            ChatMessageViewModel CreateViewModel(ChatMessageModel model)
            {
                var vm = viewModelFactoryService.ResolveViewModel<ChatMessageViewModel>();
                vm.UpdateMessageModel(model);
                return vm;
            }

            var col1 = new ObservableRangeCollection<ChatMessageViewModel>
            {
                CreateViewModel(new ChatMessageModel { Body = "1", Id = "1" }),
                CreateViewModel(new ChatMessageModel { Body = "2", Id = "2" }),
                null,
                null,
                null,
                CreateViewModel(new ChatMessageModel { Body = "3", Id = "3" }),
            };

            var col2 = new List<ChatMessageViewModel>
            {
                CreateViewModel(new ChatMessageModel { Body = "22", Id = "2" }),
                null,
                CreateViewModel(new ChatMessageModel { Body = "333", Id = "3" }),
                CreateViewModel(new ChatMessageModel { Body = "4", Id = "4" }),
            };

            var messagesToDelete = col1.Except(col2).ToList();
            messagesToDelete.AddRange(col1.Where(x => x == null));
            col1.RemoveRange(messagesToDelete);

            col1.Apply(x => x.UpdateMessage(col2.FirstOrDefault(x.Equals)));

            col1.AddRange(col2.Except(col1).Where(x => x != null));
        }
    }
}
