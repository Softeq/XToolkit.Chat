// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;

namespace Softeq.XToolkit.Chat.Models.Interfaces
{
    public interface IChatAuthService
    {
        string UserId { get; }

        Task<string> GetAccessToken();

        Task RefreshToken();
    }
}
