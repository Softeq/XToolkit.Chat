// Developed by Softeq Development Corporation
// http://www.softeq.com

using FFImageLoading;
using FFImageLoading.Cross;
using FFImageLoading.Work;

namespace Softeq.XToolkit.Chat.Droid.Extensions
{
    public static class MvxCachedImageViewExtensions
    {
        public static void LoadImageAsync(this MvxCachedImageView imageView, string bundleResourceName, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                imageView.SetImageDrawable(null);

                if (!string.IsNullOrEmpty(bundleResourceName))
                {
                    ImageService.Instance.LoadCompiledResource(bundleResourceName).IntoAsync(imageView);
                }

                return;
            }

            var expr = ImageService.Instance.LoadUrl(url);

            if (!string.IsNullOrEmpty(bundleResourceName))
            {
                expr = expr.LoadingPlaceholder(bundleResourceName, ImageSource.CompiledResource)
                           .ErrorPlaceholder(bundleResourceName, ImageSource.CompiledResource);
            }

            expr.IntoAsync(imageView);
        }
    }
}
