// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.WhiteLabel.Model;

namespace Softeq.XToolkit.Chat.Strategies.Search
{
    public interface ISearchContactsStrategy
    {
        Task<PagingModel<ChatUserModel>> Search(string query, int pageNumber, int pageSize);
    }
}