// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Windows.Input;
using CoreGraphics;
using FFImageLoading;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.iOS.Extensions;
using Softeq.XToolkit.WhiteLabel.iOS.Controls;
using Softeq.XToolkit.WhiteLabel.iOS.Extensions;
using Softeq.XToolkit.WhiteLabel.Threading;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class ChatDetailsHeaderView : NotifyingView
    {
        public ChatDetailsHeaderView(CGRect frame) : base(frame)
        {
        }

        public UITextField ChatNameField => ChatNameTextField;

        public string ChatMembersCount
        {
            get => MembersCountLabel.Text;
            set => MembersCountLabel.Text = value;
        }

        public bool IsEditMode
        {
            set => ChatNameTextField.Enabled = value;
        }

        public bool IsAddMembersButtonHidden
        {
            get => AddMembersButton.Hidden;
            set => AddMembersButton.Hidden = value;
        }

        public void SetAddMembersCommand(ICommand command)
        {
            AddMembersButton.SetCommand(command);
        }

        public void SetChangeChatPhotoCommand(ICommand command)
        {
            ChangeChatPhotoButton.SetCommand(command);
        }

        public void SetChatAvatar(string photoUrl)
        {
            ChatAvatarImageView.LoadImageAsync(Styles.ChatGroupNoAvatarBundleName, photoUrl);
        }

        public void SetEditedChatAvatar(string key)
        {
            Execute.BeginOnUIThread(() =>
            {
                if (key == null)
                {
                    EditedChatAvatarImageView.Hidden = true;
                    ChatAvatarImageView.Hidden = false;
                }
                else
                {
                    ChatAvatarImageView.Hidden = true;
                    EditedChatAvatarImageView.Hidden = false;
                    ImageService.Instance
                        .LoadFile(key)
                        .DownSampleInDip(95, 95)
                        .IntoAsync(EditedChatAvatarImageView);
                }
            });
        }

        protected override void Initialize()
        {
            base.Initialize();

            ChatNameTextField.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            ChatAvatarcontainer.Layer.MasksToBounds = false;
            ChatAvatarcontainer.Layer.CornerRadius = ChatAvatarcontainer.Bounds.Width / 2;
            ChatAvatarcontainer.ClipsToBounds = true;

            EditedChatAvatarImageView.Hidden = true;
        }
    }
}