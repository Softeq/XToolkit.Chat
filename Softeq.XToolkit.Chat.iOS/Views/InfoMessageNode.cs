// Developed by Softeq Development Corporation
// http://www.softeq.com

using UIKit;
using AsyncDisplayKitBindings;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Common.iOS.Helpers;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public class InfoMessageNode : ASCellNode
    {
        private ASTextNode _infoTextNode;

        public InfoMessageNode(ChatMessageViewModel viewModel)
        {
            _infoTextNode = new ASTextNode();
            var attributedInfoText = viewModel?.Body.BuildAttributedString()
                                              .Font(UIFont.SystemFontOfSize(11, UIFontWeight.Semibold))
                                              .Foreground(UIColor.FromRGB(141, 141, 141));
            _infoTextNode.AttributedText = attributedInfoText;

            SelectionStyle = UITableViewCellSelectionStyle.None;
            AutomaticallyManagesSubnodes = true;
            SetNeedsLayout();
        }

        public override ASLayoutSpec LayoutSpecThatFits(ASSizeRange constrainedSize)
        {
            var layoutWithInsets = ASInsetLayoutSpec.InsetLayoutSpecWithInsets(
                new UIEdgeInsets(20, 0, 20, 0),
                _infoTextNode);
            return new ASRelativeLayoutSpec(ASRelativeLayoutSpecPosition.Center,
                                            ASRelativeLayoutSpecPosition.Center,
                                            ASRelativeLayoutSpecSizingOption.Default,
                                            layoutWithInsets);
        }
    }
}
