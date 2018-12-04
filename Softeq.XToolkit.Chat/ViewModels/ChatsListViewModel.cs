// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.Chat.Models.Enum;
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
        private readonly IDialogsService _dialogsService;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly IChatLocalizedStrings _localizedStrings;
        private readonly ChatManager _chatManager;

        private List<IDisposable> _subscriptions = new List<IDisposable>();

        public ChatsListViewModel(
            IDialogsService dialogsService,
            IPageNavigationService pageNavigationService,
            IChatLocalizedStrings localizedStrings,
            ChatManager chatManager,
            ConnectionStatusViewModel connectionStatusViewModel)
        {
            _dialogsService = dialogsService;
            _pageNavigationService = pageNavigationService;
            _localizedStrings = localizedStrings;
            _chatManager = chatManager;

            ConnectionStatusViewModel = connectionStatusViewModel;

            Chats = _chatManager.ChatsCollection;

            CreateChatCommand = new RelayCommand(CreateChat);
            LeaveChatCommand = new RelayCommand<ChatSummaryViewModel>((x) => LeaveChatAsync(x).SafeTaskWrapper());
            DeleteChatCommand = new RelayCommand<ChatSummaryViewModel>((x) => DeleteChatAsync(x).SafeTaskWrapper());
        }

        public ICommand CreateChatCommand { get; }
        public ICommand LeaveChatCommand { get; }
        public ICommand DeleteChatCommand { get; }

        public ObservableRangeCollection<ChatSummaryViewModel> Chats { get; }

        public ChatSummaryViewModel SelectedChat
        {
            get => null;
            set
            {
                RaisePropertyChanged();
                if (value != null)
                {
                    _pageNavigationService.For<ChatMessagesViewModel>().WithParam(x => x.Parameter, value).Navigate();
                }
            }
        }

        public ConnectionStatusViewModel ConnectionStatusViewModel { get; }

        public string DeleteChatOptionText => _localizedStrings.Close;
        public string LeaveChatOptionText => _localizedStrings.Leave;

        public override void OnAppearing()
        {
            base.OnAppearing();

            _chatManager.ForceReconnect();

            _subscriptions.Add(_chatManager.ConnectionStatusChanged.Subscribe(OnConnectionStatusChanged));
            OnConnectionStatusChanged(_chatManager.ConnectionStatus);
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            _subscriptions.Apply(x => x.Dispose());
        }

        private void CreateChat()
        {
            _pageNavigationService.NavigateToViewModel<SelectContactsViewModel>();
        }

        private Task LeaveChatAsync(ChatSummaryViewModel chatViewModel)
        {
            return _chatManager.LeaveChatAsync(chatViewModel.ChatId);
        }

        private Task DeleteChatAsync(ChatSummaryViewModel chatViewModel)
        {
            return _chatManager.CloseChatAsync(chatViewModel.ChatId);
        }

        private void OnConnectionStatusChanged(ConnectionStatus status)
        {
            ConnectionStatusViewModel.UpdateConnectionStatus(status, _localizedStrings.ChatsTitle);
            RaisePropertyChanged(nameof(ConnectionStatusViewModel));
        }
    }
}