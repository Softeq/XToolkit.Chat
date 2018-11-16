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
using Softeq.XToolkit.Chat.Models.Interfaces;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatDetailsViewModel : ViewModelBase, IViewModelParameter<ChatSummaryViewModel>
    {
        private readonly ChatManager _chatManager;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IChatLocalizedStrings _localizedStrings;
        private readonly IFormatService _formatService;
        private ChatSummaryViewModel _chatSummaryViewModel;

        public ChatDetailsViewModel(
            ChatManager chatManager,
            IPageNavigationService pageNavigationService,
            IChatLocalizedStrings localizedStrings,
            IFormatService formatService)
        {
            _chatManager = chatManager;
            _pageNavigationService = pageNavigationService;
            _localizedStrings = localizedStrings;
            _formatService = formatService;

            AddMembersCommand = new RelayCommand(AddMembers);
            BackCommand = new RelayCommand(_pageNavigationService.GoBack, () => _pageNavigationService.CanGoBack);
            RemoveMemberAtCommand = new RelayCommand<int>(RemoveMemberAt);
        }

        public string Title => _localizedStrings.DetailsTitle;
        public string RemoveLocalizedString => _localizedStrings.Remove;

        public ChatSummaryViewModel Parameter
        {
            set => _chatSummaryViewModel = value;
        }

        public ObservableRangeCollection<ChatUserViewModel> Members { get; }
            = new ObservableRangeCollection<ChatUserViewModel>();

        public string ChatAvatarUrl => _chatSummaryViewModel.ChatPhotoUrl;
        public string ChatName => _chatSummaryViewModel.ChatName;

        public string MembersCountText => _formatService.PluralizeWithQuantity(Members.Count,
            _localizedStrings.MembersPlural,
            _localizedStrings.MemberSingular);

        public bool IsNavigated { get; private set; }

        public ICommand AddMembersCommand { get; }
        public ICommand BackCommand { get; }
        public RelayCommand<int> RemoveMemberAtCommand { get; }
        public bool IsEditAvailable => _chatSummaryViewModel.IsCreatedByMe;

        public override async void OnAppearing()
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

        public bool IsMemberRemovable(int index)
        {
            return Members[index].Id != _chatSummaryViewModel.CreatorId;
        }

        public override void OnNavigated()
        {
            base.OnNavigated();

            IsNavigated = true;
        }

        private void RemoveMemberAt(int index)
        {
            Members.RemoveAt(index);
            RaisePropertyChanged(nameof(MembersCountText));
        }

        private void AddMembers()
        {
            _pageNavigationService.NavigateToViewModel<SelectContactsViewModel,
                (SelectedContactsAction, IList<string> FilteredUsers, string OpenedChatId)>(
                (SelectedContactsAction.InviteToChat, Members.Select(x => x.Id).ToList(), _chatSummaryViewModel.ChatId));
        }
    }
}