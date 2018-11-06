﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using Autofac;
using Softeq.XToolkit.Chat.ViewModels;

namespace Softeq.XToolkit.Chat.Droid
{
    public static class BootstrapperDroid
    {
        public static void Configure(ContainerBuilder containerBuilder)
        {
            Bootstrapper.Configure(containerBuilder);

            containerBuilder.RegisterType<ChatsListViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<ChatMessagesViewModel>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<ChatDetailsViewModel>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<SelectContactsViewModel>().InstancePerLifetimeScope();
        }
    }
}
