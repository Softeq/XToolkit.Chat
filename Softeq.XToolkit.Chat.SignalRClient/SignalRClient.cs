// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Softeq.XToolkit.Chat.SignalRClient.DTOs;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Channel;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Client;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Member;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Message;
using Softeq.XToolkit.Common.Extensions;
using System.Linq;
using Softeq.XToolkit.Chat.Models.Exceptions;
using Softeq.XToolkit.Auth;

namespace Softeq.XToolkit.Chat.SignalRClient
{
    internal class SignalRClient
    {
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly string _chatHubUrl;

        private HubConnection _connection;
        private IDisposable _accessTokenExpiredSubscription;
        private readonly IAccountService _accountService;

        public SignalRClient(string url, IAccountService accountService)
        {
            _chatHubUrl = url;
            _accountService = accountService;
        }

        public event Action AccessTokenExpired;
        public event Action Disconnected;

        public event Action<ChannelSummaryResponse> ChannelUpdated;
        public event Action<ChannelSummaryResponse> ChannelAdded;
        public event Action<ChannelSummaryResponse> ChannelClosed;

        public event Action<MessageResponse> MessageAdded;
        public event Action<string, ChannelSummaryResponse> MessageDeleted;
        public event Action<MessageResponse> MessageUpdated;

        public event Action<string, string> AttachmentAdded;
        public event Action<string, string> AttachmentDeleted;

        public event Action<string> TypingStarted;

        public event Action<string> LastReadMessageChanged;

        public event Action<MemberSummary, ChannelSummaryResponse> MemberJoined;
        public event Action<MemberSummary, string> MemberLeft;

        public async Task<ClientResponse> ConnectAsync()
        {
            Console.WriteLine("Connecting to {0}", _chatHubUrl);

            _connection = new HubConnectionBuilder()
                .WithUrl($"{_chatHubUrl}/chat", options =>
                {
                    options.AccessTokenProvider = () => { return Task.FromResult(_accountService.AccessToken); };
                })
#if DEBUG
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddConsole();
                })
#endif
                .Build();

            _connection.Closed += e =>
            {
                Console.WriteLine("Connection closed...");
                Disconnected?.Invoke();
                return Task.CompletedTask;
            };

            await _connection.StartAsync().ConfigureAwait(false);

            _accessTokenExpiredSubscription?.Dispose();
            _accessTokenExpiredSubscription = _connection.On<string>(ClientEvents.AccessTokenExpired, requestId =>
            {
                AccessTokenExpired?.Invoke();
            });

            var client = await _connection.InvokeAsync<ClientResponse>(ServerMethods.AddClientAsync).ConfigureAwait(false);

            SubscribeToEvents();

