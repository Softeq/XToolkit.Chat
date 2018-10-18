// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;

namespace Softeq.XToolkit.Chat.Models.Interfaces
{
    public interface ISampleChatLoginService
    {
        Task<string> LoginAsync();
        bool IsAuthorized { get; }
        string Username { get; }
    }
}
