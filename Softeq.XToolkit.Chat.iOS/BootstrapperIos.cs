// Developed by Softeq Development Corporation
// http://www.softeq.com

using Autofac;

namespace Softeq.XToolkit.Chat.iOS
{
    public static class BootstrapperIos
    {
        public static void Configure(ContainerBuilder containerBuilder)
        {
            Bootstrapper.Configure(containerBuilder);
        }
    }
}
