// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Linq;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Navigation;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatDetailsViewModel : ViewModelBase
    {
        private readonly ChatManager _chatManager;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IChatLocalizedStrings _localizedStrings;
        private readonly IFormatService _formatService;

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

        public ChatSummaryModel Summary { get; set; }

        public ObservableRangeCollection<ChatUserViewModel> Members { get; } = new ObservableRangeCollection<ChatUserViewModel>();

        public string MembersCountText => _formatService.PluralizeWithQuantity(Members.Count,
            _localizedStrings.MembersPlural,
            _localizedStrings.MemberSingular);

        public ICommand AddMembersCommand { get; }
        public ICommand BackCommand { get; }
        public RelayCommand<int> RemoveMemberAtCommand { get; }

        public override async void OnAppearing()
        {
            base.OnAppearing();

            Members.Clear();

            var members = await _chatManager.GetChatMembersAsync(Summary.Id);
            Members.AddRange(members);
            RaisePropertyChanged(nameof(MembersCountText));
        }

        public bool IsMemberRemovable(int memberPosition)
        {
            if (Summary.IsCreatedByMe)
            {
                return Members[memberPosition].Id != Summary.CreatorId;
            }

            return false;
        }

        private void RemoveMemberAt(int index)
        {
            Members.RemoveAt(index);
            RaisePropertyChanged(nameof(MembersCountText));
        }

        private void AddMembers()
        {
            //TODO: Yauhen Sampir should be passed only chatId and action, users should be loaded from service
            _pageNavigationService.For<SelectContactsViewModel>()
                .WithParam(x => x.SelectedContactsAction, SelectedContactsAction.InviteToChat)
                .WithParam(x => x.FilteredUsers, Members.Select(x => x.Id).ToList())
                .WithParam(x => x.OpenedChatId, Summary.Id)
                .Navigate();
        }
    }
}