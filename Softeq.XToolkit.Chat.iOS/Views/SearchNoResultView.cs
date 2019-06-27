// Developed by Softeq Development Corporation
// http://www.softeq.com

using CoreGraphics;
using Foundation;
using Softeq.XToolkit.WhiteLabel.iOS.Controls;
using System;

namespace Softeq.XToolkit.Chat.iOS.Views
{
    public partial class SearchNoResultView : CustomViewBase
    {
        public SearchNoResultView() : base(CGRect.Empty)
        {
        }

        public string NoResultText
        {
            get => NoResultLabel.Text;
            set => NoResultLabel.Text = value;
        }
    }
}