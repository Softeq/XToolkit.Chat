// Developed by Softeq Development Corporation
// http://www.softeq.com

using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Bindings.Droid;
using Softeq.XToolkit.Chat.Droid.ViewHolders;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Droid.Dialogs;
using Softeq.XToolkit.Chat.Droid.Listeners;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Command;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    public class AddContactsDialogFragment : DialogFragmentBase<AddContactsViewModel>
    {
        private NavigationBarView _navigationBarView;
        private EditText _editText;
        private RecyclerView _addedMembers;
        private RecyclerView _filteredMembers;
        private ObservableRecyclerViewAdapter<ChatUserViewModel> _filteredAdapter;
        private ObservableRecyclerViewAdapter<ChatUserViewModel> _selectedAdapter;

        protected override int ThemeId => Resource.Style.CoreDialogTheme;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.dialog_select_members, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            View.SetBackgroundResource(StyleHelper.Style.ContentColor);

            InitNavigationBarView(view);

            var searchContainer = View.FindViewById<View>(Resource.Id.dialog_select_members_serch_container);
            searchContainer.SetBackgroundResource(StyleHelper.Style.UnderlinedBg);

            _editText = View.FindViewById<EditText>(Resource.Id.dialog_select_members_serch_text);
            _editText.Hint = ViewModel.Resources.Search;

            InitSelectedRecyclerView(view);
            InitFilteredRecyclerView(view);
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ContactNameSearchQuery, () => _editText.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.HasSelectedContacts).WhenSourceChanges(() =>
            {
                _addedMembers.Visibility = ViewModel.HasSelectedContacts
                    ? ViewStates.Visible
                    : ViewStates.Gone;
            }));
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            _filteredAdapter.Dispose();
            _selectedAdapter.Dispose();
        }

        private void InitNavigationBarView(View view)
        {
            _navigationBarView = view.FindViewById<NavigationBarView>(Resource.Id.dialog_select_members_nav_bar);
            _navigationBarView.SetLeftButton(StyleHelper.Style.NavigationBarBackButtonIcon, new RelayCommand(Close));
            _navigationBarView.SetRightButton(ViewModel.Resources.Done, ViewModel.DoneCommand);
            _navigationBarView.SetTitle(ViewModel.Title);
            _navigationBarView.SetBackground(StyleHelper.Style.ContentColor);
        }

        private void InitSelectedRecyclerView(View view)
        {
            _addedMembers = view.FindViewById<RecyclerView>(Resource.Id.dialog_select_members_added_members);
            _addedMembers.SetLayoutManager(new LinearLayoutManager(Context, LinearLayoutManager.Horizontal, false));
            _selectedAdapter = new ObservableRecyclerViewAdapter<ChatUserViewModel>(
                ViewModel.SelectedContacts,
                (parent, i) =>
                {
                    var v = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.view_holder_member, parent, false);
                    return new SelectedContactViewHolder(v);
                },
                (holder, i, item) =>
                {
                    var viewHolder = (SelectedContactViewHolder)holder;
                    viewHolder.Bind(item);
                });
            _addedMembers.SetAdapter(_selectedAdapter);
        }

        private void InitFilteredRecyclerView(View view)
        {
            _filteredMembers = view.FindViewById<RecyclerView>(Resource.Id.dialog_select_members_filtered_members);
            _filteredMembers.SetLayoutManager(new LinearLayoutManager(Context));
            _filteredMembers.AddItemDecoration(new DividerItemDecoration(Context, DividerItemDecoration.Vertical));
            _filteredAdapter = new ObservableRecyclerViewAdapter<ChatUserViewModel>(
                ViewModel.PaginationViewModel.Items,
                (parent, i) =>
                {
                    var v = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.view_holder_member_filter_item, parent, false);
                    return new FilteredItemViewHolder(v);
                },
                (holder, i, item) =>
                {
                    var viewHolder = (FilteredItemViewHolder)holder;
                    viewHolder.Bind(item);
                });
            _filteredMembers.SetAdapter(_filteredAdapter);
            _filteredMembers.AddOnScrollListener(new LoadMoreScrollListener(new TaskReference(async () =>
            {
                await ViewModel.PaginationViewModel.LoadNextPageAsync();
            })));
        }

        private void Close()
        {
            ViewModel.DialogComponent.CloseCommand.Execute(false);
        }
    }
}
