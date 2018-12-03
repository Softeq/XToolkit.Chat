using System;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FFImageLoading.Transformations;
using FFImageLoading.Views;
using FFImageLoading.Work;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Bindings.Droid;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Droid.Dialogs;
using Softeq.XToolkit.WhiteLabel.Droid.Shared.Extensions;

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

            _navigationBarView = View.FindViewById<NavigationBarView>(Resource.Id.dialog_select_members_nav_bar);
            _navigationBarView.SetLeftButton(StyleHelper.Style.NavigationBarBackButtonIcon, new RelayCommand(Close));
            _navigationBarView.SetRightButton(ViewModel.Resources.Done, ViewModel.DoneCommand);
            _navigationBarView.SetTitle(ViewModel.Resources.AddMembers);
            _navigationBarView.SetBackground(StyleHelper.Style.ContentColor);

            var searchContainer = View.FindViewById<View>(Resource.Id.dialog_select_members_serch_container);
            searchContainer.SetBackgroundResource(StyleHelper.Style.UnderlinedBg);

            _editText = View.FindViewById<EditText>(Resource.Id.dialog_select_members_serch_text);
            _editText.Hint = ViewModel.Resources.Search;
            
            _addedMembers = View.FindViewById<RecyclerView>(Resource.Id.dialog_select_members_added_members);
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
                    var viewHolder = (SelectedContactViewHolder) holder;
                    viewHolder.Bind(item);
                }); 
            _addedMembers.SetAdapter(_selectedAdapter);

            _filteredMembers = View.FindViewById<RecyclerView>(Resource.Id.dialog_select_members_filtered_members);
            _filteredMembers.SetLayoutManager(new LinearLayoutManager(Context));
            _filteredMembers.AddItemDecoration(new DividerItemDecoration(Context, DividerItemDecoration.Vertical));
            _filteredAdapter = new ObservableRecyclerViewAdapter<ChatUserViewModel>(
                ViewModel.FoundContacts,
                (parent, i) =>
                {
                    var v = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.view_holder_member_filter_item, parent, false);
                    return new FilteredItemViewHolder(v);
                },
                (holder, i, item) =>
                {
                    var viewHolder = (FilteredItemViewHolder) holder;
                    viewHolder.Bind(item);
                });
            _filteredMembers.SetAdapter(_filteredAdapter);
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            this.SetBinding(() => ViewModel.ContactNameSearchQuery, () => _editText.Text, BindingMode.TwoWay);
            this.SetBinding(() => ViewModel.HasSelectedContacts).WhenSourceChanges(() =>
            {
                _addedMembers.Visibility = ViewModel.HasSelectedContacts
                    ? ViewStates.Visible
                    : ViewStates.Gone;
            });
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            _filteredAdapter.Dispose();
            _selectedAdapter.Dispose();
        }

        private void Close()
        {
            ViewModel.DialogComponent.CloseCommand.Execute(false);
        }
        
        private class SelectedContactViewHolder : RecyclerView.ViewHolder
        {
            private readonly ImageViewAsync _avatar;
            private readonly ImageButton _removeButton;
            private readonly TextView _textView;
            private ChatUserViewModel _viewModel;
            
            public SelectedContactViewHolder(View itemView) : base(itemView)
            {
                _avatar = itemView.FindViewById<ImageViewAsync>(Resource.Id.view_holder_member_avatar);
                _removeButton = itemView.FindViewById<ImageButton>(Resource.Id.view_holder_member_remove);
                _removeButton.SetImageResource(StyleHelper.Style.RemoveImageButtonIcon);
                _removeButton.SetBackgroundResource(Android.Resource.Color.Transparent);
                _removeButton.Click += RemoveButtonOnClick;
                _textView = itemView.FindViewById<TextView>(Resource.Id.view_holder_member_name);
            }

            private void RemoveButtonOnClick(object sender, EventArgs e)
            {
                _viewModel.IsSelected = false;
            }

            public void Bind(ChatUserViewModel viewModel)
            {
                _viewModel = viewModel;
                _avatar.LoadImageWithTextPlaceholder(
                    viewModel.PhotoUrl, 
                    viewModel.Username,
                    StyleHelper.Style.ChatAvatarStyles,
                    x => x.Transform(new CircleTransformation()));
                _textView.Text = viewModel.Username;
            }
        }

        private class FilteredItemViewHolder : RecyclerView.ViewHolder
        {
            private readonly ImageViewAsync _avatar;
            private readonly ImageView _imageView;
            private readonly TextView _textView;
            private readonly View _view;
            private Binding _selectedBinding;
            private ChatUserViewModel _viewModel;

            public FilteredItemViewHolder(View itemView) : base(itemView)
            {
                _imageView = itemView.FindViewById<ImageView>(Resource.Id.view_holder_member_filter_item_indicator);
                _avatar = itemView.FindViewById<ImageViewAsync>(Resource.Id.view_holder_member_filter_item_avatar);
                _textView = itemView.FindViewById<TextView>(Resource.Id.view_holder_member_filter_item_title);
                _view = itemView;
                _view.Click += ViewOnClick;
            }

            private void ViewOnClick(object sender, EventArgs e)
            {
                _viewModel.IsSelected = !_viewModel.IsSelected;
            }

            public void Bind(ChatUserViewModel viewModel)
            {
                _viewModel = viewModel;
                _avatar.LoadImageWithTextPlaceholder(
                    viewModel.PhotoUrl, 
                    viewModel.Username, 
                    StyleHelper.Style.ChatAvatarStyles,
                    x => x.Transform(new CircleTransformation()));
                _textView.Text = viewModel.Username;
                
                _selectedBinding?.Detach();
                _selectedBinding = this.SetBinding(() => _viewModel.IsSelected).WhenSourceChanges(() =>
                {
                    var resId = _viewModel.IsSelected ? StyleHelper.Style.CheckedIcon : StyleHelper.Style.UnCheckedIcon;
                    _imageView.SetImageResource(resId);
                });
            }
        }
    }
}
