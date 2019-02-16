// Developed by Softeq Development Corporation
// http://www.softeq.com

using Autofac;

namespace Softeq.XToolkit.Chat.Droid
{
    public static class BootstrapperDroid
    {
        public static void Configure(ContainerBuilder containerBuilder)
        {
            Bootstrapper.Configure(containerBuilder);
        }
    }
}
