// Developed by Softeq Development Corporation
// http://www.softeq.com

﻿namespace Softeq.XToolkit.Chat.Models.Queries
{
    public class ContactsQuery
    {
        public string ChannelId { get; set; }

        public string NameFilter { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}
