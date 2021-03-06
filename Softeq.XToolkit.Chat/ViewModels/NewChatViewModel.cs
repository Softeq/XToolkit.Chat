// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Interfaces;
using Softeq.XToolkit.Chat.Messages;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.Services;
using Softeq.XToolkit.Chat.Strategies.Search;
using Softeq.XToolkit.Common.Commands;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Common.Logger;
using Softeq.XToolkit.WhiteLabel.Messenger;
using Softeq.XToolkit.WhiteLabel.Model;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.WhiteLabel.Navigation;
using Softeq.XToolkit.WhiteLabel.Threading;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class NewChatViewModel : ViewModelBase
    {
        private const int DefaultSearchResultsPageSize = 20;

        private readonly CreateChatSearchContactsStrategy _searchContactsStrategy;
        private readonly IChatsListManager _chatsListManager;
        private readonly IPageNavigationService _pageNavigationService;
        private readonly ILogger _logger;

        private CancellationTokenSource _lastSearchCancelSource = new CancellationTokenSource();
        private string _searchQuery;
        private bool _hasResults;

        public NewChatViewModel(
            IChatService chatService,
            IChatsListManager chatsListManager,
            IChatLocalizedStrings localizedStrings,
            IPageNavigationService pageNavigationService,
            ILogManager logManager)
        {
            _searchContactsStrategy = new CreateChatSearchContactsStrategy(chatService);
            _chatsListManager = chatsListManager;
            LocalizedStrings = localizedStrings;
            _pageNavigationService = pageNavigationService;
            _logger = logManager.GetLogger<NewChatViewModel>();
            NoResultVisible = false;

            PaginationViewModel = new PaginationViewModel<ChatUserViewModel, ChatUserModel>(
                new ChatUserViewModelFactory(),
                SearchLoader,
                SearchFilter,
                DefaultSearchResultsPageSize);
            SearchCommand = new AsyncCommand(DoSearch);
            CancelCommand = new RelayCommand(GoBack);
            CreateGroupChatCommand = new RelayCommand(() => _pageNavigationService.NavigateToViewModel<CreateChatViewModel>());
            CreatePersonalChatCommand = new RelayCommand<ChatUserViewModel>(CreatePersonalChat);
        }

        public ICommand SearchCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CreateGroupChatCommand { get; }
        public RelayCommand<ChatUserViewModel> CreatePersonalChatCommand { get; }

        public IChatLocalizedStrings LocalizedStrings { get; }
        public PaginationViewModel<ChatUserViewModel, ChatUserModel> PaginationViewModel { get; }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (Set(ref _searchQuery, value))
                {
                    SearchCommand.Execute(value);
                }
            }
        }

        public bool NoResultVisible
        {
            get => _hasResults;
            set
            {
                Set(ref _hasResults, value);
            }
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            Task.Run(async () =>
            {
                Execute.BeginOnUIThread(() => IsBusy = true);

                await PaginationViewModel.LoadFirstPageAsync(CancellationToken.None).ConfigureAwait(false);

                Execute.BeginOnUIThread(() =>
                {
                    NoResultVisible = PaginationViewModel.Items.Count == 0;
                    IsBusy = false;
                });
            });
        }

        private async Task DoSearch()
        {
            _lastSearchCancelSource?.Cancel();
            _lastSearchCancelSource = new CancellationTokenSource();
            await PaginationViewModel.LoadFirstPageAsync(_lastSearchCancelSource.Token).ConfigureAwait(false);
            Execute.BeginOnUIThread(() => NoResultVisible = PaginationViewModel.Items.Count == 0);
        }

        private Task<PagingModel<ChatUserModel>> SearchLoader(int pageNumber, int pageSize)
        {
            return _searchContactsStrategy.Search(_searchQuery, pageNumber, pageSize);
        }

        private IReadOnlyList<ChatUserViewModel> SearchFilter(IReadOnlyList<ChatUserViewModel> contacts)
        {
            contacts.Apply(x =>
            {
                x.IsSelectable = false;
            });

            return contacts;
        }

        private void GoBack()
        {
            if (_pageNavigationService.CanGoBack)
            {
                _pageNavigationService.GoBack();
            }
        }

        private async void CreatePersonalChat(ChatUserViewModel chatUserModelView)
        {
            if (IsBusy)
            {
                return;
            }

            Execute.BeginOnUIThread(() => IsBusy = true);

            var chatViewModel = await _chatsListManager.FindOrCreateDirectChatAsync(chatUserModelView.Id).ConfigureAwait(false);

            if (chatViewModel == null)
            {
                _logger.Error($"Attempt to create a direct chat with myself (id={chatUserModelView.Id})");
            }
            else
            {
                Messenger.Default.Send(new OpenNewChatMessage(chatViewModel));
            }

            Execute.BeginOnUIThread(() => IsBusy = false);
        }
    }
}
