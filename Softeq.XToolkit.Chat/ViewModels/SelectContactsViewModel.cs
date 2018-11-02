// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Navigation;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class SelectContactsViewModel : ViewModelBase,
        IViewModelParameter<(SelectedContactsAction, IList<string> FilteredUsers, string OpenedChatId)>
    {
        private readonly ChatManager _chatManager;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IChatLocalizedStrings _localizedStrings;
        private readonly IFormatService _formatService;
        private readonly ICommand _memberSelectedCommand;

        private SelectedContactsAction _selectedContactsAction;
        private IList<string> _filteredUsers = new List<string>();
        private string _openedChatId;
        private string _chatName;

        public SelectContactsViewModel(
            ChatManager chatManager,
            IPageNavigationService pageNavigationService,
            IChatLocalizedStrings localizedStrings,
            IFormatService formatService)
        {
            _chatManager = chatManager;
            _pageNavigationService = pageNavigationService;
            _localizedStrings = localizedStrings;
            _formatService = formatService;

            _memberSelectedCommand = new RelayCommand(() => RaisePropertyChanged(nameof(ContactsCountText)));
            BackCommand = new RelayCommand(_pageNavigationService.GoBack, () => _pageNavigationService.CanGoBack);
            AddChatCommand = new RelayCommand(() => AddChatAsync().SafeTaskWrapper());
        }

        public ICommand BackCommand { get; }
        public ICommand AddChatCommand { get; }

        public string Title => IsInviteToChat ? _localizedStrings.InviteUsers : _localizedStrings.CreateGroup;
        public string ActionButtonName => IsInviteToChat ? _localizedStrings.Invite : _localizedStrings.Create;
        public string ContactsCountText => _formatService.PluralizeWithQuantity(Contacts.Count(x => x.IsSelected),
                                                                                _localizedStrings.MembersPlural,
                                                                                _localizedStrings.MemberSingular);

        public ObservableRangeCollection<ChatUserViewModel> Contacts { get; } = new ObservableRangeCollection<ChatUserViewModel>();

        public (SelectedContactsAction, IList<string> FilteredUsers, string OpenedChatId) Parameter
        {
            set
            {
                _selectedContactsAction = value.Item1;
                _filteredUsers = value.FilteredUsers ?? new List<string>();
                _openedChatId = value.OpenedChatId;

                RaisePropertyChanged(nameof(ActionButtonName));
                RaisePropertyChanged(nameof(IsCreateChat));
                RaisePropertyChanged(nameof(IsInviteToChat));
            }
        }

        public string ChatName
        {
            get => _chatName;
            set => Set(ref _chatName, value);
        }

        public bool IsCreateChat => _selectedContactsAction == SelectedContactsAction.CreateChat;
        public bool IsInviteToChat => _selectedContactsAction == SelectedContactsAction.InviteToChat;

        public override async void OnAppearing()
        {
            base.OnAppearing();

            Contacts.Clear();

            var users = await _chatManager.GetContactsAsync().ConfigureAwait(false);
            if (users != null)
            {
                var filteredUsers = users.Where(x => !_filteredUsers.Contains(x?.Id)).ToList();
                filteredUsers.Apply(x =>
                {
                    x.IsSelectable = true;
                    x.SetSelectionCommand(_memberSelectedCommand);
                });
                Contacts.AddRange(filteredUsers);
            }

            RaisePropertyChanged(nameof(ContactsCountText));
        }

        private async Task AddChatAsync()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            try
            {
                var selectedContactsIds = Contacts.Where(x => x.IsSelected).Select(x => x.Id).ToList();
                if (IsCreateChat)
                {
                    await _chatManager.CreateChatAsync(_chatName, selectedContactsIds).ConfigureAwait(false);
                }
                else if (IsInviteToChat)
                {
                    await _chatManager.InviteMembersAsync(_openedChatId, selectedContactsIds).ConfigureAwait(false);
                }
                _pageNavigationService.GoBack();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
