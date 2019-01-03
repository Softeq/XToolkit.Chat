﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using UIKit;
using CoreGraphics;
using Foundation;
using Softeq.XToolkit.WhiteLabel.iOS;
using Softeq.XToolkit.Chat.iOS.Views;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Bindings.iOS;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using Softeq.XToolkit.WhiteLabel.iOS.ImagePicker;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.Permissions;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.WhiteLabel.Threading;
using Softeq.XToolkit.Chat.iOS.Controls;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class ChatDetailsViewController : ViewControllerBase<ChatDetailsViewModel>
    {
        private ChatDetailsHeaderView _chatDetailsHeaderView;
        private WeakReferenceEx<ObservableTableViewSource<ChatUserViewModel>> _sourceRef;
        private SimpleImagePicker _simpleImagePicker;
        private string _previewImageKey;

        public ChatDetailsViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitNavigationBar();
            InitDetailsHeader();
            InitChatMembersTableView();
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.Summary.AvatarUrl).WhenSourceChanges(() =>
            {
                _chatDetailsHeaderView.SetChatAvatar(ViewModel.Summary.AvatarUrl, ViewModel.Summary.Name);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.Summary.Name, () => _chatDetailsHeaderView.ChatNameField.Text));
            Bindings.Add(this.SetBinding(() => ViewModel.MembersCountText, () => _chatDetailsHeaderView.ChatMembersCount));
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
                    Execute.BeginOnUIThread(() =>
                    {
                        _chatDetailsHeaderView.SetEditedChatAvatar(_previewImageKey);
                        CustomNavigationItem.SetCommand(
                            ViewModel.LocalizedStrings.Save, UIColor.Black, new RelayCommand(SaveAsync), false);
                    });
                }));
            Bindings.Add(this.SetBinding(() => ViewModel.IsMuted, () => _chatDetailsHeaderView.IsNotificationsMuted));
            Bindings.Add(this.SetBinding(() => ViewModel.IsBusy, () => _chatDetailsHeaderView.IsMuteNotificationsAvailable)
                .ConvertSourceToTarget(x => !x));
        }

        private void InitNavigationBar()
        {
            CustomNavigationItem.Title = ViewModel.LocalizedStrings.DetailsTitle;
            CustomNavigationItem.SetCommand(
                UIImage.FromBundle(StyleHelper.Style.BackButtonBundleName),
                ViewModel.BackCommand,
                true);
        }

        private void InitDetailsHeader()
        {
            _chatDetailsHeaderView = new ChatDetailsHeaderView(new CGRect(0, 0, 200, 300));
            _chatDetailsHeaderView.EnableEditMode(false);
            _chatDetailsHeaderView.ChatNamePlaceholder = ViewModel.LocalizedStrings.ChatName;
            _chatDetailsHeaderView.SetAddMembersCommand(ViewModel.AddMembersCommand, ViewModel.LocalizedStrings.AddMembers);
            _chatDetailsHeaderView.SetChangeChatPhotoCommand(new RelayCommand(OpenPicker), ViewModel.LocalizedStrings.ChangePhoto);
            _chatDetailsHeaderView.SetChangeMuteNotificationsCommand(
                ViewModel.ChangeMuteNotificationsCommand,
                ViewModel.LocalizedStrings.Notifications);

            _simpleImagePicker = new SimpleImagePicker(this, Dependencies.IocContainer.Resolve<IPermissionsManager>(), false)
            {
                MaxImageWidth = 280,
                MaxImageHeight = 280
            };
        }

        private void InitChatMembersTableView()
        {
            TableView.RegisterNibForCellReuse(ChatUserViewCell.Nib, ChatUserViewCell.Key);
            TableView.RowHeight = 50;
            TableView.TableHeaderView = _chatDetailsHeaderView;
            TableView.TableFooterView = new UIView();

            var source = ViewModel.Members.GetTableViewSource((cell, viewModel, index) =>
            {
                if (cell is IBindableViewCell<ChatUserViewModel> userViewCell)
                {
                    userViewCell.Bind(viewModel);
                }
            }, ChatUserViewCell.Key);

            TableView.Source = source;
            TableView.Delegate = new ParticipantsTableViewDelegate(ViewModel);

            _sourceRef = WeakReferenceEx.Create(source);
        }

        private void OpenPicker()
        {
            _simpleImagePicker.OpenGalleryAsync();
        }

        private async void SaveAsync()
        {
            await ViewModel.SaveAsync(_simpleImagePicker.StreamFunc).ConfigureAwait(false);
            Execute.BeginOnUIThread(() =>
            {
                CustomNavigationItem.SetRightBarButtonItem(null, true);
            });
        }

        private class ParticipantsTableViewDelegate : UITableViewDelegate
        {
            private readonly ChatDetailsViewModel _viewModel;

            public ParticipantsTableViewDelegate(ChatDetailsViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public override UITableViewRowAction[] EditActionsForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (!_viewModel.IsMemberRemovable((int)indexPath.Item))
                {
                    return new UITableViewRowAction[0];
                }

                var remove = UITableViewRowAction.Create(
                    UITableViewRowActionStyle.Default,
                    _viewModel.LocalizedStrings.Remove,
                    (row, index) => _viewModel.RemoveMemberAtCommand.Execute((int)indexPath.Item));

                return new[] { remove };
            }
        }
    }
}