// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Softeq.XToolkit.Auth;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Exceptions;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Channel;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Member;
using Softeq.XToolkit.Chat.SignalRClient.DTOs.Message;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.RemoteData;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.SignalRClient
{
    public class SignalRAdapter : ISocketChatAdapter
    {
        private readonly IAccountService _accountService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IRestHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly SignalRClient _signalRClient;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private readonly Subject<ChatMessageModel> _messageReceived = new Subject<ChatMessageModel>();
        private readonly Subject<ChatMessageModel> _messageEdited = new Subject<ChatMessageModel>();
        private readonly Subject<(string DeletedMessageId, ChatSummaryModel UpdatedChatSummary)> _messageDeleted
            = new Subject<(string DeletedMessageId, ChatSummaryModel UpdatedChatSummary)>();
        private readonly Subject<ChatSummaryModel> _chatAdded = new Subject<ChatSummaryModel>();
        private readonly Subject<string> _chatRemoved = new Subject<string>();
        private readonly Subject<SocketConnectionStatus> _connectionStatusChanged = new Subject<SocketConnectionStatus>();

        private string _memberId;
        private bool _isConnected = false;
        private bool _canReconnectAutomatically = true;

        public SignalRAdapter(
            IAccountService accountService,
            IRefreshTokenService refreshTokenService,
            IRestHttpClient httpClient,
            ILogManager logManager,
            IChatConfig chatConfig)
        {
            _accountService = accountService;
            _refreshTokenService = refreshTokenService;
            _httpClient = httpClient;
            _logger = logManager.GetLogger<SignalRAdapter>();
            _signalRClient = new SignalRClient(chatConfig.BaseUrl);

            SubscribeToEvents();

            // TODO YP: need investigate auto-connect (when init before login)
            ConnectIfNotConnectedAsync().SafeTaskWrapper();
        }

        public IObservable<ChatMessageModel> MessageReceived => _messageReceived;

        public IObservable<(string DeletedMessageId, ChatSummaryModel UpdatedChatSummary)> MessageDeleted => _messageDeleted;

        public IObservable<ChatMessageModel> MessageEdited => _messageEdited;

        public IObservable<ChatSummaryModel> ChatAdded => _chatAdded;

        public IObservable<string> ChatRemoved => _chatRemoved;

        public IObservable<string> MessageRead => null;

        public IObservable<(string ChatId, bool IsMuted)> IsChatMutedChanged => null;

        public IObservable<(string ChatId, int NewCount)> UnreadMessageCountChanged => null;

        public IObservable<SocketConnectionStatus> ConnectionStatusChanged => _connectionStatusChanged;

        public SocketConnectionStatus ConnectionStatus { get; private set; } = SocketConnectionStatus.Connecting;

        public Task CloseChatAsync(string chatId)
        {
            return CheckConnectionAndSendRequest(new TaskReference(() =>
            {
                return _signalRClient.CloseChannelAsync(new Guid(chatId));
            }));
        }

        public Task LeaveChatAsync(string chatId)
        {
            return CheckConnectionAndSendRequest(new TaskReference(() =>
            {
                return _signalRClient.LeaveChannelAsync(new Guid(chatId));
            }));
        }

        public Task<ChatSummaryModel> CreateChatAsync(string chatName, IList<string> participantsIds, string chatAvatar)
        {
            return CheckConnectionAndSendRequest(new TaskReference<ChatSummaryModel>(async () =>
            {
                var createChannelRequest = new CreateChannelRequest
                {
                    AllowedMembers = participantsIds,
                    Type = ChannelTypeDto.Private,
                    Name = chatName,
                    PhotoUrl = chatAvatar
                };
                var dto = await _signalRClient.CreateChannelAsync(createChannelRequest).ConfigureAwait(false);
                return Mapper.DtoToChatSummary(dto);
            }));
        }

        public Task InviteMembersAsync(string chatId, IList<string> participantsIds)
        {
            return CheckConnectionAndSendRequest(new TaskReference(() =>
            {
                var inviteMembersRequest = new InviteMembersRequest
                {
                    InvitedMembersIds = participantsIds,
                    ChannelId = new Guid(chatId),
                };
                return _signalRClient.InviteMembersAsync(inviteMembersRequest);
            }));
        }

        public Task DeleteMemberAsync(string chatId, string memberId)
        {
            return CheckConnectionAndSendRequest(new TaskReference(() =>
            {
                var deleteMemberRequest = new DeleteMemberRequest
                {
                    ChannelId = new Guid(chatId),
                    MemberId = new Guid(memberId)
                };
                return _signalRClient.DeleteMemberAsync(deleteMemberRequest);
            }));
        }

        public Task<ChatMessageModel> SendMessageAsync(string chatId, string messageBody, string imageUrl)
        {
            return CheckConnectionAndSendRequest(new TaskReference<ChatMessageModel>(async () =>
            {
                var createMessageRequest = new CreateMessageRequest
                {
                    ChannelId = new Guid(chatId),
                    Body = messageBody,
                    ImageUrl = imageUrl
                };
                var dto = await _signalRClient.CreateMessageAsync(createMessageRequest);
                return Mapper.DtoToChatMessage(dto);
            }));
        }

        public Task EditMessageAsync(string messageId, string newBody)
        {
            return CheckConnectionAndSendRequest(new TaskReference(() =>
            {
                var request = new UpdateMessageRequest
                {
                    MessageId = new Guid(messageId),
                    Body = newBody
                };
                return _signalRClient.UpdateMessageAsync(request);
            }));
        }

        public Task DeleteMessageAsync(string channelId, string messageId)
        {
            return CheckConnectionAndSendRequest(new TaskReference(() =>
            {
                var request = new DeleteMessageRequest
                {
                    MessageId = new Guid(messageId),
                    ChannelId = new Guid(channelId)
                };
                return _signalRClient.DeleteMessageAsync(request);
            }));
        }

        public void ForceReconnect()
        {
            ConnectIfNotConnectedAsync(true).SafeTaskWrapper();
            _canReconnectAutomatically = true;
        }

        public void ForceDisconnect()
        {
            _signalRClient.Disconnect().SafeTaskWrapper();
            _isConnected = false;
            _canReconnectAutomatically = false;
        }

        public Task EditChatAsync(ChatSummaryModel x)
        {
            return CheckConnectionAndSendRequest(new TaskReference(() =>
            {
                var request = new UpdateChannelRequest
                {
                    ChannelId = x.Id,
                    Name = x.Name,
                    PhotoUrl = x.AvatarUrl,
                    WelcomeMessage = x.WelcomeMessage,
                };

                return _signalRClient.UpdateChannelAsync(request);
            }));
        }

        private void SubscribeToEvents()
        {
            _signalRClient.AccessTokenExpired += () =>
            {
                _refreshTokenService.RefreshToken(_httpClient);
            };

            _signalRClient.MessageAdded += message =>
            {
                var messageModel = Mapper.DtoToChatMessage(message);
                messageModel.UpdateIsMineStatus(_memberId);
                _messageReceived.OnNext(messageModel);
            };
            _signalRClient.MessageUpdated += message =>
            {
                _messageEdited.OnNext(Mapper.DtoToChatMessage(message));
            };
            _signalRClient.MessageDeleted += (deletedMessageId, updatedChatSummary) =>
            {
                _messageDeleted.OnNext((deletedMessageId, Mapper.DtoToChatSummary(updatedChatSummary)));
            };
            _signalRClient.ChannelAdded += channel =>
            {
                var chat = Mapper.DtoToChatSummary(channel);
                chat.UpdateIsCreatedByMeStatus(_memberId);
                _chatAdded.OnNext(chat);
            };
            _signalRClient.ChannelClosed += channel =>
            {
                _chatRemoved.OnNext(channel.Id.ToString());
            };
            _signalRClient.MemberLeft += (user, channelId) =>
            {
                if (user == null)
                {
                    return;
                }

                if (user.Id.Equals(_memberId))
                {
                    _chatRemoved.OnNext(channelId);
                }
            };
            _signalRClient.MemberJoined += (user, channel) =>
            {
                if (user == null)
                {
                    return;
                }

                if (user.Id.Equals(_memberId))
                {
                    var chat = Mapper.DtoToChatSummary(channel);
                    chat.UpdateIsCreatedByMeStatus(_memberId);
                    _chatAdded.OnNext(chat);
                }
            };
            _signalRClient.Disconnected += OnDisconnected;
        }

        private async Task CheckConnectionAndSendRequest(TaskReference funcSendRequest)
        {
            try
            {
                await ConnectIfNotConnectedAsync().ConfigureAwait(false);
                if (_isConnected)
                {
                    await funcSendRequest.RunAsync().ConfigureAwait(false);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(ex);
                _isConnected = false;
                await CheckConnectionAndSendRequest(funcSendRequest).ConfigureAwait(false);
            }
            catch (WebSocketException ex)
            {
                _logger.Error(ex);
                _isConnected = false;
                await CheckConnectionAndSendRequest(funcSendRequest).ConfigureAwait(false);
            }
            catch (HubException ex)
            {
                _logger.Error(ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async Task<T> CheckConnectionAndSendRequest<T>(TaskReference<T> funcSendRequest)
        {
            try
            {
                await ConnectIfNotConnectedAsync().ConfigureAwait(false);
                if (_isConnected)
                {
                    return await funcSendRequest.RunAsync().ConfigureAwait(false);
                }
            }
            catch (InvalidOperationException ex)
            {
                _logger.Error(ex);
                _isConnected = false;
                return await CheckConnectionAndSendRequest(funcSendRequest).ConfigureAwait(false);
            }
            catch (WebSocketException ex)
            {
                _logger.Error(ex);
                _isConnected = false;
                return await CheckConnectionAndSendRequest(funcSendRequest).ConfigureAwait(false);
            }
            catch (HubException ex)
            {
                _logger.Error(ex);
            }
            catch (ChatValidationException ex)
            {
                //TODO YP: need an approach how to handle this exception for user
                _logger.Error(ex);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return default(T);
        }

        private async Task ConnectIfNotConnectedAsync(bool isForceConnect = false)
        {
            var accessToken = _accountService.AccessToken;
            await _semaphoreSlim.WaitAsync();
            try
            {
                if (isForceConnect || !_isConnected)
                {
                    UpdateConnectionStatus(SocketConnectionStatus.Connecting);
                    var client = await _signalRClient.ConnectAsync(accessToken).ConfigureAwait(false);

                    //TODO: review this
                    if (client == null)
                    {
                        _logger.Error("SignalRAdapter: accessToken is not valid, please relogin");
                    }

                    _memberId = client.MemberId;

                    _isConnected = true;
                    UpdateConnectionStatus(SocketConnectionStatus.Connected);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                _isConnected = false;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
            if (!_isConnected)
            {
                await Task.Delay(5000).ConfigureAwait(false);
                await ConnectIfNotConnectedAsync(isForceConnect);
            }
        }

        private void OnDisconnected()
        {
            _isConnected = false;
            if (_canReconnectAutomatically)
            {
                ConnectIfNotConnectedAsync().SafeTaskWrapper();
            }
        }

        private void UpdateConnectionStatus(SocketConnectionStatus status)
        {
            ConnectionStatus = status;
            _connectionStatusChanged.OnNext(status);
        }
    }
}
