// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Navigation;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatDetailsViewModel : ViewModelBase, IViewModelParameter<ChatSummaryViewModel>
    {
        private readonly ChatManager _chatManager;
        private readonly IPageNavigationService _pageNavigationService;

        private ChatSummaryViewModel _chatSummaryViewModel;

        public ChatDetailsViewModel(ChatManager chatManager, IPageNavigationService pageNavigationService)
        {
            _chatManager = chatManager;
            _pageNavigationService = pageNavigationService;

            AddMembersCommand = new RelayCommand(AddMembers);
        }

        public ChatSummaryViewModel Parameter { set => _chatSummaryViewModel = value; }

        public ObservableRangeCollection<ChatUserViewModel> Members { get; }
                = new ObservableRangeCollection<ChatUserViewModel>();

        public string ChatAvatarUrl => _chatSummaryViewModel.ChatPhotoUrl;
        public string ChatName => _chatSummaryViewModel.ChatName;
        public string MembersCountText => $"{Members.Count} member" + (Members.Count == 1 ? string.Empty : "s");

        public bool IsNavigated { get; private set; }

        public ICommand AddMembersCommand { get; }

        public async override void OnAppearing()
        {
            base.OnAppearing();

            Members.Clear();

            var members = await _chatManager.GetChatMembersAsync(_chatSummaryViewModel.ChatId);
            Members.AddRange(members);
            RaisePropertyChanged(nameof(MembersCountText));
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();

            IsNavigated = false;
        }

        public override void OnNavigated()
        {
            base.OnNavigated();

            IsNavigated = true;
        }

        private void AddMembers()
        {
            _pageNavigationService.NavigateToViewModel<SelectContactsViewModel,
                (SelectedContactsAction, IList<string> FilteredUsers, string OpenedChatId)>(
                (SelectedContactsAction.InviteToChat, Members.Select(x => x.Id).ToList(), _chatSummaryViewModel.ChatId));
        }
    }
}
