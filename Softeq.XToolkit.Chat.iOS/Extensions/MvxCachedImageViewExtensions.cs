// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using AsyncDisplayKitBindings;
using FFImageLoading;
using Softeq.XToolkit.WhiteLabel.iOS.Shared;
using Softeq.XToolkit.WhiteLabel.Threading;
using static Softeq.XToolkit.WhiteLabel.iOS.Extensions.MvxCachedImageViewExtensions;
using static Softeq.XToolkit.WhiteLabel.iOS.Shared.AvatarImageHelpers;

namespace Softeq.XToolkit.Chat.iOS.Extensions
{
    public static class MvxCachedImageViewExtensions
    {
        public static async Task LoadImageAsync(this ASImageNode imageNode, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            var expr = ImageService.Instance.LoadUrl(url);

            try
            {
                var image = await expr.AsUIImageAsync().ConfigureAwait(false);
                imageNode.Image = image;
            }
            catch
            {
                // do nothing
            }
        }

        //TODO VPY: code duplication
        public static async void LoadImageWithTextPlaceholder(this ASImageNode imageView,
            string url,
            string name,
            AvatarStyles styles)
        {
            Execute.BeginOnUIThread(() =>
            {
                imageView.Image = CreateAvatarWithTextPlaceholder(name, styles);
            });

            var expr = ImageService.Instance
                .LoadUrl(url)
                .DownSampleInDip((int)styles.Size.Width, (int)styles.Size.Height);

            try
            {
                var image = await expr.AsUIImageAsync().ConfigureAwait(false);
                if (image != null)
                {
                    imageView.Image = image;
                }
            }
            catch
            {
                // do nothing
            }
        }
    }
}
