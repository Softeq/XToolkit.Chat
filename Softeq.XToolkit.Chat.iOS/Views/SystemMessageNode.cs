// Developed by Softeq Development Corporation
// http://www.softeq.com

using AsyncDisplayKitBindings;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.iOS.Helpers;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public class SystemMessageNode : ASCellNode
    {
        private readonly ASTextNode _messageNode = new ASTextNode();

        public SystemMessageNode(ChatMessageViewModel viewModel)
        {
            BackgroundColor = UIColor.FromRGB(245, 245, 245);
            SelectionStyle = UITableViewCellSelectionStyle.None;
            AutomaticallyManagesSubnodes = true;

            var attributedText = viewModel.Body
                .BuildAttributedString()
                .Font(UIFont.SystemFontOfSize(11, UIFontWeight.Semibold))
                .Foreground(UIColor.FromRGB(255, 255, 255));

            _messageNode.AttributedText = attributedText;
        }

        public override ASLayoutSpec LayoutSpecThatFits(ASSizeRange constrainedSize)
        {
            var messageLayout = ASInsetLayoutSpec.InsetLayoutSpecWithInsets(
                new UIEdgeInsets(10, 10, 10, 10), _messageNode);

            var backgroundNode = new ASDisplayNode
            {
                BackgroundColor = StyleHelper.Style.AccentColor,
                CornerRoundingType = ASCornerRoundingType.DefaultSlowCALayer,
                CornerRadius = 16
            };

            var messageLayoutWithBackground = ASBackgroundLayoutSpec.BackgroundLayoutSpecWithChild(
                messageLayout, backgroundNode);

            var mainLayout = ASCenterLayoutSpec.CenterLayoutSpecWithCenteringOptions(
                ASCenterLayoutSpecCenteringOptions.Xy,
                ASCenterLayoutSpecSizingOptions.MinimumXY,
                messageLayoutWithBackground);

            return ASInsetLayoutSpec.InsetLayoutSpecWithInsets(
                new UIEdgeInsets(8, 20, 8, 20), mainLayout);
        }
    }
}
