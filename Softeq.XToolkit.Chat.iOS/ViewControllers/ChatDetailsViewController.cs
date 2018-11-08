// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using UIKit;
using CoreGraphics;
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

            CustomNavigationItem.Title = ViewModel.Title;
            CustomNavigationItem.SetCommand(UIImage.FromBundle(Styles.BackButtonBundleName), ViewModel.BackCommand, true);

            TableView.RegisterNibForCellReuse(ChatUserViewCell.Nib, ChatUserViewCell.Key);
            TableView.RowHeight = 80;

            _chatDetailsHeaderView = new ChatDetailsHeaderView(new CGRect(0, 0, 200, 250));
            _chatDetailsHeaderView.IsEditMode = false;
            _chatDetailsHeaderView.SetAddMembersCommand(ViewModel.AddMembersCommand);
            _chatDetailsHeaderView.SetChangeChatPhotoCommand(new RelayCommand(OpenPicker));

            TableView.TableHeaderView = _chatDetailsHeaderView;
            TableView.TableFooterView = new UIView();

            var source = ViewModel.Members.GetTableViewSource((cell, viewModel, index) =>
            {
                (cell as ChatUserViewCell)?.BindViewModel(viewModel);
            }, ChatUserViewCell.Key);
            TableView.Source = source;
            _sourceRef = WeakReferenceEx.Create(source);

            _simpleImagePicker = new SimpleImagePicker(this, ServiceLocator.Resolve<IPermissionsManager>(), false)
            {
                MaxImageWidth = 280,
                MaxImageHeight = 280
            };
        }

        protected override void DoAttachBindings()
        {
            base.DoAttachBindings();

            Bindings.Add(this.SetBinding(() => ViewModel.ChatAvatarUrl).WhenSourceChanges((() =>
            {
                _chatDetailsHeaderView.SetChatAvatar(ViewModel.ChatAvatarUrl);
            })));
            Bindings.Add(this.SetBinding(() => ViewModel.ChatName, () => _chatDetailsHeaderView.ChatNameField.Text));
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
    }
}

