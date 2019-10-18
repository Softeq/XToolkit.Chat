// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Bindings.Droid;
using Softeq.XToolkit.Chat.Droid.Controls;
using Softeq.XToolkit.Chat.Droid.Listeners;
using Softeq.XToolkit.Chat.Droid.ViewHolders;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Droid.Converters;
using Softeq.XToolkit.WhiteLabel.Droid;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    [Activity]
    public class NewChatActivity : ActivityBase<NewChatViewModel>
    {
        private NavigationBarView _navigationBarView;
        private EditText _editText;
        private RecyclerView _recyclerView;
        private ObservableRecyclerViewAdapter<ChatUserViewModel> _recyclerViewAdapter;
        private BusyOverlayView _busyOverlayView;
        private LinearLayout _newGroupLayout;
        private ImageView _newGroupImage;
        private TextView _newGroupText;
        private SearchNoResultView _searchNoResultView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            OverridePendingTransition(0, 0);

            SetTheme(StyleHelper.Style.CommonActivityStyle);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_chat_new);

            InitNavigationBar();
            InitSearchBar();
            InitNewGroupLayout();
            InitRecyclerView();

            _busyOverlayView = FindViewById<BusyOverlayView>(Resource.Id.activity_chat_new_busy_view);
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.NoResultVisible).WhenSourceChanges(() =>
            {
                _searchNoResultView.Visibility = BoolToViewStateConverter.ConvertGone(ViewModel.NoResultVisible);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.SearchQuery, () => _editText.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.IsBusy, () => _busyOverlayView.Visibility)
                .ConvertSourceToTarget(BoolToViewStateConverter.ConvertGone));

            _newGroupLayout.Click += NewGroupLayoutClick;
        }

        protected override void DoDetachBindings()
        {
            base.DoDetachBindings();

            _newGroupLayout.Click -= NewGroupLayoutClick;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _recyclerViewAdapter.Dispose();
        }

        private void InitNavigationBar()
        {
            _navigationBarView = FindViewById<NavigationBarView>(Resource.Id.activity_chat_new_nav_bar);
            _navigationBarView.SetLeftButton(StyleHelper.Style.NavigationBarBackButtonIcon, ViewModel.CancelCommand);
            _navigationBarView.SetTitle(ViewModel.LocalizedStrings.NewChat);
            _navigationBarView.SetBackground(Resource.Color.chat_content_color);
        }

        private void InitSearchBar()
        {
            _editText = FindViewById<EditText>(Resource.Id.activity_chat_new_search_text);
            _editText.Hint = ViewModel.LocalizedStrings.Search;
        }

        private void InitNewGroupLayout()
        {
            _newGroupLayout = FindViewById<LinearLayout>(Resource.Id.activity_chat_new_group_container);

            _newGroupImage = FindViewById<ImageView>(Resource.Id.activity_chat_new_group_image);
            _newGroupImage.SetBackgroundResource(StyleHelper.Style.NewGroupAvatarIcon);

            _newGroupText = FindViewById<TextView>(Resource.Id.activity_chat_new_group_text);
            _newGroupText.Text = ViewModel.LocalizedStrings.NewGroup;
        }

        private void InitRecyclerView()
        {
            _recyclerView = FindViewById<RecyclerView>(Resource.Id.activity_chat_new_filtered_members);
            _recyclerView.HasFixedSize = true;
            _recyclerView.SetLayoutManager(new LinearLayoutManager(this));
            _recyclerView.AddItemDecoration(new LeftOffsetItemDecoration(this, Resource.Color.chat_divider_color, 72));
            _recyclerViewAdapter = new ObservableRecyclerViewAdapter<ChatUserViewModel>(
                ViewModel.PaginationViewModel.Items,
                (parent, i) =>
                {
                    var v = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.item_chat_new_contact, parent, false);
                    return new FilteredItemViewHolder(v, ViewModel.CreatePersonalChatCommand);
                },
                (holder, i, item) =>
                {
                    var viewHolder = (FilteredItemViewHolder)holder;
                    viewHolder.Bind(item);
                });
            _recyclerView.SetAdapter(_recyclerViewAdapter);
            _recyclerView.AddOnScrollListener(new LoadMoreScrollListener(new TaskReference(async () =>
            {
                await ViewModel.PaginationViewModel.LoadNextPageAsync();
            })));
            _searchNoResultView = FindViewById<SearchNoResultView>(Resource.Id.search_no_result_container);
            _searchNoResultView.Text = ViewModel.LocalizedStrings.SearchNoResults;
        }

        private void NewGroupLayoutClick(object sender, EventArgs e)
        {
            ViewModel.CreateGroupChatCommand.Execute(null);
        }
    }
}
