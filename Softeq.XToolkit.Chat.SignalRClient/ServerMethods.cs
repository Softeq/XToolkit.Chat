// Developed by Softeq Development Corporation
// http://www.softeq.com

namespace Softeq.XToolkit.Chat.SignalRClient
{
    // Latest SignalR Hub commands:
    // https://github.com/Softeq/NetKit.Chat/blob/develop/Softeq.NetKit.Chat.SignalR/Hubs/ChatHub.cs
    internal static class ServerMethods
    {
        // Client
        public const string AddClientAsync = "AddClientAsync";
        public const string DeleteClientAsync = "DeleteClientAsync";

        // Message
        public const string AddMessageAsync = "AddMessageAsync";
        public const string DeleteMessageAsync = "DeleteMessageAsync";
        public const string UpdateMessageAsync = "UpdateMessageAsync";

        public const string MarkAsReadMessageAsync = "MarkAsReadMessageAsync"; // not implemented

        // Message Attachment
        public const string AddMessageAttachmentAsync = "AddMessageAttachmentAsync"; // not implemented
        public const string DeleteMessageAttachmentAsync = "DeleteMessageAttachmentAsync"; // not implemented

        // Member
        public const string InviteMemberAsync = "InviteMemberAsync";
        public const string InviteMultipleMembersAsync = "InviteMultipleMembersAsync";
        public const string DeleteMemberAsync = "DeleteMemberAsync";

        // Channel
        public const string JoinToChannelAsync = "JoinToChannelAsync";

        public const string CreateChannelAsync = "CreateChannelAsync";
        public const string UpdateChannelAsync = "UpdateChannelAsync";
        public const string CloseChannelAsync = "CloseChannelAsync";

        public const string LeaveChannelAsync = "LeaveChannelAsync";

        public const string MuteChannelAsync = "MuteChannelAsync"; // not implemented
        public const string PinChannelAsync = "PinChannelAsync"; // not implemented
    }
}
