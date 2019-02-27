// Developed by Softeq Development Corporation
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
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Model;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Navigation;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class CreateChatViewModel : ViewModelBase
    {
        private readonly IChatsListManager _chatsListManager;
        private readonly IChatService _chatService;
        private readonly IFormatService _formatService;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IUploadImageService _uploadImageService;
        private readonly IDialogsService _dialogsService;

        private ICommand _updateContactsCountCommand;
        private string _chatName;

        public CreateChatViewModel(
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
            _formatService = formatService;
            _uploadImageService = uploadImageService;
            _dialogsService = dialogsService;
            LocalizedStrings = localizedStrings;
        }

        public IChatLocalizedStrings LocalizedStrings { get; }

        public ICommand BackCommand { get; private set; }

        public ICommand AddMembersCommand { get; private set; }

        public ICommand SaveCommand { get; private set; }

        public string ContactsCountText => _formatService.PluralizeWithQuantity(Contacts.Count(x => x.IsSelected),
            LocalizedStrings.MembersPlural,
            LocalizedStrings.MemberSingular);

        public ObservableRangeCollection<ChatUserViewModel> Contacts { get; } =
            new ObservableRangeCollection<ChatUserViewModel>();

        public string ChatName
        {
            get => _chatName;
            set => Set(ref _chatName, value);
        }

        public override void OnInitialize()
        {
            base.OnInitialize();

            _updateContactsCountCommand = new RelayCommand(() => RaisePropertyChanged(nameof(ContactsCountText)));

            BackCommand = new RelayCommand(_pageNavigationService.GoBack, () => _pageNavigationService.CanGoBack);
            AddMembersCommand = new RelayCommand(OpenDialogForAddMembers);
            SaveCommand = new RelayCommand<Func<(Task<Stream>, string)>>(SaveAsync);
        }

        private async void OpenDialogForAddMembers()
        {
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
                contacts.Apply(x => x.SetSelectionCommand(_updateContactsCountCommand));
                Contacts.AddRange(contacts);

                _updateContactsCountCommand.Execute(null);
            }
        }

        private async void SaveAsync(Func<(Task<Stream> GetImageTask, string Extension)> getImageFunc)
        {
            if (IsBusy || string.IsNullOrEmpty(ChatName))
            {
                return;
            }

            IsBusy = true;

            var imagePath = await _uploadImageService.UploadImageAsync(getImageFunc);

            try
            {
                var selectedContactsIds = Contacts.Where(x => x.IsSelected).Select(x => x.Id).ToList();
                var isChatCreated = await _chatsListManager.CreateChatAsync(ChatName, selectedContactsIds, imagePath).ConfigureAwait(false);

                if (!isChatCreated)
                {
                    await _dialogsService.ShowDialogAsync(LocalizedStrings.ValidationErrorsDialogTitle,
                        string.Empty, LocalizedStrings.Ok);
                    return;
                }

                Execute.BeginOnUIThread(() =>
                {
                    ChatName = string.Empty;

                    _pageNavigationService.GoBack();
                });
            }
            catch (Exception ex)
            {
                LogManager.LogError<CreateChatViewModel>(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}