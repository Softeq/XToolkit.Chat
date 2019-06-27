// Developed by Softeq Development Corporation
// http://www.softeq.com

using Autofac;
using Softeq.XToolkit.WhiteLabel.Bootstrapper.Abstract;
using Softeq.XToolkit.WhiteLabel.ImagePicker;
using Softeq.XToolkit.WhiteLabel.iOS.ImagePicker;

namespace Softeq.XToolkit.Chat.iOS
{
    public static class BootstrapperIos
    {
        public static void Configure(IContainerBuilder containerBuilder)
        {
            containerBuilder.PerDependency<IosImagePickerService, IImagePickerService>();
            Bootstrapper.Configure(containerBuilder);
        }
    }
}
