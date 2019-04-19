// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Interfaces;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Navigation;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class ChatsListViewModel : ViewModelBase
    {
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IDialogsService _dialogsService;
        private readonly IChatsListManager _chatsListManager;

        public ChatsListViewModel(
            IPageNavigationService pageNavigationService,
            IDialogsService dialogsService,
            IChatLocalizedStrings localizedStrings,
            IChatsListManager chatsListManager,
            ConnectionStatusViewModel connectionStatusViewModel)
        {
            _pageNavigationService = pageNavigationService;
            _dialogsService = dialogsService;
            _chatsListManager = chatsListManager;

            LocalizedStrings = localizedStrings;
            ConnectionStatusViewModel = connectionStatusViewModel;

            Chats = _chatsListManager.ChatsCollection;

            CreateChatCommand = new RelayCommand(CreateChat);
            LeaveChatCommand = new AsyncCommand<ChatSummaryViewModel>(LeaveChatAsync);
            DeleteChatCommand = new AsyncCommand<ChatSummaryViewModel>(DeleteChatAsync);
        }

        public ICommand CreateChatCommand { get; }
        public ICommand LeaveChatCommand { get; }
        public ICommand DeleteChatCommand { get; }

        public ObservableRangeCollection<ChatSummaryViewModel> Chats { get; }

        public ConnectionStatusViewModel ConnectionStatusViewModel { get; }

        public IChatLocalizedStrings LocalizedStrings { get; }

        public ChatSummaryViewModel SelectedChat
        {
            get => null;
            set
            {
                RaisePropertyChanged();

                if (value != null)
                {
                    _pageNavigationService
                        .For<ChatMessagesViewModel>()
                        .WithParam(x => x.Parameter, value)
                        .Navigate();
                }
            }
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            ConnectionStatusViewModel.Initialize(LocalizedStrings.ChatsTitle);

            _chatsListManager.RefreshChatsListOnBackgroundAsync();
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();

            ConnectionStatusViewModel.Dispose();
        }

        private void CreateChat()
        {
            _pageNavigationService.NavigateToViewModel<NewChatViewModel>();
        }

        private async Task LeaveChatAsync(ChatSummaryViewModel chatViewModel)
        {
            var hasLeaveConfirmation = await _dialogsService.ShowDialogAsync(
                LocalizedStrings.LeaveChatConfirmationTitle,
                LocalizedStrings.LeaveChatConfirmationMessage,
                LocalizedStrings.Yes,
                LocalizedStrings.No);

            if (hasLeaveConfirmation)
            {
                await _chatsListManager.LeaveChatAsync(chatViewModel.ChatId);
            }
        }

        private Task DeleteChatAsync(ChatSummaryViewModel chatViewModel)
        {
            return _chatsListManager.CloseChatAsync(chatViewModel.ChatId);
        }
    }
}