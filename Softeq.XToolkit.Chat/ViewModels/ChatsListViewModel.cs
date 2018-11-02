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
using Softeq.XToolkit.WhiteLabel;
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

        private ISampleChatLoginService _loginService;
        private string _userName;
        private bool _isReloginButtonVisible = true;

        private List<IDisposable> _subscriptions = new List<IDisposable>();

        public ChatsListViewModel(
            IDialogsService dialogsService,
            IPageNavigationService pageNavigationService,
            IChatLocalizedStrings localizedStrings,
            ChatManager chatManager,
            ConnectionStatusViewModel connectionStatusViewModel)
        {
            TryInitLoginService();

            _dialogsService = dialogsService;
            _pageNavigationService = pageNavigationService;
            _localizedStrings = localizedStrings;
            _chatManager = chatManager;

            _userName = _localizedStrings.NotLoggedIn;

            ConnectionStatusViewModel = connectionStatusViewModel;

            Chats = _chatManager.ChatsCollection;

            CreateChatCommand = new RelayCommand(CreateChat);
            LeaveChatCommand = new RelayCommand<ChatSummaryViewModel>((x) => LeaveChatAsync(x).SafeTaskWrapper());
            DeleteChatCommand = new RelayCommand<ChatSummaryViewModel>((x) => DeleteChatAsync(x).SafeTaskWrapper());

            LoginCommand = new RelayCommand(Login);

            if (_loginService != null && _loginService.IsAuthorized)
            {
                UserName = _loginService.Username;
            }
        }

        public ICommand LoginCommand { get; private set; }
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
                    _pageNavigationService.NavigateToViewModel<ChatMessagesViewModel, ChatSummaryViewModel>(value);
                }
            }
        }

        public ConnectionStatusViewModel ConnectionStatusViewModel { get; }

        public string DeleteChatOptionText => _localizedStrings.Close;
        public string LeaveChatOptionText => _localizedStrings.Leave;

        public string UserName
        {
            get => _userName;
            set => Set(ref _userName, value);
        }

        public bool IsReloginButtonVisible
        {
            get => _isReloginButtonVisible;
            set => Set(ref _isReloginButtonVisible, value);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            _subscriptions.Add(_chatManager.ConnectionStatusChanged.Subscribe(OnConnectionStatusChanged));
            OnConnectionStatusChanged(_chatManager.ConnectionStatus);
        }

        public override void OnDisappearing()
        {
            base.OnDisappearing();
            _subscriptions.Apply(x => x.Dispose());
        }

        private async void Login()
        {
            if (_loginService != null)
            {
                var username = await _loginService.LoginAsync();
                if (username != null)
                {
                    UserName = username;
                    Chats.Clear();
                    _chatManager.ForceReconnect();
                }
            }
        }

        private void CreateChat()
        {
            _pageNavigationService.NavigateToViewModel<SelectContactsViewModel,
                (SelectedContactsAction, IList<string> FilteredUsers, string OpenedChatId)>(
                (SelectedContactsAction.CreateChat, null, null));
        }

        private Task LeaveChatAsync(ChatSummaryViewModel chatViewModel)
        {
            return _chatManager.LeaveChatAsync(chatViewModel.ChatId);
        }

        private Task DeleteChatAsync(ChatSummaryViewModel chatViewModel)
        {
            return _chatManager.CloseChatAsync(chatViewModel.ChatId);
        }

        private void TryInitLoginService()
        {
            if (ServiceLocator.IsRegistered<ISampleChatLoginService>())
            {
                _loginService = ServiceLocator.Resolve<ISampleChatLoginService>();
            }
            else
            {
                UserName = string.Empty;
                IsReloginButtonVisible = false;
            }
        }

        private void OnConnectionStatusChanged(ConnectionStatus status)
        {
            ConnectionStatusViewModel.UpdateConnectionStatus(status, _localizedStrings.ChatsTitle);
            RaisePropertyChanged(nameof(ConnectionStatusViewModel));
        }
    }
}
