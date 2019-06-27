// Developed by Softeq Development Corporation
// http://www.softeq.com

using Autofac;
using Softeq.XToolkit.WhiteLabel.Bootstrapper.Abstract;
using Softeq.XToolkit.WhiteLabel.Droid.ImagePicker;
using Softeq.XToolkit.WhiteLabel.ImagePicker;

namespace Softeq.XToolkit.Chat.Droid
{
    public static class BootstrapperDroid
    {
        public static void Configure(IContainerBuilder containerBuilder)
        {
            containerBuilder.PerDependency<DroidImagePickerService, IImagePickerService>();
            Bootstrapper.Configure(containerBuilder);
        }
    }
}
