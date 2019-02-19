using Softeq.XToolkit.WhiteLabel.Mvvm;
using System.Collections.Generic;
using Softeq.XToolkit.Chat.Models;
using System.Threading.Tasks;
using Softeq.XToolkit.Common.Models;
using Softeq.XToolkit.Chat.Strategies.Search;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.WhiteLabel.Threading;
using System.Threading;
using Softeq.XToolkit.Common.Extensions;
using System.Windows.Input;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.WhiteLabel.Navigation;
using Softeq.XToolkit.Chat.Services;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class NewChatViewModel : ViewModelBase
    {
        private const int DefaultSearchResultsPageSize = 20;

        private readonly CreateChatSearchContactsStrategy _searchContactsStrategy;
        private readonly IPageNavigationService _pageNavigationService;

        private CancellationTokenSource _lastSearchCancelSource = new CancellationTokenSource();
        private string _searchQuery;

        public NewChatViewModel(
            IChatService chatService,
            IChatLocalizedStrings localizedStrings,
            IPageNavigationService pageNavigationService)
        {
            _searchContactsStrategy = new CreateChatSearchContactsStrategy(chatService);

            LocalizedStrings = localizedStrings;
            _pageNavigationService = pageNavigationService;

            PaginationViewModel = new PaginationViewModel<ChatUserViewModel, ChatUserModel>(
                new ChatUserViewModelFactory(),
                SearchLoader,
                SearchFilter,
                DefaultSearchResultsPageSize);

            SearchCommand = new RelayCommand(DoSearch);
            CancelCommand = new RelayCommand(GoBack);
            CreateGroupChatCommand = new RelayCommand(() => _pageNavigationService.NavigateToViewModel<CreateChatViewModel>());
            CreatePersonalChatCommand = new RelayCommand<ChatUserModel>(x =>
            {
                // TODO YP: nav to personal chat page
            });
        }

        public ICommand SearchCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CreateGroupChatCommand { get; }
        public RelayCommand<ChatUserModel> CreatePersonalChatCommand { get; }

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

        public override void OnAppearing()
        {
            base.OnAppearing();

            Task.Run(async () =>
            {
                Execute.BeginOnUIThread(() => IsBusy = true);

                await PaginationViewModel.LoadFirstPageAsync(CancellationToken.None).ConfigureAwait(false);

                Execute.BeginOnUIThread(() => IsBusy = false);
            });
        }

        private void DoSearch()
        {
            _lastSearchCancelSource?.Cancel();
            _lastSearchCancelSource = new CancellationTokenSource();
            PaginationViewModel.LoadFirstPageAsync(_lastSearchCancelSource.Token).ConfigureAwait(false);
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
    }
}
