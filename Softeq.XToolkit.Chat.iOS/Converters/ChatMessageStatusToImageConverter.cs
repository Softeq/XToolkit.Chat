using System;
using System.ComponentModel;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Common.Interfaces;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Converters
{
    public class ChatMessageStatusToImageConverter : IConverter<UIImage, ChatMessageStatus>
    {
        public UIImage ConvertValue(ChatMessageStatus status, object parameter = null, string language = null)
        {
            var statusImage = default(UIImage);

            switch (status)
            {
                case ChatMessageStatus.Sending:
                    statusImage = UIImage.FromBundle(StyleHelper.Style.MessageSendingBoundleName);
                    break;
                case ChatMessageStatus.Delivered:
                    statusImage = UIImage.FromBundle(StyleHelper.Style.MessageDeliveredBoundleName);
                    break;
                case ChatMessageStatus.Read:
                    statusImage = UIImage.FromBundle(StyleHelper.Style.MessageReadBoundleName);
                    break;
                case ChatMessageStatus.Other:
                    break;
                default: throw new InvalidEnumArgumentException();
            }

            return statusImage;
        }

        public ChatMessageStatus ConvertValueBack(UIImage value, object parameter = null, string language = null)
        {
            throw new NotImplementedException();
        }
    }
}
