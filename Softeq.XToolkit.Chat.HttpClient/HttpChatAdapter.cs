// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Request.Channel;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Request.Member;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Request.Message;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Response;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Response.Channel;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Response.Member;
using Softeq.NetKit.Chat.TransportModels.Models.CommonModels.Response.Message;
using Softeq.XToolkit.Chat.HttpClient.Dtos;
using Softeq.XToolkit.Chat.HttpClient.Requests;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Chat.Models.Queries;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.Common.Models;
using Softeq.XToolkit.RemoteData;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient
{
    public class HttpChatAdapter : IHttpChatAdapter
    {
        private readonly IRestHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IChatConfig _chatConfig;

        public HttpChatAdapter(
            IRestHttpClient httpClient,
            ILogManager logManager,
            IJsonSerializer jsonSerializer,
            IChatConfig chatConfig)
        {
            _httpClient = httpClient;
            _logger = logManager.GetLogger<HttpChatAdapter>();
            _jsonSerializer = jsonSerializer;
            _chatConfig = chatConfig;
        }

        public Task<ChatUserModel> GetUserSummaryAsync()
        {
            var request = new GetUserSummaryRequest(_chatConfig.ApiUrl);

            return _httpClient.GetModelAsync<ChatUserModel, MemberSummaryResponse>(request, _logger, Mapper.DtoToChatUser);
        }

        public Task<IList<ChatUserModel>> GetChatMembersAsync(string chatId)
        {
            var request = new GetChatMembersRequest(_chatConfig.ApiUrl, chatId);

            return _httpClient.GetModelAsync<IList<ChatUserModel>, IList<MemberSummaryResponse>>(request, _logger,
                x => x.Select(Mapper.DtoToChatUser).ToList());
        }

        public Task<bool> CloseChatAsync(string chatId)
        {
            var request = new PutCloseChatRequest(_chatConfig.ApiUrl, chatId);

            return _httpClient.TrySendAsync(request, _logger);
        }

        [Obsolete("Used SignalR method.")]
        public Task<ChatSummaryModel> CreateChatAsync(IEnumerable<string> participantsIds)
        {
            var dto = new CreateChannelRequest
            {
                AllowedMembers = participantsIds.ToList(),
            };

            var request = new PostCreateChatRequest(_chatConfig.ApiUrl, _jsonSerializer, dto);

            return _httpClient.GetModelAsync<ChatSummaryModel, ChannelSummaryResponse>(request, _logger, Mapper.DtoToChatSummary);
        }

        public Task<ChatSummaryModel> CreateDirectChatAsync(string memberId)
        {
            var dto = new CreateDirectChannelRequest
            {
                MemberId = new Guid(memberId)
            };

            var request = new PostCreateDirectChatRequest(_chatConfig.ApiUrl, _jsonSerializer, dto);

            return _httpClient.GetModelAsync<ChatSummaryModel, ChannelSummaryResponse>(request, _logger, Mapper.DtoToChatSummary);
        }

        public async Task<IList<ChatSummaryModel>> GetChannelsAsync()
        {
            var request = new GetChatsListRequest(_chatConfig.ApiUrl);

            var result = await _httpClient.GetModelOrExceptionAsync<IList<ChatSummaryModel>, IList<ChannelSummaryResponse>>(request,
                _logger, x => x.Select(Mapper.DtoToChatSummary).ToList()).ConfigureAwait(false);

            return result.Model;
        }

        public Task<IList<ChatMessageModel>> GetOlderMessagesAsync(MessagesQuery query)
        {
            var messagesQuery = new GetMessagesQuery
            {
                ChannelId = query.ChannelId,
                FromId = query.FromId,
                FromDateTime = query.FromDateTime,
                Count = query.Count
            };
            var request = new GetOlderMessagesRequest(_chatConfig.ApiUrl, messagesQuery);
            return LoadMessagesAsync(request);
        }

        public Task<IList<ChatMessageModel>> GetLatestMessagesAsync(string chatId)
        {
            return LoadMessagesAsync(new GetLatestMessagesRequest(_chatConfig.ApiUrl, chatId));
        }

        public Task<IList<ChatMessageModel>> GetMessagesFromAsync(MessagesQuery query)
        {
            var messagesQuery = new GetMessagesQuery
            {
                ChannelId = query.ChannelId,
                FromId = query.FromId,
                FromDateTime = query.FromDateTime,
                Count = query.Count
            };
            var request = new GetMessagesRequest(_chatConfig.ApiUrl, messagesQuery);
            return LoadMessagesAsync(request);
        }

        public Task<IList<ChatMessageModel>> GetAllMessagesAsync(string chatId)
        {
            var query = new GetMessagesQuery
            {
                ChannelId = chatId,
            };
            var request = new GetMessagesRequest(_chatConfig.ApiUrl, query);
            return LoadMessagesAsync(request);
        }

        public async Task MarkMessageAsReadAsync(string channelId, string messageId)
        {
            var dto = new SetLastReadMessageRequest
            {
                ChannelId = new Guid(channelId),
                MessageId = new Guid(messageId)
            };
            var request = new PostMarkAsReadRequest(_chatConfig.ApiUrl, _jsonSerializer, dto);

            await _httpClient.TrySendAsync(request, _logger).ConfigureAwait(false);
        }

        public async Task<PagingModel<ChatUserModel>> GetContactsAsync(string nameFilter, int pageNumber, int pageSize)
        {
            var request = new GetMembersRequest(_chatConfig.ApiUrl, nameFilter, pageNumber, pageSize);

            var result = await _httpClient.TrySendAndDeserializeAsync<QueryResult<MemberSummaryResponse>>(request,
                _logger).ConfigureAwait(false);

            return result == null
                ? null
                : Mapper.PagedDtoToPagingModel(result, Mapper.DtoToChatUser);
        }

        public async Task<PagingModel<ChatUserModel>> GetContactsForInviteAsync(ContactsQuery query)
        {
            var dto = new GetPotentialChannelMembersRequest
            {
                NameFilter = query.NameFilter,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };

            var request = new GetMembersForInviteRequest(_chatConfig.ApiUrl, _jsonSerializer, dto, query.ChannelId);

            var result = await _httpClient.TrySendAndDeserializeAsync<QueryResult<MemberSummaryResponse>>(request,
                _logger).ConfigureAwait(false);

            return result == null
                ? null
                : Mapper.PagedDtoToPagingModel(result, Mapper.DtoToChatUser);
        }

        public Task MuteChatAsync(string chatId)
        {
            var request = new PostMuteChatRequest(_chatConfig.ApiUrl, chatId);

            return _httpClient.TrySendAsync(request, _logger);
        }

        public Task UnMuteChatAsync(string chatId)
        {
            var request = new PostUnMuteChatRequest(_chatConfig.ApiUrl, chatId);

            return _httpClient.TrySendAsync(request, _logger);
        }

        public Task<bool> SubscribeForPushNotificationsAsync(string token, int devicePlatform)
        {
            var dto = new PushTokenDto
            {
                Token = token,
                DevicePlatform = devicePlatform
            };

            var request = new PostSubscribePushTokenRequest(_chatConfig.ApiUrl, _jsonSerializer, dto);

            return _httpClient.TrySendAsync(request, _logger);
        }

        public Task<bool> UnsubscribeFromPushNotificationsAsync(string token, int devicePlatform)
        {
            var dto = new PushTokenDto
            {
                Token = token,
                DevicePlatform = devicePlatform
            };

            var request = new PostUnsubscribePushTokenRequest(_chatConfig.ApiUrl, _jsonSerializer, dto);

            return _httpClient.TrySendAsync(request, _logger);
        }

        private async Task<IList<ChatMessageModel>> LoadMessagesAsync(BaseRestRequest request)
        {
            var result = await _httpClient.TrySendAndDeserializeAsync<QueryResult<MessageResponse>>(request,
                _logger).ConfigureAwait(false);

            return Mapper.PagedDtoToPagingModel(result, Mapper.DtoToChatMessage)?.Data;
        }
    }
}
