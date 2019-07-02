// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Request.Channel;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Request.Member;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Request.Message;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Enum;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Common;
using Softeq.XToolkit.Common.Extensions;
using Softeq.XToolkit.Common.Interfaces;
using ChannelType = Softeq.NetKit.Chat.TransportModels.Enums.ChannelType;

namespace Softeq.XToolkit.Chat.SignalRClient
{
    public class SignalRAdapter : ISocketChatAdapter
    {
        private readonly ILogger _logger;
        private readonly SignalRClient _signalRClient;
        private readonly IChatAuthService _authService;

        private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        private readonly ISubject<ChatMessageModel> _messageReceived = new Subject<ChatMessageModel>();
        private readonly ISubject<ChatDeletedMessageModel> _messageDeleted = new Subject<ChatDeletedMessageModel>();
        private readonly ISubject<ChatMessageModel> _messageEdited = new Subject<ChatMessageModel>();
        private readonly ISubject<ChatSummaryModel> _chatAdded = new Subject<ChatSummaryModel>();
        private readonly ISubject<ChatSummaryModel> _chatUpdated = new Subject<ChatSummaryModel>();
        private readonly ISubject<string> _chatRemoved = new Subject<string>();
        private readonly ISubject<string> _chatRead = new Subject<string>();
        private readonly ISubject<SocketConnectionStatus> _connectionStatusChanged = new Subject<SocketConnectionStatus>();

        private string _memberId;
        private bool _isConnected;
        private bool _canReconnectAutomatically = true;

        public SignalRAdapter(
            IChatAuthService authService,
            ILogManager logManager,
            IChatConfig chatConfig)
        {
            _authService = authService;
            _logger = logManager.GetLogger<SignalRAdapter>();
            _signalRClient = new SignalRClient(chatConfig.BaseUrl, _authService.GetAccessToken);

            SubscribeToEvents();

            // TODO YP: need investigate auto-connect (when init before login)
            ConnectIfNotConnectedAsync().SafeTaskWrapper();
        }

        public IObservable<ChatMessageModel> MessageReceived => _messageReceived;
        public IObservable<ChatDeletedMessageModel> MessageDeleted => _messageDeleted;
        public IObservable<ChatMessageModel> MessageEdited => _messageEdited;
        public IObservable<ChatSummaryModel> ChatAdded => _chatAdded;
        public IObservable<string> ChatRemoved => _chatRemoved;
        public IObservable<ChatSummaryModel> ChatUpdated => _chatUpdated;
        public IObservable<string> ChatRead => _chatRead;
        public IObservable<(string ChatId, bool IsMuted)> IsChatMutedChanged => null;
        public IObservable<(string ChatId, int NewCount)> UnreadMessageCountChanged => null;
        public IObservable<SocketConnectionStatus> ConnectionStatusChanged => _connectionStatusChanged;

        public SocketConnectionStatus ConnectionStatus { get; private set; } = SocketConnectionStatus.Connecting;

        public Task CloseChatAsync(string channelId)
        {
            return CheckConnectionAndSendRequest(new TaskReference(() =>
            {
                var closeChannelRequest = new ChannelRequest
                {
                    ChannelId = new Guid(channelId)
                };
                return _signalRClient.CloseChannelAsync(closeChannelRequest);
            }));
        }

        public Task LeaveChatAsync(string channelId)
        {
            return CheckConnectionAndSendRequest(new TaskReference(() =>
            {
                var channelRequestModel = new ChannelRequest
                {
                    ChannelId = new Guid(channelId)
                };
                return _signalRClient.LeaveChannelAsync(channelRequestModel);
            }));
        }

