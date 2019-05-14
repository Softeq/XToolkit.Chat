// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using UIKit;
using CoreGraphics;
using Softeq.XToolkit.WhiteLabel.iOS;
using Softeq.XToolkit.Chat.iOS.Views;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Bindings.iOS;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.Command;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using Softeq.XToolkit.WhiteLabel.iOS.ImagePicker;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.Permissions;
using Softeq.XToolkit.WhiteLabel.Threading;
using Softeq.XToolkit.Chat.iOS.Controls;
using Softeq.XToolkit.Chat.iOS.TableSources;

namespace Softeq.XToolkit.Chat.iOS.ViewControllers
{
    public partial class ChatDetailsViewController : ViewControllerBase<ChatDetailsViewModel>
    {
        private ChatDetailsHeaderView _chatDetailsHeaderView;
        private SimpleImagePicker _simpleImagePicker;
        private string _previewImageKey;
        private bool _isChangeChatPhotoInitialized;

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

            Bindings.Add(this.SetBinding(() => ViewModel.HeaderViewModel.AvatarUrl).WhenSourceChanges(() =>
            {
                _chatDetailsHeaderView.SetChatAvatar(ViewModel.HeaderViewModel.AvatarUrl, ViewModel.HeaderViewModel.ChatName);
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.HeaderViewModel.ChatName, () => _chatDetailsHeaderView.ChatNameField.Text, BindingMode.TwoWay));
            Bindings.Add(this.SetBinding(() => ViewModel.HeaderViewModel.ChatName).WhenSourceChanges(() =>
            {
                var chatName = ViewModel.HeaderViewModel.ChatName;
                if (!string.IsNullOrEmpty(chatName) && !ViewModel.CanEdit)
                {
                    _chatDetailsHeaderView.ChatNameTextView.Text = chatName;
                    _chatDetailsHeaderView.ChatNameTextView.EnableAutoScroll();
                }
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.HeaderViewModel.IsMuted, () => _chatDetailsHeaderView.IsNotificationsMuted));
            Bindings.Add(this.SetBinding(() => ViewModel.HeaderViewModel.IsBusy, () => _chatDetailsHeaderView.IsMuteNotificationsAvailable)
                .ConvertSourceToTarget(x => !x));

            Bindings.Add(this.SetBinding(() => ViewModel.MembersCountText, () => _chatDetailsHeaderView.ChatMembersCount));
            Bindings.Add(this.SetBinding(() => _simpleImagePicker.ViewModel.ImageCacheKey).WhenSourceChanges(() =>
            {
                var newImageCacheKey = _simpleImagePicker.ViewModel.ImageCacheKey;

                if (string.IsNullOrEmpty(newImageCacheKey) || newImageCacheKey == _previewImageKey)
                {
                    return;
                }

                _previewImageKey = newImageCacheKey;

                _chatDetailsHeaderView.SetEditedChatAvatar(_previewImageKey);

                Execute.BeginOnUIThread(() =>
                {
                    ViewModel.HeaderViewModel.StartEditingCommand.Execute(null);
                });
            }));

            Bindings.Add(this.SetBinding(() => ViewModel.IsLoading).WhenSourceChanges(() =>
            {
                if (ViewModel.IsLoading)
                {
                    BusyIndicator.StartAnimating();
                }
                else
                {
                    BusyIndicator.StopAnimating();
                }
                _chatDetailsHeaderView.MembersCountLabelHidden = ViewModel.IsLoading;
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.CanEdit).WhenSourceChanges(() =>
            {
                _chatDetailsHeaderView.HideChangeChatPhoto(!ViewModel.CanEdit);
                _chatDetailsHeaderView.EnableEditMode(ViewModel.CanEdit);

                if (!ViewModel.CanEdit)
                {
                    _chatDetailsHeaderView.Frame = new CGRect(0, 0, 200, 260);

                    return;
                }

                if (!_isChangeChatPhotoInitialized)
                {
                    _chatDetailsHeaderView.SetChangeChatPhotoCommand(new RelayCommand(OpenPicker), ViewModel.LocalizedStrings.ChangePhoto);
                    _chatDetailsHeaderView.SetChangeChatName(ViewModel.HeaderViewModel.StartEditingCommand);

                    _isChangeChatPhotoInitialized = true;
                }
            }));
            Bindings.Add(this.SetBinding(() => ViewModel.HeaderViewModel.IsInEditMode).WhenSourceChanges(() =>
            {
                if (ViewModel.HeaderViewModel.IsInEditMode)
                {
                    CustomNavigationItem.SetCommand(ViewModel.LocalizedStrings.Save, UIColor.Black, new RelayCommand(Save), false);
                }
                else
                {
                    CustomNavigationItem.SetRightBarButtonItem(null, true);

                    _chatDetailsHeaderView.EndEditing();
                }
            }));
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
            _chatDetailsHeaderView.SetChangeMuteNotificationsCommand(
                ViewModel.HeaderViewModel.ChangeMuteNotificationsCommand,
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
            TableView.RowHeight = 60;
            TableView.TableHeaderView = _chatDetailsHeaderView;
            TableView.TableFooterView = new UIView();

            TableView.Source = CreateTableViewSource();
            TableView.Delegate = CreateTableViewDelegate();

            BusyIndicator.Color = StyleHelper.Style.AccentColor;
            BusyIndicator.HidesWhenStopped = true;
        }

        private void OpenPicker()
        {
            _simpleImagePicker.OpenGalleryAsync();
        }

        private void Save()
        {
            ViewModel.HeaderViewModel.SaveCommand.Execute(_simpleImagePicker.StreamFunc);
        }

        private UITableViewSource CreateTableViewSource()
        {
            return new ObservableTableViewSource<ChatUserViewModel>
            {
                DataSource = ViewModel.Members,
                BindCellDelegate = (cell, viewModel, index) =>
                {
                    if (cell is IBindableViewCell<ChatUserViewModel> userViewCell)
                    {
                        userViewCell.Bind(viewModel);
                    }
                },
                CanEditCellDelegate = (viewModel, index) =>
                {
                    return viewModel.IsRemovable;
                },
                ReuseId = ChatUserViewCell.Key
            };
        }

        private UITableViewDelegate CreateTableViewDelegate()
        {
            return new ActionableTableViewDelegate(() =>
            {
                var removeAction = UITableViewRowAction.Create(
                    UITableViewRowActionStyle.Default,
                    ViewModel.LocalizedStrings.Remove,
                    (row, index) =>
                    {
                        ViewModel.RemoveMemberCommand.Execute(ViewModel.Members[index.Row]);
                    });
                return new[] { removeAction };
            });
        }
    }
}