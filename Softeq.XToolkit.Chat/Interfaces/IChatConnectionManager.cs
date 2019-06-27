// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using Softeq.XToolkit.Chat.Models.Enum;

namespace Softeq.XToolkit.Chat.Interfaces
{
    public interface IChatConnectionManager
    {
        IObservable<ConnectionStatus> ConnectionStatusChanged { get; }
        ConnectionStatus ConnectionStatus { get; }
    }
}