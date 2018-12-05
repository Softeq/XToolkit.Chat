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
        private const int SearchDelayMs = 2000;
        private const int DefaultSearchResultsPageSize = 20;

        private readonly ICommand _contactSelectedCommand;

        private string _contactNameSearchQuery;
        private ISearchContactsStrategy _searchContactsStrategy;
        private CancellationTokenSource _lastSearchCancelSource = new CancellationTokenSource();
        private IReadOnlyList<ChatUserViewModel> _excludedContacts = new List<ChatUserViewModel>();

        public AddContactsViewModel(
            IChatLocalizedStrings chatLocalizedStrings,
            IViewModelFactoryService viewModelFactoryService)
        {
            Resources = chatLocalizedStrings;

            PaginationViewModel = new PaginationViewModel<ChatUserViewModel, ChatUserModel>(
                viewModelFactoryService,
                SearchLoader,
                SearchFilter,
                DefaultSearchResultsPageSize);

            _contactSelectedCommand = new RelayCommand<ChatUserViewModel>(SwitchSelectedContact);
            SearchContactCommand = new RelayCommand<string>(DoSearch);
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
                if (Set(ref _contactNameSearchQuery, value))
                {
                    SearchContactCommand.Execute(value);
                }
            }
        }

        public ObservableRangeCollection<ChatUserViewModel> SelectedContacts { get; } =
            new ObservableRangeCollection<ChatUserViewModel>();

        public bool HasSelectedContacts => SelectedContacts.Count > 0;

        public override async void OnAppearing()
        {
            base.OnAppearing();

            await PaginationViewModel.LoadFirstPageAsync();
        }

        private async void DoSearch(string query)
        {
            _lastSearchCancelSource?.Cancel();
            _lastSearchCancelSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(SearchDelayMs, _lastSearchCancelSource.Token);

                await PaginationViewModel.LoadFirstPageAsync();
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
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
                .Contains(x.Id)
            ).ToList();

            ApplySelectionCommand(filteredContacts, true);

            return filteredContacts;
        }

        private void ApplySelectionCommand(IEnumerable<ChatUserViewModel> contacts, bool isSelectable)
        {
            contacts.Apply(x =>
            {
                x.IsSelectable = isSelectable;
                x.SetSelectionCommand(_contactSelectedCommand);
            });
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