// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Softeq.XToolkit.Chat.SignalRClient.DTOs;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Channel;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Client;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Member;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Message;

namespace Softeq.XToolkit.Chat.SignalRClient
{
    internal class SignalRClient
    {
        private HubConnection _connection;

        public SignalRClient(string url)
        {
            SourceUrl = url;
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
        public event Action<string> TypingEnded;

        public event Action<MemberSummary, ChannelSummaryResponse> MemberJoined;
        public event Action<MemberSummary, string> MemberLeft;

        public string SourceUrl { get; }

        public async Task<ClientResponse> ConnectAsync(string accessToken)
        {
            Console.WriteLine("Connecting to {0}", SourceUrl);
            _connection = new HubConnectionBuilder()
                .WithUrl($"{SourceUrl}/chat", options =>
                {
                    options.Headers.Add("Authorization", "Bearer " + accessToken);
                })
                .Build();

            _connection.Closed += e =>
            {
                Console.WriteLine("Connection closed...");
                Disconnected?.Invoke();
                return Task.CompletedTask;
            };

            // Handle the connected connection
            while (true)
            {
                try
                {
                    await _connection.StartAsync().ConfigureAwait(false);
                    Console.WriteLine("Connected to {0}", SourceUrl);
                    break;
                }
                catch (IOException ex)
                {
                    // Process being shutdown
                    Console.WriteLine(ex);
                    break;
                }
                catch (OperationCanceledException ex)
                {
                    // The connection closed
                    Console.WriteLine(ex);
                    break;
                }
                catch (ObjectDisposedException ex)
                {
                    // We're shutting down the client
                    Console.WriteLine(ex);
                    break;
                }
                catch (Exception ex)
                {
                    // Send could have failed because the connection closed
                    Console.WriteLine(ex);
                    Console.WriteLine("Failed to connect, trying again in 5000(ms)");
                    await Task.Delay(5000).ConfigureAwait(false);
                }
            }

            _connection.On<string>(ClientEvents.AccessTokenExpired, (requestId) =>
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

        public Task UpdateChannelAsync(UpdateChannelRequest request)
        {
            return SendAndHandleExceptionsAsync(ServerMethods.UpdateChannelAsync, request);
        }

        public Task CloseChannelAsync(Guid channelId)
        {
            return SendAndHandleExceptionsAsync(ServerMethods.CloseChannelAsync, new ChannelRequest { ChannelId = channelId });
        }

        public Task InviteMemberAsync(InviteMemberRequest request)
        {
            return SendAndHandleExceptionsAsync(ServerMethods.InviteMemberAsync, request);
        }

        public Task InviteMembersAsync(InviteMembersRequest request)
        {
            return SendAndHandleExceptionsAsync(ServerMethods.InviteMembersAsync, request);
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

        public async Task SendAndHandleExceptionsAsync(string methodName, BaseRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();
            var requestId = Guid.NewGuid().ToString();
            IDisposable exceptionSubscription = null;
            exceptionSubscription = _connection.On<Exception, string>(ClientEvents.ExceptionOccurred, (ex, id) =>
            {
                if (id == requestId)
                {
                    exceptionSubscription.Dispose();
                    tcs.SetException(ex);
                }
            });
            IDisposable successSubscription = null;
            successSubscription = _connection.On<string>(ClientEvents.RequestSuccess, id =>
            {
                if (id == requestId)
                {
                    successSubscription.Dispose();
                    tcs.SetResult(true);
                }
            });
            IDisposable validationFailedSubscription = null;
            validationFailedSubscription = _connection.On<Exception, string>(ClientEvents.RequestValidationFailed, (ex, id) =>
            {
                if (id == requestId)
                {
                    validationFailedSubscription.Dispose();
                    tcs.SetException(ex);
                }
            });
            request.RequestId = requestId;
            await _connection.InvokeAsync(methodName, request).ConfigureAwait(false);
            await tcs.Task.ConfigureAwait(false);
        }

        public async Task<T> SendAndHandleExceptionsAsync<T>(string methodName, BaseRequest request)
        {
            var tcs = new TaskCompletionSource<T>();
            var requestId = Guid.NewGuid().ToString();
            IDisposable exceptionSubscription = null;
            exceptionSubscription = _connection.On<Exception, string>(ClientEvents.ExceptionOccurred, (ex, id) =>
            {
                if (id == requestId)
                {
                    exceptionSubscription.Dispose();
                    tcs.SetException(ex);
                }
            });
            IDisposable successSubscription = null;
            bool isCallEnded = false;
            T result = default(T);
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

        public async Task Disconnect()
        {
            await _connection.InvokeAsync(ServerMethods.DeleteClientAsync).ConfigureAwait(false);
            await _connection.StopAsync().ConfigureAwait(false);
        }

        private void SubscribeToEvents()
        {
            _connection.On<MessageResponse>(ClientEvents.MessageAdded, (message) =>
            {
                MessageAdded?.Invoke(message);
            });

            _connection.On<Guid, ChannelSummaryResponse>(ClientEvents.MessageDeleted, (deletedMessageId, updatedChannelSummary) =>
            {
                MessageDeleted?.Invoke(deletedMessageId.ToString(), updatedChannelSummary);
            });

            _connection.On<MessageResponse>(ClientEvents.MessageUpdated, (message) =>
            {
                MessageUpdated?.Invoke(message);
            });

            _connection.On<MemberSummary, string>(ClientEvents.MemberLeft, (user, channelId) =>
            {
                MemberLeft?.Invoke(user, channelId);
            });

            _connection.On<MemberSummary, ChannelSummaryResponse>(ClientEvents.MemberJoined, (user, channel) =>
            {
                MemberJoined?.Invoke(user, channel);
            });

            _connection.On<ChannelSummaryResponse>(ClientEvents.ChannelUpdated, (channel) =>
            {
                ChannelUpdated?.Invoke(channel);
            });

            _connection.On<ChannelSummaryResponse>(ClientEvents.ChannelClosed, (channel) =>
            {
                ChannelClosed?.Invoke(channel);
            });

            _connection.On<ChannelSummaryResponse>(ClientEvents.ChannelAdded, (channel) =>
            {
                ChannelAdded?.Invoke(channel);
            });

            _connection.On<string, string>(ClientEvents.AttachmentAdded, (username, message) =>
            {
                AttachmentAdded?.Invoke(username, message);
            });

            _connection.On<string, string>(ClientEvents.AttachmentDeleted, (username, message) =>
            {
                AttachmentDeleted?.Invoke(username, message);
            });

            _connection.On<string>(ClientEvents.TypingStarted, (username) =>
            {
                TypingStarted?.Invoke(username);
            });

            _connection.On<string>(ClientEvents.TypingEnded, (username) =>
            {
                TypingEnded?.Invoke(username);
            });
        }
    }
}
