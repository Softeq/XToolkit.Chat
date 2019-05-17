// Developed by Softeq Development Corporation
// http://www.softeq.com

using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Softeq.XToolkit.Auth;

namespace Softeq.XToolkit.Chat.SignalRClient
{
    internal class SignalRClient : NetKit.Chat.SignalRClient.SignalRClient
    {
        public SignalRClient(string url, IAccountService accountService)
            : base(url, () => Task.FromResult(accountService.AccessToken))
        {
        }

        public override IHubConnectionBuilder SetupConnection()
        {
            var connection = base.SetupConnection();
#if DEBUG
            connection.ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Trace);
                logging.AddConsole();
            });
#endif
            return connection;
        }

        // TODO YP: wait response
        // https://github.com/Softeq/NetKit.Chat.Client.SDK/issues/9
//        public async Task Disconnect()
//        {
//            await _connection.InvokeAsync(ServerMethods.DeleteClientAsync).ConfigureAwait(false);
//            await _connection.StopAsync().ConfigureAwait(false);
//        }

    }
}
