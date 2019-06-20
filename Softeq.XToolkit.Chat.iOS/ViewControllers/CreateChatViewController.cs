// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using UIKit;
using Softeq.XToolkit.WhiteLabel.iOS;
using Softeq.XToolkit.Bindings.iOS;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.iOS.Views;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using Softeq.XToolkit.WhiteLabel.iOS.ImagePicker;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.Permissions;
using Softeq.XToolkit.Chat.iOS.Controls;
using CoreGraphics;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class CreateChatViewController : ViewControllerBase<CreateChatViewModel>
    {
        private SimpleImagePicker _simpleImagePicker;
        private ChatDetailsHeaderView _chatDetailsHeaderView;
        private string _previewImageKey;
        private ObservableTableViewSource<ChatUserViewModel> _tableViewSource;
        private UITapGestureRecognizer _hideKeyboardByTapGestureRecognizer;

        public CreateChatViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitNavigationBar();
            InitDetailsHeader();
            InitChatMembersTableView();
            InitProgressIndicator();

            _hideKeyboardByTapGestureRecognizer = new UITapGestureRecognizer(() =>
            {
                _chatDetailsHeaderView.ChatNameField.ResignFirstResponder();
            });
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatDetailsHeaderView.ChatNameField.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.ContactsCountText, () => _chatDetailsHeaderView.ChatMembersCount));
            Bindings.Add(this.SetBinding(() => _simpleImagePicker.ViewModel.ImageCacheKey).WhenSourceChanges(() =>
            {
                var newImageCacheKey = _simpleImagePicker.ViewModel.ImageCacheKey;

                if (string.IsNullOrEmpty(newImageCacheKey) || newImageCacheKey == _previewImageKey)
                {
                    return;
                }

                _previewImageKey = newImageCacheKey;

                _chatDetailsHeaderView.SetEditedChatAvatar(_previewImageKey);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.IsBusy).WhenSourceChanges(() =>
            {
                if (ViewModel.IsBusy)
                {
                    ProgressIndicator.StartAnimating();
                }
                else
                {
                    ProgressIndicator.StopAnimating();
                }
            }));

            TableView.AddGestureRecognizer(_hideKeyboardByTapGestureRecognizer);
        }

        protected override void DoDetachBindings()
        {
            base.DoDetachBindings();

            TableView.RemoveGestureRecognizer(_hideKeyboardByTapGestureRecognizer);

            ResetSelectedRow();
        }

        private void InitNavigationBar()
        {
            CustomNavigationItem.Title = ViewModel.LocalizedStrings.CreateGroup;
            CustomNavigationItem.SetCommand(
                UIImage.FromBundle(StyleHelper.Style.BackButtonBundleName),
                ViewModel.BackCommand,
                true);
            CustomNavigationItem.SetCommand(
                ViewModel.LocalizedStrings.Create,
                StyleHelper.Style.AccentColor,
                new RelayCommand(AddChat),
                false);
        }

        private void InitDetailsHeader()
        {
            _chatDetailsHeaderView = new ChatDetailsHeaderView(new CGRect(0, 0, 200, 250));
            _chatDetailsHeaderView.EnableEditMode(true);
            _chatDetailsHeaderView.HideMuteContainer();
            _chatDetailsHeaderView.ChatNamePlaceholder = ViewModel.LocalizedStrings.ChatName;
            _chatDetailsHeaderView.SetChangeChatPhotoCommand(new RelayCommand(OpenPicker), ViewModel.LocalizedStrings.ChangePhoto);
            _chatDetailsHeaderView.SetAddMembersCommand(ViewModel.AddMembersCommand, ViewModel.LocalizedStrings.AddMembers);
            _chatDetailsHeaderView.SetChatAvatar(null, string.Empty);

            _simpleImagePicker = new SimpleImagePicker(this, Dependencies.Container.Resolve<IPermissionsManager>(), false)
            {
                MaxImageWidth = 280,
                MaxImageHeight = 280
            };
        }

        private void InitChatMembersTableView()
        {
            TableView.RegisterNibForCellReuse(ChatUserViewCell.Nib, ChatUserViewCell.Key);
            TableView.RowHeight = 60;
            TableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;
            TableView.TableHeaderView = _chatDetailsHeaderView;

            _tableViewSource = ViewModel.Contacts.GetTableViewSource((cell, viewModel, index) =>
            {
                if (cell is IBindableViewCell<ChatUserViewModel> userCell)
                {
                    userCell.Bind(viewModel);
                }
            }, ChatUserViewCell.Key);

            TableView.Source = _tableViewSource;
        }

        private void InitProgressIndicator()
        {
            ProgressIndicator.Color = StyleHelper.Style.AccentColor;
            ProgressIndicator.HidesWhenStopped = true;
        }

        private void ResetSelectedRow()
        {
            var indexPath = TableView.IndexPathForSelectedRow;
            if (indexPath != null)
            {
                TableView.DeselectRow(indexPath, false);
            }
        }

        private void OpenPicker()
        {
            _simpleImagePicker.OpenGalleryAsync();
        }

        private void AddChat()
        {
            ViewModel.SaveCommand.Execute(_simpleImagePicker.StreamFunc);
        }
    }
}