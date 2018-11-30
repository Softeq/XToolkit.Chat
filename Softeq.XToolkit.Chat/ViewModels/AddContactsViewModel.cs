using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.WhiteLabel.Mvvm;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class AddContactsViewModel : DialogViewModelBase
    {
        private const int SearchDelayMs = 2000;

        private readonly ChatManager _chatManager;
        private readonly ICommand _memberSelectedCommand;

        private string _userNameSearchQuery;
        private CancellationTokenSource _lastSearchCancelSource = new CancellationTokenSource();

        public AddContactsViewModel(
            ChatManager chatManager)
        {
            _chatManager = chatManager;

            _memberSelectedCommand = new RelayCommand<ChatUserViewModel>(SwitchSelectedMember);

            SearchMemberCommand = new RelayCommand<string>(DoSearch);

            CancelCommand = new RelayCommand(() =>
            {
                DialogComponent.CloseCommand.Execute(false);
            });

            DoneCommand = new RelayCommand(() =>
            {
                DialogComponent.CloseCommand.Execute(true);
            });

            RemoveSelectedMemberCommand = _memberSelectedCommand; // TODO:
        }

        public ICommand CancelCommand { get; }
        public ICommand SearchMemberCommand { get; }
        public ICommand DoneCommand { get; }
        public ICommand RemoveSelectedMemberCommand { get; }

        public string UserNameSearchQuery
        {
            get => _userNameSearchQuery;
            set
            {
                Set(ref _userNameSearchQuery, value);

                SearchMemberCommand.Execute(value);
            }
        }

        public ObservableRangeCollection<ChatUserViewModel> SelectedContacts { get; } =
            new ObservableRangeCollection<ChatUserViewModel>();

        public ObservableRangeCollection<ChatUserViewModel> FoundContacts { get; } =
            new ObservableRangeCollection<ChatUserViewModel>();

        public override async void OnAppearing()
        {
            base.OnAppearing();

            await LoadMembersAsync("");
        }

        private async void DoSearch(string query)
        {
            _lastSearchCancelSource?.Cancel();
            _lastSearchCancelSource = new CancellationTokenSource();

            try
            {
                await Task.Delay(SearchDelayMs, _lastSearchCancelSource.Token);

                await LoadMembersAsync(query);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
        }

        private async Task LoadMembersAsync(string query)
        {
            FoundContacts.Clear();

            var users = await _chatManager.GetContactsAsync(query, 1, 20).ConfigureAwait(false);
            if (users != null)
            {
                var selectedUserIds = SelectedContacts.Select(x => x.Id).ToList();
                var filteredUsers = users.Where(x => !selectedUserIds.Contains(x.Id)).ToList();
                filteredUsers.Apply(x =>
                {
                    x.IsSelectable = true;
                    x.SetSelectionCommand(_memberSelectedCommand);
                });
                FoundContacts.AddRange(filteredUsers);
            }
        }

        private void SwitchSelectedMember(ChatUserViewModel viewModel)
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
        }
    }
}