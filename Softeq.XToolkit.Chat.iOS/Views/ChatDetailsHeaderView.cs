// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Windows.Input;
using CoreGraphics;
using Foundation;
using UIKit;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.WhiteLabel.iOS.Helpers;
using Softeq.XToolkit.WhiteLabel.iOS.Controls;
using Softeq.XToolkit.Chat.iOS.Extensions;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class ChatDetailsHeaderView : NotifyingView
    {
        private bool _isInitialized;

        public ChatDetailsHeaderView(IntPtr handle) : base(handle) { }
        public ChatDetailsHeaderView(CGRect frame) : base(frame) { }

        public UITextField ChatNameField => ChatNameTextField;

        public string ChatMembersCount
        {
            get => MembersCountLabel.Text;
            set => MembersCountLabel.Text = value;
        }

        public string ChatAvatarUrl
        {
            get => null;
            set
            {
                TryInitAvatarImageView();
                ChatAvatarImageView.LoadImageAsync("NoPhoto", value);
            }
        }

        public bool IsEditMode
        {
            set
            {
                ChatNameTextField.Enabled = value;
            }
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

        protected override void Initialize()
        {
            base.Initialize();

            ChatNameTextField.ShouldReturn += (textField) =>
            {
                textField.ResignFirstResponder();
                return true;
            };
        }

        private void TryInitAvatarImageView()
        {
            if (_isInitialized)
            {
                return;
            }

            ChatAvatarImageView.MakeImageViewCircular();

            _isInitialized = true;
        }
    }
}
