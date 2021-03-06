﻿// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Softeq.XToolkit.Chat.SignalRClient
{
    internal class SignalRClient : NetKit.Chat.SignalRClient.SignalRClient
    {
        public SignalRClient(string url, Func<Task<string>> accessTokenProvider)
            : base(url, accessTokenProvider)
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
    }
}
