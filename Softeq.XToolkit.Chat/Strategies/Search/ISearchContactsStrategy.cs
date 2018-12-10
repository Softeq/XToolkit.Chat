// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Collections.Generic;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.ViewModels;

namespace Softeq.XToolkit.Chat.Strategies.Search
{
    public interface ISearchContactsStrategy
    {
        Task<IList<ChatUserViewModel>> Search(string query, int pageNumber, int pageSize);
    }
}