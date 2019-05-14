using System.Collections.Generic;
using Newtonsoft.Json;

namespace Softeq.XToolkit.Chat.HttpClient.Dtos
{
    // TODO YP: Migrate to PagingModelDto on backend side
    internal class PagedMembersDto
    {
        public int TotalRows { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        
        [JsonProperty("entities")]
        public IList<ChatUserDto> Items { get; set; }
    }
}