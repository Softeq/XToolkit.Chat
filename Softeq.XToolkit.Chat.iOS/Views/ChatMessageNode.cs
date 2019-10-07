// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using AsyncDisplayKitBindings;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using UIKit;
using Softeq.XToolkit.Bindings;
using Softeq.XToolkit.Chat.iOS.Extensions;
using Softeq.XToolkit.Chat.iOS.Controls;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.iOS.Helpers;
using Softeq.XToolkit.WhiteLabel.iOS.Helpers;
using Softeq.XToolkit.WhiteLabel.Threading;
using Softeq.XToolkit.WhiteLabel.ViewModels;
using FFImageLoading;
using System.Threading.Tasks;
using FFImageLoading.Work;
using Softeq.XToolkit.WhiteLabel;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.Chat.iOS.Converters;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public class ChatMessageNode : ASCellNode
    {
        private const double AvatarSize = 35;

        private readonly WeakReferenceEx<ChatMessageViewModel> _viewModelRef;
        private readonly IItemActionHandler<ChatMessageViewModel> _contextMenuComponent;
        private readonly bool _isMyMessage;

        private readonly ASTextNode _descriptionTextNode = new ASTextNode();
        private readonly ASTextNode _dateTimeTextNode = new ASTextNode();
        private readonly ASImageNode _avatarImageNode = new ASImageNode();
        private readonly ASImageNode _attachmentImageNode = new ASImageNode();
        private readonly ASImageNode _statusImageNode = new ASImageNode();
        private readonly ChatMessageStatusToImageConverter _statusToImageConverter = new ChatMessageStatusToImageConverter();

        private Binding _messageBodyBinding;
        private Binding _messageStatusBinding;

        public ChatMessageNode(
            ChatMessageViewModel viewModel,
            IItemActionHandler<ChatMessageViewModel> contextMenuComponent)
        {
            _viewModelRef = WeakReferenceEx.Create(viewModel);
            _contextMenuComponent = contextMenuComponent;
            _isMyMessage = viewModel.IsMine;

            Execute.BeginOnUIThread(() => View.AddGestureRecognizer(new UILongPressGestureRecognizer(TryShowMenu)));

            BackgroundColor = UIColor.FromRGB(245, 245, 245);
            SelectionStyle = UITableViewCellSelectionStyle.None;
            AutomaticallyManagesSubnodes = true;

            UpdateText();

            if (_viewModelRef.Target != null)
            {
                var dateTimeText = _viewModelRef.Target?.TextDateTime;
                var attributedDateTimeText = dateTimeText.BuildAttributedString()
                                                         .Font(UIFont.SystemFontOfSize(11, UIFontWeight.Semibold))
                                                         .Foreground(UIColor.FromRGB(141, 141, 141));
                _dateTimeTextNode.AttributedText = attributedDateTimeText;
            }

            _avatarImageNode.ImageModificationBlock = image =>
            {
                var size = new CGSize(AvatarSize, AvatarSize);
                return image.MakeCircularImageWithSize(size);
            };
            _avatarImageNode.ContentMode = UIViewContentMode.ScaleAspectFill;
            _avatarImageNode.Hidden = _isMyMessage;
            if (!_isMyMessage)
            {
                _avatarImageNode.LoadImageWithTextPlaceholder(
                    _viewModelRef.Target.SenderPhotoUrl,
                    _viewModelRef.Target.SenderName,
                    new AvatarImageHelpers.AvatarStyles
                    {
                        BackgroundHexColors = StyleHelper.Style.AvatarStyles.BackgroundHexColors,
                        Font = StyleHelper.Style.AvatarStyles.Font,
                        Size = new System.Drawing.Size((int) AvatarSize, (int) AvatarSize)
                    });
            }

            _attachmentImageNode.ContentMode = UIViewContentMode.ScaleAspectFit;
            _attachmentImageNode.ImageModificationBlock = image => image.MakeImageWithRoundedCorners(12);
            LoadAttachmentImageIfNeeded();

            _statusImageNode.ContentMode = UIViewContentMode.Center;
            _statusImageNode.Hidden = !_isMyMessage;
        }

        public override ASLayoutSpec LayoutSpecThatFits(ASSizeRange constrainedSize)
        {
            return BuildNode(constrainedSize.max.Width);
        }

        public override void DidEnterDisplayState()
        {
            base.DidEnterDisplayState();
            _messageBodyBinding?.Detach();
            _messageStatusBinding?.Detach();
            if (_viewModelRef.Target == null)
            {
                return;
            }
            _messageBodyBinding = this.SetBinding(() => _viewModelRef.Target.Body).WhenSourceChanges(() =>
            {
                if (_viewModelRef.Target != null && _viewModelRef.Target.Body != _descriptionTextNode?.AttributedText?.Value)
                {
                    UpdateText();
                    SetNeedsLayout();
                    LayoutIfNeeded();
                }
            });

            _messageStatusBinding = this.SetBinding(() => _viewModelRef.Target.Status).WhenSourceChanges(() =>
            {
                if (_viewModelRef.Target != null)
                {
                    _statusImageNode.Image = _statusToImageConverter.ConvertValue(_viewModelRef.Target.Status);
                }
            });
        }

        public override void DidExitDisplayState()
        {
            base.DidExitDisplayState();

            _messageBodyBinding?.Detach();
            _messageStatusBinding?.Detach();
        }

        private void UpdateText()
        {
            var attrText = _viewModelRef.Target.Body
                .BuildAttributedString()
                .Font(UIFont.SystemFontOfSize(17))
                .DetectLinks(StyleHelper.Style.AccentColor, NSUnderlineStyle.Single, true, out var links);

            if (links != null)
            {
                _descriptionTextNode.Delegate = new LinksDelegate();
                _descriptionTextNode.UserInteractionEnabled = true;
                _descriptionTextNode.LinkAttributeNames = links;
            }

            _descriptionTextNode.AttributedText = attrText;
        }

        private ASLayoutSpec BuildNode(nfloat width)
        {
            var layout = new ASStackLayoutSpec
            {
                Direction = ASStackLayoutDirection.Horizontal,
                Children = new[]
                {
                    BuildAvatarNode(),
                    BuildMessageBodyNode(width)
                },
                AlignItems = ASStackLayoutAlignItems.Start
            };

            if (_isMyMessage)
            {
                layout.JustifyContent = ASStackLayoutJustifyContent.End;
            }

            return ASInsetLayoutSpec.InsetLayoutSpecWithInsets(new UIEdgeInsets(0, 9, 0, 8), layout);
        }

        private IASLayoutElement BuildAvatarNode()
        {
            _avatarImageNode.Style.PreferredSize = new CGSize(AvatarSize, AvatarSize);
            return ASInsetLayoutSpec.InsetLayoutSpecWithInsets(new UIEdgeInsets(6, 0, 0, 0), _avatarImageNode);
        }

        private IASLayoutElement BuildMessageBodyNode(nfloat width)
        {
            var messageBodyNode = BuildMessageBodyWithBackground(BuildMessageBodyContentNode(width));

            messageBodyNode.Style.FlexShrink = 1;

            if (_isMyMessage)
            {
                messageBodyNode.Style.SpacingBefore = 73;
            }
            else
            {
                messageBodyNode.Style.SpacingAfter = 73;
            }

            return messageBodyNode;
        }

        private IASLayoutElement BuildMessageBodyWithBackground(IASLayoutElement messageBodyNode)
        {
            var backgroundImageBundleName = _isMyMessage
                ? StyleHelper.Style.BubbleMineBoundleName
                : StyleHelper.Style.BubbleOtherBoundleName;

            var backgroundImage = UIImage.FromBundle(backgroundImageBundleName)
                    ?.CreateResizableImage(new UIEdgeInsets(36, 20, 10, 20));

            var backgroundNode = new ASImageNode { Image = backgroundImage };

            return ASBackgroundLayoutSpec.BackgroundLayoutSpecWithChild(messageBodyNode, backgroundNode);
        }

        private IASLayoutElement BuildMessageBodyContentNode(nfloat width)
        {
            var messageContentNodes = new List<IASLayoutElement> { _descriptionTextNode };

            if (BuildAttachmentImageNodeIfNeeded(width, out IASLayoutElement attachmentImageNode))
            {
                messageContentNodes.Add(attachmentImageNode);
            }

            messageContentNodes.Add(BuildDateTimeNode());

            var messageInsetLeft = _isMyMessage ? 20 : 30;
            var messageInsetRight = _isMyMessage ? 30 : 20;

            return ASInsetLayoutSpec.InsetLayoutSpecWithInsets(
                new UIEdgeInsets(10, messageInsetLeft, 15, messageInsetRight),
                new ASStackLayoutSpec
                {
                    Direction = ASStackLayoutDirection.Vertical,
                    Children = messageContentNodes.ToArray()
                });
        }

        private bool BuildAttachmentImageNodeIfNeeded(nfloat width, out IASLayoutElement layoutElement)
        {
            if (_viewModelRef.Target == null || !_viewModelRef.Target.HasAttachment)
            {
                layoutElement = null;
                return false;
            }

            var maxWidth = width - 50 - 73 - 17 - (nfloat) AvatarSize;

            if (_attachmentImageNode.Image != null)
            {
                _attachmentImageNode.Style.PreferredSize = CalculateAttachmentImageNodeSize(
                    _attachmentImageNode.Image.Size, maxWidth);

                layoutElement = _attachmentImageNode;
            }
            else
            {
                var placeholderImage = UIImage.FromBundle(StyleHelper.Style.AttachmentImagePlaceholderBundleName);
                var placeholderImageNode = new ASImageNode { Image = placeholderImage };

                placeholderImageNode.Style.PreferredSize = CalculateAttachmentImageNodeSize(placeholderImage.Size, maxWidth);

                layoutElement = placeholderImageNode;
            }

            return true;
        }

        private IASLayoutElement BuildDateTimeNode()
        {
            IASLayoutElement dateTimeNode = _dateTimeTextNode;

            if (!_statusImageNode.Hidden)
            {
                _statusImageNode.Style.PreferredSize = new CGSize(17, _dateTimeTextNode.Style.Height.value);

                dateTimeNode = new ASStackLayoutSpec
                {
                    Direction = ASStackLayoutDirection.Horizontal,
                    Children = new IASLayoutElement[] { _dateTimeTextNode, _statusImageNode },
                    Spacing = 4
                };
            }

            var dateTimeLayoutNode = new ASRelativeLayoutSpec(
                ASRelativeLayoutSpecPosition.End,
                ASRelativeLayoutSpecPosition.Start,
                ASRelativeLayoutSpecSizingOption.Default,
                dateTimeNode);

            dateTimeLayoutNode.Style.SpacingBefore = 5;

            return dateTimeLayoutNode;
        }

        public override bool CanPerformAction(Selector action, NSObject sender)
        {
            return true;
        }

        public override bool CanBecomeFirstResponder => true;

        private void TryShowMenu()
        {
            if (_viewModelRef.Target == null || !_viewModelRef.Target.IsMine)
            {
                return;
            }

            var _ = BecomeFirstResponder;

            _contextMenuComponent.Handle(_descriptionTextNode.View, _viewModelRef.Target);
        }

        [Export("OnAttachmentTapped")]
        private void OnAttachmentTapped()
        {
            var url = _viewModelRef.Target?.Model?.ImageRemoteUrl;
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            var options = new FullScreenImageOptions
            {
                CloseButtonTintColor = Common.iOS.Extensions.UIColorExtensions.ToHex(StyleHelper.Style.ButtonTintColor),
                ImageUrl = url,
                IosCloseButtonImageBundleName = StyleHelper.Style.CloseButtonImageBoundleName
            };

            _viewModelRef.Target?.ShowImage(options);
        }

        private static CGSize CalculateAttachmentImageNodeSize(CGSize imageSize, nfloat maxSize)
        {
            var imageSizeThatFits = imageSize;
            var ratio = imageSize.Height / imageSize.Width;
            if (imageSize.Height > imageSize.Width && imageSize.Height > maxSize)
            {
                imageSizeThatFits.Height *= maxSize / imageSize.Height;
                imageSizeThatFits.Width = imageSizeThatFits.Height / ratio;
            }
            else if (imageSize.Height <= imageSize.Width && imageSize.Width > maxSize)
            {
                imageSizeThatFits.Width *= maxSize / imageSize.Width;
                imageSizeThatFits.Height = imageSizeThatFits.Width * ratio;
            }
            return imageSizeThatFits;
        }

        private void LoadAttachmentImageIfNeeded()
        {
            if (_viewModelRef.Target != null && _viewModelRef.Target.HasAttachment && _attachmentImageNode.Image == null)
            {
                Task.Run(async () =>
                {
                    var model = _viewModelRef.Target.Model;
                    if (model == null)
                    {
                        return;
                    }

                    var expr = default(TaskParameter);

                    if (!string.IsNullOrEmpty(model.ImageCacheKey))
                    {
                        expr = ImageService.Instance
                            .LoadFile(model.ImageCacheKey);

                    }
                    else if (!string.IsNullOrEmpty(model.ImageRemoteUrl))
                    {
                        expr = ImageService.Instance
                            .LoadUrl(model.ImageRemoteUrl);
                    }

                    expr = expr.DownSampleInDip(90, 90);

                    var image = default(UIImage);

                    try
                    {
                        image = await expr.AsUIImageAsync().ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogManager.LogError<ChatMessageNode>(ex);
                    }

                    if (image == null)
                    {
                        return;
                    }

                    Execute.BeginOnUIThread(() =>
                    {
                        _attachmentImageNode.Image = image;

                        SetNeedsLayout();
                        Layout();

                        _attachmentImageNode.AddTarget(
                            this,
                            new Selector(nameof(OnAttachmentTapped)),
                            ASControlNodeEvent.TouchUpInside);
                    });
                });
            }
        }

        private class LinksDelegate : ASTextNodeDelegate
        {
            public override void TappedLinkAttribute(ASTextNode textNode, string attribute, NSObject value, CGPoint point,
                NSRange textRange)
            {
                var launcherService = Dependencies.Container.Resolve<ILauncherService>();
                launcherService.OpenUrl(((NSUrl) value).AbsoluteString);
            }

            public override bool ShouldHighlightLinkAttribute(ASTextNode textNode, string attribute, NSObject value,
                CGPoint point)
            {
                return true;
            }
        }
    }
}
