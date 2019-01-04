// Developed by Softeq Development Corporation
// http://www.softeq.com

namespace Softeq.XToolkit.Chat.SignalRClient
{
    internal static class ServerMethods
    {
        // client
        public const string AddClientAsync = "AddClientAsync";
        public const string DeleteClientAsync = "DeleteClientAsync";

        // channel
        public const string CreateChannelAsync = "CreateChannelAsync";
        public const string UpdateChannelAsync = "UpdateChannelAsync";
        public const string CloseChannelAsync = "CloseChannelAsync";

        public const string GetChannelInfoByIdAsync = "GetChannelInfoByIdAsync";
        public const string GetChannelInfoByNameAsync = "GetChannelInfoByNameAsync";

        public const string GetChannelSettingsAsync = "GetChannelSettingsAsync";

        public const string JoinToChannelAsync = "JoinToChannelAsync";
        public const string LeaveChannelAsync = "LeaveChannelAsync";

        // channels
        public const string GetAllChannelsAsync = "GetAllChannelsAsync";
        public const string GetMyChannelsAsync = "GetMyChannelsAsync";

        // members
        public const string GetChannelMembersAsync = "GetChannelMembersAsync";

        public const string InviteMemberAsync = "InviteMemberAsync";
        public const string InviteMultipleMembersAsync = "InviteMultipleMembersAsync";
        public const string DeleteMemberAsync = "DeleteMemberAsync";

        // messages
        public const string GetChannelMessagesAsync = "GetChannelMessagesAsync";

        public const string AddMessageAsync = "AddMessageAsync";
        public const string DeleteMessageAsync = "DeleteMessageAsync";
        public const string UpdateMessageAsync = "UpdateMessageAsync";


    }
}
