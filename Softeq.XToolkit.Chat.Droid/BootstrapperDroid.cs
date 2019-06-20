// Developed by Softeq Development Corporation
// http://www.softeq.com

using Autofac;
using Softeq.XToolkit.WhiteLabel.Bootstrapper.Abstract;

namespace Softeq.XToolkit.Chat.Droid
{
    public static class BootstrapperDroid
    {
        public static void Configure(IContainerBuilder containerBuilder)
        {
            Bootstrapper.Configure(containerBuilder);
        }
    }
}
