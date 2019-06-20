// Developed by Softeq Development Corporation
// http://www.softeq.com

using Autofac;
using Softeq.XToolkit.Chat.HttpClient;
using Softeq.XToolkit.Chat.Interfaces;
using Softeq.XToolkit.Chat.Manager;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.SignalRClient;
using Softeq.XToolkit.Chat.ViewModels;
using Softeq.XToolkit.Chat.Services;
using Softeq.XToolkit.WhiteLabel.Bootstrapper.Abstract;

namespace Softeq.XToolkit.Chat
{
    public static class Bootstrapper
    {
        public static void Configure(IContainerBuilder containerBuilder)
        {
            containerBuilder.Singleton<SignalRAdapter, ISocketChatAdapter>();
            containerBuilder.Singleton<HttpChatAdapter, IHttpChatAdapter>();
            containerBuilder.Singleton<ChatService, IChatService>();
            containerBuilder.Singleton<ChatManager, ChatManager>();
            containerBuilder.Singleton<ChatManager, IChatsListManager>();
            containerBuilder.Singleton<ChatManager, IChatMessagesManager>();
            containerBuilder.Singleton<ChatManager, IChatConnectionManager>();
            containerBuilder.Singleton<InMemoryMessagesCache, IMessagesCache>();
            containerBuilder.Singleton<FormatService, IFormatService>();
            containerBuilder.Singleton<UploadImageService, IUploadImageService>();

            // ViewModels
            containerBuilder.PerDependency<ChatsListViewModel>();
            containerBuilder.PerDependency<CreateChatViewModel>();
            containerBuilder.PerDependency<AddContactsViewModel>();
            containerBuilder.PerDependency<ChatMessagesViewModel>();
            containerBuilder.PerDependency<ChatDetailsViewModel>();
            containerBuilder.PerDependency<ChatSummaryViewModel>();
            containerBuilder.PerDependency<ChatMessageViewModel>();
            containerBuilder.PerDependency<ConnectionStatusViewModel>();
            containerBuilder.PerDependency<NewChatViewModel>();
        }
    }
}
