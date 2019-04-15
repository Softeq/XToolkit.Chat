using CoreGraphics;
using Foundation;
using System;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Controls
{
    // TODO YP: Experimental control
    [Register("AutoScrollLabel")]
    public class AutoScrollLabel : UIScrollView
    {
        private const int _animationDuration = 10;

        private readonly string _separator = "         ";

        private UILabel _label;
        private nfloat _separatorWidth;
        private nfloat _originalTextWidth;

        public AutoScrollLabel(IntPtr handle) : base(handle)
        {
        }

        public AutoScrollLabel(CGRect frame) : base(frame)
        {
            Initialize();
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Initialize();
        }

        public string Text { get; set; }

        public UIFont Font { get; set; } = UIFont.SystemFontOfSize(20);

        public nfloat AnimationDuration { get; set; } = _animationDuration;

        public void EnableAutoScroll()
        {
            _label.Text = Text;
            _label.Font = Font;

            UpdateSizes();
        }

        // TODO YP: stop - start ... - don't work
        //public void StopAnimation()
        //{
        //    Layer.RemoveAllAnimations();
        //}

        private void Initialize()
        {
            SetupScrollView();

            _label = new UILabel();
            SetupLabel(_label);
            AddSubview(_label);
        }

        protected virtual void SetupScrollView()
        {
            ShowsHorizontalScrollIndicator = false;
            ShowsVerticalScrollIndicator = false;
            ScrollEnabled = false;
            ClipsToBounds = true;
        }

        protected virtual void SetupLabel(UILabel label)
        {
            label.BackgroundColor = UIColor.Clear;
            label.Lines = 1;
        }

        private void UpdateSizes()
        {
            var labelWidth = GetLabelTextWidth();
            _originalTextWidth = labelWidth;

            var needAutoScroll = labelWidth > Bounds.Width;

            if (needAutoScroll)
            {
                _separatorWidth = GetTextWidth(_separator, _label.Font);

                _label.Text = _label.Text + _separator + _label.Text;
                labelWidth = GetLabelTextWidth();
            }

            var labelHeight = Bounds.Height;

            ContentSize = new CGSize(labelWidth + 20, labelHeight);

            _label.Frame = new CGRect(0, 0, labelWidth, labelHeight);

            if (needAutoScroll)
            {
                StartAnimation();
            }
        }

        private void StartAnimation()
        {
            var duration = CalculateAnimationDuration();

            UIView.Animate(duration, 1,
                UIViewAnimationOptions.Repeat | UIViewAnimationOptions.CurveLinear,
                () =>
                {
                    ContentOffset = new CGPoint(_originalTextWidth + _separatorWidth, 0);
                },
                () => { });
        }

        private double CalculateAnimationDuration()
        {
            var speed = ContentSize.Width / Bounds.Width;
            return speed * AnimationDuration;
        }

        private nfloat GetLabelTextWidth()
        {
            return GetTextWidth(_label.Text, _label.Font);
        }

        private nfloat GetTextWidth(string text, UIFont font)
        {
            return new NSString(text).GetBoundingRect(
                new CGSize(9999, Bounds.Height),
                NSStringDrawingOptions.UsesLineFragmentOrigin,
                new UIStringAttributes { Font = font }, null).Width;
        }
    }
}