        public Task<ChatSummaryModel> CreateChatAsync(string chatName, IList<string> participantsIds, string chatAvatar)
        {
            return CheckConnectionAndSendRequest(new TaskReference<ChatSummaryModel>(async () =>
            {
                var createChannelRequest = new CreateChannelRequest
                {
                    AllowedMembers = participantsIds.ToList(),
                    Type = ChannelType.Private,
                    Name = chatName,
                    PhotoUrl = chatAvatar
                };
                var dto = await _signalRClient.CreateChannelAsync(createChannelRequest).ConfigureAwait(false);
                return Mapper.DtoToChatSummary(dto);
            }));
        }

        public Task<ChatSummaryModel> CreateDirectChatAsync(string memberId)
        {
            return CheckConnectionAndSendRequest(new TaskReference<ChatSummaryModel>(async () =>
            {
                var createChannelRequest = new CreateDirectChannelRequest
                {
                    MemberId = new Guid(memberId)
                };
                var dto = await _signalRClient.CreateDirectChannelAsync(createChannelRequest).ConfigureAwait(false);
                return Mapper.DtoToChatSummary(dto);
            }));
        }

        public Task InviteMembersAsync(string chatId, IList<string> participantsIds)
        {
            return CheckConnectionAndSendRequest(new TaskReference(() =>
            {
                var inviteMembersRequest = new InviteMultipleMembersRequest
                {
                    InvitedMembersIds = participantsIds.Select(x => new Guid(x)).ToList(),
                    ChannelId = new Guid(chatId),
                };
                return _signalRClient.InviteMultipleMembersAsync(inviteMembersRequest);
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
                var createMessageRequest = new AddMessageRequest
                {
                    ChannelId = new Guid(chatId),
                    Body = messageBody,
                    ImageUrl = imageUrl
                };
                var dto = await _signalRClient.AddMessageAsync(createMessageRequest);
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
                    MessageId = new Guid(messageId)
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
            _signalRClient.DisconnectAsync().SafeTaskWrapper();
            _isConnected = false;
            _canReconnectAutomatically = false;
        }

        public Task EditChatAsync(ChatSummaryModel x)
        {
            return CheckConnectionAndSendRequest(new TaskReference(() =>
            {
                var request = new UpdateChannelRequest
                {
                    ChannelId = new Guid(x.Id),
                    Name = x.Name,
                    PhotoUrl = x.PhotoUrl,
                    WelcomeMessage = x.WelcomeMessage
                };

                return _signalRClient.UpdateChannelAsync(request);
            }));
        }

        private void SubscribeToEvents()
        {
            _signalRClient.AccessTokenExpired += () =>
            {
                _authService.RefreshToken();
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

            _signalRClient.MessageDeleted += (messageId, channelId) =>
            {
                _messageDeleted.OnNext(new ChatDeletedMessageModel
                {
                    MessageId = messageId.ToString(),
                    ChannelId = channelId.ToString()
                });
            };

            _signalRClient.ChannelClosed += channelId =>
            {
                _chatRemoved.OnNext(channelId.ToString());
            };

            _signalRClient.ChannelUpdated += updatedChannel =>
            {
                var chat = Mapper.DtoToChatSummary(updatedChannel);
                chat.UpdateIsCreatedByMeStatus(_memberId);
                _chatUpdated.OnNext(chat);
            };

            _signalRClient.LastReadMessageUpdated += channelId =>
            {
                _chatRead.OnNext(channelId.ToString());
            };

            _signalRClient.MemberLeft += channelId =>
            {
                _chatRemoved.OnNext(channelId.ToString());
            };

            _signalRClient.MemberJoined += channel =>
            {
                var chat = Mapper.DtoToChatSummary(channel);
                chat.UpdateIsCreatedByMeStatus(_memberId);
                _chatAdded.OnNext(chat);
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
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
            return default(T);
        }

        private async Task ConnectIfNotConnectedAsync(bool isForceConnect = false)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                if (isForceConnect || !_isConnected)
                {
                    UpdateConnectionStatus(SocketConnectionStatus.Connecting);
                    var client = await _signalRClient.ConnectAsync().ConfigureAwait(false);

                    _memberId = client.MemberId.ToString();

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
