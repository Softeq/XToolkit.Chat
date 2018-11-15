// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Auth;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Permissions;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Navigation;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class SelectContactsViewModel : ViewModelBase,
        IViewModelParameter<(SelectedContactsAction, IList<string> FilteredUsers, string OpenedChatId)>
    {
        private readonly IAccountService _accountService;
        private readonly ChatManager _chatManager;
        private readonly IFormatService _formatService;
        private readonly IChatLocalizedStrings _localizedStrings;
        private readonly ICommand _memberSelectedCommand;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IUploadImageService _uploadImageService;
        private string _chatName;
        private IList<string> _filteredUsers = new List<string>();
        private string _openedChatId;

        private SelectedContactsAction _selectedContactsAction;

        public SelectContactsViewModel(
            ChatManager chatManager,
            IAccountService accountService,
            IPageNavigationService pageNavigationService,
            IChatLocalizedStrings localizedStrings,
            IFormatService formatService,
            IPermissionsManager permissionsManager,
            IUploadImageService uploadImageService)
        {
            PermissionsManager = permissionsManager;
            _chatManager = chatManager;
            _accountService = accountService;
            _pageNavigationService = pageNavigationService;
            _localizedStrings = localizedStrings;
            _formatService = formatService;
            _uploadImageService = uploadImageService;

            _memberSelectedCommand = new RelayCommand(() => RaisePropertyChanged(nameof(ContactsCountText)));

            BackCommand = new RelayCommand(_pageNavigationService.GoBack, () => _pageNavigationService.CanGoBack);

            AddChatCommand = new RelayCommand(() => AddChatAsync().SafeTaskWrapper());
        }

        public ICommand BackCommand { get; }

        [Obsolete] public ICommand AddChatCommand { get; }

        public RelayCommand<Func<(Task<Stream>, string)>> SaveCommand { get; private set; }

        public IPermissionsManager PermissionsManager { get; }

        public string Title => IsInviteToChat ? _localizedStrings.InviteUsers : _localizedStrings.CreateGroup;
        public string ActionButtonName => IsInviteToChat ? _localizedStrings.Invite : _localizedStrings.Create;

        public string ContactsCountText => _formatService.PluralizeWithQuantity(Contacts.Count(x => x.IsSelected),
            _localizedStrings.MembersPlural,
            _localizedStrings.MemberSingular);

        public ObservableRangeCollection<ChatUserViewModel> Contacts { get; } =
            new ObservableRangeCollection<ChatUserViewModel>();

        public string ChatName
        {
            get => _chatName;
            set => Set(ref _chatName, value);
        }

        public bool IsCreateChat => _selectedContactsAction == SelectedContactsAction.CreateChat;

        public bool IsInviteToChat => _selectedContactsAction == SelectedContactsAction.InviteToChat;

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

        public override void OnInitialize()
        {
            base.OnInitialize();

            SaveCommand = new RelayCommand<Func<(Task<Stream>, string)>>(SaveAsync);
        }

        public override async void OnAppearing()
        {
            base.OnAppearing();

            Contacts.Clear();

            var users = await _chatManager.GetContactsAsync().ConfigureAwait(false);
            if (users != null)
            {
                var filteredUsers = users.Where(x => x.Id != _accountService.UserId
                                                     && !_filteredUsers.Contains(x?.Id)).ToList();
                filteredUsers.Apply(x =>
                {
                    x.IsSelectable = true;
                    x.SetSelectionCommand(_memberSelectedCommand);
                });
                Contacts.AddRange(filteredUsers);
            }

            RaisePropertyChanged(nameof(ContactsCountText));
        }

        [Obsolete]
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
                    await _chatManager.CreateChatAsync(_chatName, selectedContactsIds, null).ConfigureAwait(false);
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

        private async void SaveAsync(Func<(Task<Stream> GetImageTask, string Extension)> getImageFunc)
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;

            var imageInfo = getImageFunc();
            var imagePath = default(string);

            using (var image = await imageInfo.GetImageTask.ConfigureAwait(false))
            {
                if (image != null)
                {
                    imagePath = await _uploadImageService.UploadImageAsync(image, imageInfo.Extension)
                        .ConfigureAwait(false);
                }
            }

            try
            {
                var selectedContactsIds = Contacts.Where(x => x.IsSelected).Select(x => x.Id).ToList();
                if (IsCreateChat)
                {
                    await _chatManager.CreateChatAsync(_chatName, selectedContactsIds, imagePath).ConfigureAwait(false);
                    ChatName = string.Empty;
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