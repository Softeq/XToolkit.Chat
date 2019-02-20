// Developed by Softeq Development Corporation
// http://www.softeq.com

namespace Softeq.XToolkit.Chat.SignalRClient
{
    // Latest SignalR Hub events:
    // https://github.com/Softeq/NetKit.Chat/blob/develop/Softeq.NetKit.Chat.SignalR/Hubs/HubEvents.cs
    internal static class ClientEvents
    {
        public const string MessageDeleted = "MessageDeleted";
        public const string MessageAdded = "MessageAdded";
        public const string MessageUpdated = "MessageUpdated";
        public const string LastReadMessageChanged = "LastReadMessageChanged";

        public const string TypingStarted = "TypingStarted";

        public const string AttachmentAdded = "AttachmentAdded";
        public const string AttachmentDeleted = "AttachmentDeleted";

        public const string MemberLeft = "MemberLeft";
        public const string MemberJoined = "MemberJoined";
        public const string MemberDeleted = "MemberDeleted"; // not implemented
        public const string YouAreDeleted = "YouAreDeleted"; // not implemented

        public const string ChannelAdded = "ChannelAdded";
        public const string ChannelClosed = "ChannelClosed";
        public const string ChannelUpdated = "ChannelUpdated";

        public const string AccessTokenExpired = "AccessTokenExpired";
        public const string ExceptionOccurred = "ExceptionOccurred";
        public const string RequestSuccess = "RequestSuccess";

        public const string RequestValidationFailed = "RequestValidationFailed";
    }
}
