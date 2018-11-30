using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FFImageLoading.Views;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.WhiteLabel.Droid.Controls;
using Softeq.XToolkit.WhiteLabel.Droid.Dialogs;
using Softeq.XToolkit.WhiteLabel.Droid.Shared.Extensions;

namespace Softeq.XToolkit.Chat.Droid.Views
{
    public class SelectMembersDialogFragment : DialogFragmentBase<SelectMembersViewModel>
    {
        private NavigationBarView _navigationBarView;
        private EditText _editText;
        private RecyclerView _addedMembers;
        private RecyclerView _filteredMembers;

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
            _navigationBarView.SetLeftButton(StyleHelper.Style.NavigationBarBackButtonIcon, new RelayCommand(() => { }));
            _navigationBarView.SetRightButton("done", new RelayCommand(() => { }));
            _navigationBarView.SetTitle("title");
            _navigationBarView.SetBackground(StyleHelper.Style.ContentColor);

            var searchContainer = View.FindViewById<View>(Resource.Id.dialog_select_members_serch_container);
            searchContainer.SetBackgroundResource(StyleHelper.Style.UnderlinedBg);

            _editText = View.FindViewById<EditText>(Resource.Id.dialog_select_members_serch_text);

            _addedMembers = View.FindViewById<RecyclerView>(Resource.Id.dialog_select_members_added_members);
            _addedMembers.SetLayoutManager(new LinearLayoutManager(Context, LinearLayoutManager.Horizontal, false));
            _addedMembers.SetAdapter(new TmpAdapter());

            _filteredMembers = View.FindViewById<RecyclerView>(Resource.Id.dialog_select_members_filtered_members);
            _filteredMembers.SetLayoutManager(new LinearLayoutManager(Context));
            _filteredMembers.SetAdapter(new TmpFilterAdapter());
            _filteredMembers.AddItemDecoration(new DividerItemDecoration(Context, DividerItemDecoration.Vertical));
        }

        private class TmpAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => 20;

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var viewHolder = (SelectedContactViewHolder) holder;
                viewHolder.Bind();
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.view_holder_member, parent, false);
                return new SelectedContactViewHolder(view);
            }
        }

        private class TmpFilterAdapter : RecyclerView.Adapter
        {
            public override int ItemCount => 20;

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var viewHolder = (FilteredItemViewHolder) holder;
                viewHolder.Bind();
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.view_holder_member_filter_item, parent, false);
                return new FilteredItemViewHolder(view);
            }
        }
        
        private class SelectedContactViewHolder : RecyclerView.ViewHolder
        {
            private readonly ImageViewAsync _avatar;
            private readonly ImageButton _removeButton;
            private readonly TextView _textView;
            
            public SelectedContactViewHolder(View itemView) : base(itemView)
            {
                _avatar = itemView.FindViewById<ImageViewAsync>(Resource.Id.view_holder_member_avatar);
                _removeButton = itemView.FindViewById<ImageButton>(Resource.Id.view_holder_member_remove);
                _removeButton.SetImageResource(StyleHelper.Style.RemoveImageButtonIcon);
                _removeButton.SetBackgroundResource(Android.Resource.Color.Transparent);
                _textView = itemView.FindViewById<TextView>(Resource.Id.view_holder_member_name);
            }

            public void Bind()
            {
                _avatar.LoadImageWithTextPlaceholder("", "Vadim Pylsky", StyleHelper.Style.ChatAvatarStyles);
                _textView.Text = "Vadim Pylsky";
            }
        }

        private class FilteredItemViewHolder : RecyclerView.ViewHolder
        {
            private readonly ImageViewAsync _avatar;
            private readonly ImageView _imageView;
            private readonly TextView _textView;
            int i;

            public FilteredItemViewHolder(View itemView) : base(itemView)
            {
                _imageView = itemView.FindViewById<ImageView>(Resource.Id.view_holder_member_filter_item_indicator);
                _avatar = itemView.FindViewById<ImageViewAsync>(Resource.Id.view_holder_member_filter_item_avatar);
                _textView = itemView.FindViewById<TextView>(Resource.Id.view_holder_member_filter_item_title);
            }

            public void Bind()
            {
                _avatar.LoadImageWithTextPlaceholder("", "Vadim Pylsky", StyleHelper.Style.ChatAvatarStyles);
                _textView.Text = "Vadim Pylsky";
                i++;
                var resId = i % 2 == 0 ? StyleHelper.Style.UnCheckedIcon : StyleHelper.Style.CheckedIcon;
                _imageView.SetImageResource(resId);
            }
        }
    }
}
