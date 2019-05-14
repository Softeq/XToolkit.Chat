// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.Strategies.Search;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Common.Models;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;
using Softeq.XToolkit.Auth;
using Softeq.XToolkit.WhiteLabel.Threading;
using Softeq.XToolkit.Chat.Services;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class AddContactParameters
    {
        public IReadOnlyList<ChatUserViewModel> SelectedContacts { get; set; }
        public SelectedContactsAction SelectionType { get; set; }
        public ISearchContactsStrategy SearchStrategy { get; set; }
    }

    public class AddContactsViewModel : DialogViewModelBase, IViewModelParameter<AddContactParameters>
    {
        private const int DefaultSearchResultsPageSize = 20;

        private readonly ICommand _contactSelectedCommand;
        private readonly IAccountService _accountService;

        private string _contactNameSearchQuery;
        private ISearchContactsStrategy _searchContactsStrategy;
        private CancellationTokenSource _lastSearchCancelSource = new CancellationTokenSource();
        private IReadOnlyList<ChatUserViewModel> _excludedContacts = new List<ChatUserViewModel>();

        public AddContactsViewModel(
            IAccountService accountService,
            IChatLocalizedStrings chatLocalizedStrings)
        {
            _accountService = accountService;
            Resources = chatLocalizedStrings;

            PaginationViewModel = new PaginationViewModel<ChatUserViewModel, ChatUserModel>(
                new ChatUserViewModelFactory(),
                SearchLoader,
                SearchFilter,
                DefaultSearchResultsPageSize);

            _contactSelectedCommand = new RelayCommand<ChatUserViewModel>(SwitchSelectedContact);
            SearchContactCommand = new RelayCommand(DoSearch);
            CancelCommand = new RelayCommand(() => DialogComponent.CloseCommand.Execute(false));
            DoneCommand = new RelayCommand(() => DialogComponent.CloseCommand.Execute(true));
        }

        public ICommand SearchContactCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DoneCommand { get; }

        public IChatLocalizedStrings Resources { get; }

        public AddContactParameters Parameter
        {
            get => null;
            set
            {
                _excludedContacts = value.SelectedContacts;
                _searchContactsStrategy = value.SearchStrategy;

                Title = value.SelectionType == SelectedContactsAction.CreateChat
                    ? Resources.AddMembers
                    : Resources.InviteUsers;
            }
        }

        public PaginationViewModel<ChatUserViewModel, ChatUserModel> PaginationViewModel { get; }

        public string Title { get; private set; }

        public string ContactNameSearchQuery
        {
            get => _contactNameSearchQuery;
            set
            {
                if (_contactNameSearchQuery == value)
                {
                    return;
                }

                _contactNameSearchQuery = value;
                RaisePropertyChanged();
                SearchContactCommand.Execute(value);
            }
        }

        public ObservableRangeCollection<ChatUserViewModel> SelectedContacts { get; } =
            new ObservableRangeCollection<ChatUserViewModel>();

        public bool HasSelectedContacts => SelectedContacts.Count > 0;

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
            return _searchContactsStrategy.Search(_contactNameSearchQuery, pageNumber, pageSize);
        }

        private IReadOnlyList<ChatUserViewModel> SearchFilter(IReadOnlyList<ChatUserViewModel> contacts)
        {
            var filteredContacts = contacts.Where(x => !SelectedContacts
                .Concat(_excludedContacts)
                .Select(c => c.Id)
                .Concat(new[] { _accountService.UserId })
                .Contains(x.Id)
            ).ToList();

            filteredContacts.Apply(x =>
            {
                x.IsSelectable = x.IsActive;
                x.SetSelectionCommand(_contactSelectedCommand);
            });

            return filteredContacts;
        }

        private void SwitchSelectedContact(ChatUserViewModel viewModel)
        {
            if (viewModel.IsSelected)
            {
                SelectedContacts.Add(viewModel);
            }
            else
            {
                if (SelectedContacts.Contains(viewModel))
                {
                    SelectedContacts.Remove(viewModel);
                }
            }

            RaisePropertyChanged(nameof(HasSelectedContacts));
        }
    }
}