            return client;
        }

        public Task<ChannelSummaryResponse> CreateChannelAsync(CreateChannelRequest request)
        {
            return SendAndHandleExceptionsAsync<ChannelSummaryResponse>(ServerMethods.CreateChannelAsync, request);
        }

        public Task<ChannelSummaryResponse> CreateDirectChannelAsync(CreateDirectChannelRequest request)
        {
            return SendAndHandleExceptionsAsync<ChannelSummaryResponse>(ServerMethods.CreateDirectChannelAsync, request);
        }

        public Task<ChannelSummaryResponse> UpdateChannelAsync(UpdateChannelRequest request)
        {
            return SendAndHandleExceptionsAsync<ChannelSummaryResponse>(ServerMethods.UpdateChannelAsync, request);
        }

        public Task CloseChannelAsync(Guid channelId)
        {
            return SendAndHandleExceptionsAsync(ServerMethods.CloseChannelAsync, new ChannelRequest { ChannelId = channelId });
        }

        public Task DeleteMemberAsync(DeleteMemberRequest request)
        {
            return SendAndHandleExceptionsAsync(ServerMethods.DeleteMemberAsync, request);
        }

        public Task InviteMemberAsync(InviteMemberRequest request)
        {
            return SendAndHandleExceptionsAsync(ServerMethods.InviteMemberAsync, request);
        }

        public Task InviteMembersAsync(InviteMembersRequest request)
        {
            return SendAndHandleExceptionsAsync(ServerMethods.InviteMultipleMembersAsync, request);
        }

        public Task JoinToChannelAsync(JoinToChannelRequest request)
        {
            return SendAndHandleExceptionsAsync(ServerMethods.JoinToChannelAsync, request);
        }

        public Task LeaveChannelAsync(Guid channelId)
        {
            return SendAndHandleExceptionsAsync(ServerMethods.LeaveChannelAsync, new ChannelRequest { ChannelId = channelId });
        }

        public Task<MessageResponse> CreateMessageAsync(CreateMessageRequest request)
        {
            return SendAndHandleExceptionsAsync<MessageResponse>(ServerMethods.AddMessageAsync, request);
        }

        public Task DeleteMessageAsync(DeleteMessageRequest request)
        {
            return SendAndHandleExceptionsAsync(ServerMethods.DeleteMessageAsync, request);
        }

        public Task UpdateMessageAsync(UpdateMessageRequest request)
        {
            return SendAndHandleExceptionsAsync(ServerMethods.UpdateMessageAsync, request);
        }

        private async Task SendAndHandleExceptionsAsync(string methodName, BaseRequest request)
        {
            //TODO:YS in viewmodel that use this method(LeaveChannelAsync) I see exception handling(try/catch), better log exception here 
            var tcs = new TaskCompletionSource<bool>();
            var requestId = Guid.NewGuid().ToString();

            CreateExceptionSubscription(requestId, tcs);
            CreateValidationFailedSubscription(requestId, tcs);

            IDisposable successSubscription = null;
            successSubscription = _connection.On<string>(ClientEvents.RequestSuccess, id =>
            {
                if (id == requestId)
                {
                    successSubscription.Dispose();
                    tcs.SetResult(true);
                }
            });

            request.RequestId = requestId;
            await _connection.InvokeAsync(methodName, request).ConfigureAwait(false);
            await tcs.Task.ConfigureAwait(false);
        }

        private async Task<T> SendAndHandleExceptionsAsync<T>(string methodName, BaseRequest request)
        {
            var tcs = new TaskCompletionSource<T>();
            var requestId = Guid.NewGuid().ToString();

            CreateExceptionSubscription(requestId, tcs);
            CreateValidationFailedSubscription(requestId, tcs);

            IDisposable successSubscription = null;
            var isCallEnded = false;
            var result = default(T);
            successSubscription = _connection.On<string>(ClientEvents.RequestSuccess, id =>
            {
                if (id == requestId)
                {
                    successSubscription.Dispose();
                    if (isCallEnded)
                    {
                        tcs.SetResult(result);
                    }
                    else
                    {
                        isCallEnded = true;
                    }
                }
            });

            request.RequestId = requestId;
            result = await _connection.InvokeAsync<T>(methodName, request).ConfigureAwait(false);
            if (isCallEnded)
            {
                return result;
            }
            isCallEnded = true;
            return await tcs.Task.ConfigureAwait(false);
        }

        private void CreateExceptionSubscription<T>(string requestId, TaskCompletionSource<T> tcs)
        {
            IDisposable exceptionSubscription = null;
            exceptionSubscription = _connection.On<Exception, string>(ClientEvents.ExceptionOccurred,
                (ex, id) =>
                {
                    if (id == requestId)
                    {
                        exceptionSubscription.Dispose();
                        tcs.SetException(ex);
                    }
                });
        }

        private void CreateValidationFailedSubscription<T>(string requestId, TaskCompletionSource<T> tcs)
        {
            IDisposable validationFailedSubscription = null;
            validationFailedSubscription = _connection.On<IEnumerable<ValidationErrorsResponse>, string>(
                ClientEvents.RequestValidationFailed,
                (errors, id) =>
                {
                    if (id == requestId)
                    {
                        validationFailedSubscription.Dispose();
                        tcs.SetException(new ChatValidationException(errors.Select(x => x.ErrorMessage).ToList()));
                    }
                });
        }

        public async Task Disconnect()
        {
            await _connection.InvokeAsync(ServerMethods.DeleteClientAsync).ConfigureAwait(false);
            await _connection.StopAsync().ConfigureAwait(false);
        }

        private void SubscribeToEvents()
        {
            _subscriptions.Apply(x => x.Dispose());
            _subscriptions.Clear();

            _subscriptions.Add(_connection.On<MessageResponse>(ClientEvents.MessageAdded,
                message =>
                {
                    MessageAdded?.Invoke(message);
                }));

            _subscriptions.Add(_connection.On<Guid, ChannelSummaryResponse>(ClientEvents.MessageDeleted,
                (deletedMessageId, updatedChannelSummary) =>
                {
                    MessageDeleted?.Invoke(deletedMessageId.ToString(), updatedChannelSummary);
                }));

            _subscriptions.Add(_connection.On<MessageResponse>(ClientEvents.MessageUpdated,
                message =>
                {
                    MessageUpdated?.Invoke(message);
                }));

            _subscriptions.Add(_connection.On<MemberSummary, string>(ClientEvents.MemberLeft,
                (user, channelId) =>
                {
                    MemberLeft?.Invoke(user, channelId);
                }));

            _subscriptions.Add(_connection.On<MemberSummary, ChannelSummaryResponse>(ClientEvents.MemberJoined,
                (user, channel) =>
                {
                    MemberJoined?.Invoke(user, channel);
                }));

            _subscriptions.Add(_connection.On<ChannelSummaryResponse>(ClientEvents.ChannelUpdated,
                channel =>
                {
                    ChannelUpdated?.Invoke(channel);
                }));

            _subscriptions.Add(_connection.On<ChannelSummaryResponse>(ClientEvents.ChannelClosed,
                channel =>
                {
                    ChannelClosed?.Invoke(channel);
                }));

            _connection.On<ChannelSummaryResponse>(ClientEvents.ChannelAdded,
                channel =>
                {
                    ChannelAdded?.Invoke(channel);
                });

            _subscriptions.Add(_connection.On<string, string>(ClientEvents.AttachmentAdded,
                (username, message) =>
                {
                    AttachmentAdded?.Invoke(username, message);
                }));

            _subscriptions.Add(_connection.On<string, string>(ClientEvents.AttachmentDeleted,
                (username, message) =>
                {
                    AttachmentDeleted?.Invoke(username, message);
                }));

            _subscriptions.Add(_connection.On<string>(ClientEvents.TypingStarted,
                username =>
                {
                    TypingStarted?.Invoke(username);
                }));

            _subscriptions.Add(_connection.On<string>(ClientEvents.LastReadMessageChanged,
                channelId =>
                {
                    LastReadMessageChanged?.Invoke(channelId);
                }));
        }
    }
}
