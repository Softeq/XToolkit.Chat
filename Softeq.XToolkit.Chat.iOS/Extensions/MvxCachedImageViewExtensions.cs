// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using AsyncDisplayKitBindings;
using FFImageLoading;
using FFImageLoading.Cross;
using FFImageLoading.Work;
using UIKit;

namespace Softeq.XToolkit.Chat.iOS.Extensions
{
    public static class MvxCachedImageViewExtensions
    {
        public static void LoadImageAsync(this MvxCachedImageView imageView, string boundleResource, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                imageView.Image?.Dispose();
                imageView.Image = null;
                if (!string.IsNullOrEmpty(boundleResource))
                {
                    imageView.Image = UIImage.FromBundle(boundleResource);
                }
                return;
            }

            var expr = ImageService.Instance.LoadUrl(url);

            if (!string.IsNullOrEmpty(boundleResource))
            {
                expr = expr.LoadingPlaceholder(boundleResource, ImageSource.CompiledResource)
                           .ErrorPlaceholder(boundleResource, ImageSource.CompiledResource);
            }

            expr.IntoAsync(imageView);
        }

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
    }
}
