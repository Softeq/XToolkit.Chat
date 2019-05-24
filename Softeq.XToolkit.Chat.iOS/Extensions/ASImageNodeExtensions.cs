// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using AsyncDisplayKitBindings;
using FFImageLoading;
using Softeq.XToolkit.WhiteLabel.Threading;
using UIKit;
using static Softeq.XToolkit.WhiteLabel.iOS.Helpers.AvatarImageHelpers;

namespace Softeq.XToolkit.Chat.iOS.Extensions
{
    public static class ASImageNodeExtensions
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

        public static async void LoadImageWithTextPlaceholder(this ASImageNode imageView,
            string url,
            string name,
            AvatarStyles styles)
        {
            Execute.BeginOnUIThread(() =>
            {
                imageView.Image = CreateAvatarWithTextPlaceholder(name, styles);
            });

            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            var expr = ImageService.Instance
                .LoadUrl(url)
                .DownSampleInDip(styles.Size.Width, styles.Size.Height);

            UIImage image = null;

            try
            {
                image = await expr.AsUIImageAsync().ConfigureAwait(false);
            }
            catch
            {
                // do nothing
            }

            if (image != null)
            {
                Execute.BeginOnUIThread(() =>
                {
                    imageView.Image = image;
                });
            }
        }
    }
}
