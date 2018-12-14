﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using UIKit;
using CoreGraphics;
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

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class SelectContactsViewController : ViewControllerBase<SelectContactsViewModel>
    {
        private ChatDetailsHeaderView _chatDetailsHeaderView;
        private SimpleImagePicker _simpleImagePicker;
        private string _previewImageKey;

        public SelectContactsViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CustomNavigationItem.SetCommand(
                UIImage.FromBundle(StyleHelper.Style.BackButtonBundleName),
                ViewModel.BackCommand,
                true);
            CustomNavigationItem.SetCommand(
                ViewModel.ActionButtonName,
                StyleHelper.Style.NavigationBarTintColor,
                new RelayCommand(AddChat),
                false);

            TableView.AllowsSelection = false;
            TableView.RegisterNibForCellReuse(ChatUserViewCell.Nib, ChatUserViewCell.Key);
            TableView.RowHeight = 50;
            TableView.KeyboardDismissMode = UIScrollViewKeyboardDismissMode.Interactive;
            TableView.AddGestureRecognizer(new UITapGestureRecognizer(() =>
            {
                _chatDetailsHeaderView.ChatNameField.ResignFirstResponder();
            }));

            _simpleImagePicker = new SimpleImagePicker(this, Dependencies.IocContainer.Resolve<IPermissionsManager>(), false)
            {
                MaxImageWidth = 280,
                MaxImageHeight = 280,
            };

            _chatDetailsHeaderView = new ChatDetailsHeaderView(new CGRect(0, 0, 200, 250))
            {
                IsEditMode = true
            };

            _chatDetailsHeaderView.SetChangeChatPhotoCommand(new RelayCommand(OpenPicker));
            _chatDetailsHeaderView.SetAddMembersCommand(ViewModel.AddMembersCommand);
            _chatDetailsHeaderView.SetChatAvatar(null);

            TableView.TableHeaderView = _chatDetailsHeaderView;
            TableView.TableFooterView = new UIView();

            var source = ViewModel.Contacts.GetTableViewSource((cell, viewModel, index) =>
            {
                (cell as ChatUserViewCell)?.BindViewModel(viewModel);
            }, ChatUserViewCell.Key);
            TableView.Source = source;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            var indexPath = TableView.IndexPathForSelectedRow;
            if (indexPath != null)
            {
                TableView.DeselectRow(indexPath, false);
            }
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatDetailsHeaderView.ChatNameField.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.ContactsCountText, () => _chatDetailsHeaderView.ChatMembersCount));
            Bindings.Add(this.SetBinding(() => _simpleImagePicker.ViewModel.ImageCacheKey)
                .WhenSourceChanges(() =>
                {
                    if (string.IsNullOrEmpty(_simpleImagePicker.ViewModel.ImageCacheKey))
                    {
                        return;
                    }

                    var key = _simpleImagePicker.ViewModel.ImageCacheKey;
                    if (key == _previewImageKey)
                    {
                        return;
                    }

                    _previewImageKey = key;
                    _chatDetailsHeaderView.SetEditedChatAvatar(_previewImageKey);
                }));
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