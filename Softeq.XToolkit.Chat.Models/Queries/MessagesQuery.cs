// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;

namespace Softeq.XToolkit.Chat.Models.Queries
{
    /// <summary>
    /// Parameters for getting messages
    /// </summary>
    public class MessagesQuery
    {
        /// <summary>
        /// Channel related messages
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// Last message Id
        /// </summary>
        public string FromId { get; set; } = null;

        /// <summary>
        /// Last message Date
        /// </summary>
        public DateTimeOffset? FromDateTime { get; set; } = null;

        /// <summary>
        /// Count of messages
        /// </summary>
        public int? Count { get; set; } = null;
    }
}
