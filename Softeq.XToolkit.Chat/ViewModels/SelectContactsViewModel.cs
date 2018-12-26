﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Interfaces;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.Strategies.Search;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Commands;
using Softeq.XToolkit.WhiteLabel.Model;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Navigation;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.ViewModels
{
    // TODO YP: rename to CreateChatViewModel or merge with ChatDetailsViewModel
    public class SelectContactsViewModel : ViewModelBase
    {
        private readonly IChatsListManager _chatsListManager;
        private readonly IChatService _chatService;
        private readonly IFormatService _formatService;
        private readonly IChatLocalizedStrings _localizedStrings;
        private readonly ICommand _memberSelectedCommand;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IUploadImageService _uploadImageService;
        private readonly IDialogsService _dialogsService;

        private string _chatName;

        public SelectContactsViewModel(
            IChatsListManager chatsListManager,
            IChatService chatService,
            IPageNavigationService pageNavigationService,
            IChatLocalizedStrings localizedStrings,
            IFormatService formatService,
            IUploadImageService uploadImageService,
            IDialogsService dialogsService)
        {
            _chatsListManager = chatsListManager;
            _chatService = chatService;
            _pageNavigationService = pageNavigationService;
            _localizedStrings = localizedStrings;
            _formatService = formatService;
            _uploadImageService = uploadImageService;
            _dialogsService = dialogsService;
            _memberSelectedCommand = new RelayCommand(() => RaisePropertyChanged(nameof(ContactsCountText)));

            BackCommand = new RelayCommand(_pageNavigationService.GoBack, () => _pageNavigationService.CanGoBack);
            AddMembersCommand = new AsyncCommand(OpenDialogForAddMembersAsync);
        }

        public ICommand BackCommand { get; }

        public ICommand AddMembersCommand { get; }

        public ICommand SaveCommand { get; private set; }

        public string Title => _localizedStrings.CreateGroup;

        public string ActionButtonName => _localizedStrings.Create;

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

        public string AddMembersText => _localizedStrings.AddMembers;

        public override void OnInitialize()
        {
            base.OnInitialize();

            SaveCommand = new AsyncCommand<Func<(Task<Stream>, string)>>(SaveAsync);
        }

        private async Task OpenDialogForAddMembersAsync()
        {
            Contacts.Clear();

            var result = await _dialogsService.ShowForViewModel<AddContactsViewModel, AddContactParameters>(
                new AddContactParameters
                {
                    SelectedContacts = Contacts,
                    SelectionType = SelectedContactsAction.CreateChat,
                    SearchStrategy = new CreateChatSearchContactsStrategy(_chatService)
                },
                new OpenDialogOptions
                {
                    ShouldShowBackgroundOverlay = false
                });

            if (result != null)
            {
                var contacts = result.SelectedContacts;
                contacts.Apply(x => x.SetSelectionCommand(_memberSelectedCommand));
                Contacts.AddRange(contacts);

                RaisePropertyChanged(nameof(ContactsCountText));
            }
        }

        private async Task SaveAsync(Func<(Task<Stream> GetImageTask, string Extension)> getImageFunc)
        {
            if (IsBusy || string.IsNullOrEmpty(ChatName))
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
                await _chatsListManager.CreateChatAsync(ChatName, selectedContactsIds, imagePath).ConfigureAwait(false);

                Execute.BeginOnUIThread(() =>
                {
                    ChatName = string.Empty;

                    _pageNavigationService.GoBack();
                });
            }
            catch (Exception ex)
            {
                LogManager.LogError<SelectContactsViewModel>(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}