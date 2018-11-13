// Developed by Softeq Development Corporation
// http://www.softeq.com

using Autofac;
using Softeq.XToolkit.Chat.ViewModels;

namespace Softeq.XToolkit.Chat.iOS
{
    public static class BootstrapperIos
    {
        public static void Configure(ContainerBuilder containerBuilder)
        {
            Bootstrapper.Configure(containerBuilder);

            containerBuilder.RegisterType<ChatsListViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<ChatMessagesViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<ChatDetailsViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<SelectContactsViewModel>().InstancePerDependency();
        }
    }
}
