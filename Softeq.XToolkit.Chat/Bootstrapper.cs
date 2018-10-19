// Developed by Softeq Development Corporation
// http://www.softeq.com

using Autofac;
using Softeq.XToolkit.Chat.HttpClient;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.SignalRClient;
using Softeq.XToolkit.Chat.ViewModels;

namespace Softeq.XToolkit.Chat
{
    public static class Bootstrapper
    {
        public static void Configure(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<SignalRAdapter>().As<ISocketChatAdapter>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<HttpChatAdapter>().As<IHttpChatAdapter>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<ChatService>().As<IChatService>()
                .InstancePerLifetimeScope();
            containerBuilder.RegisterType<ChatManager>().InstancePerLifetimeScope();
            containerBuilder.RegisterType<InMemoryMessagesCache>().As<IMessagesCache>().InstancePerLifetimeScope();

            containerBuilder.RegisterType<ChatSummaryViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<ChatMessageViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<ChatUserViewModel>().InstancePerDependency();
            containerBuilder.RegisterType<ConnectionStatusViewModel>().InstancePerDependency();
        }
    }
}
