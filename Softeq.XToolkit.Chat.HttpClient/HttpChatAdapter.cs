// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.HttpClient.Dtos;
using Softeq.XToolkit.Chat.HttpClient.Requests;
using Softeq.XToolkit.Chat.Models;
using Softeq.XToolkit.Chat.Models.Interfaces;
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
            return _httpClient.GetModelAsync<ChatUserModel, ChatUserDto>(request, _logger, Mapper.DtoToChatUser);
        }

        public Task<IList<ChatUserModel>> GetChatMembersAsync(string chatId)
        {
            var request = new GetChatMembersRequest(_chatConfig.ApiUrl, chatId);
            return _httpClient.GetModelAsync<IList<ChatUserModel>, IList<ChatUserDto>>(request, _logger, x =>
                                                                                       x.Select(Mapper.DtoToChatUser).ToList());
        }

        public Task<bool> CloseChatAsync(string chatId)
        {
            var request = new PutCloseChatRequest(_chatConfig.ApiUrl, chatId);
            return _httpClient.TrySendAsync(request, _logger);
        }

        public Task<ChatSummaryModel> CreateChatAsync(IEnumerable<string> participantsIds)
        {
            var dto = new CreateChatDto
            {
                AllowedMembers = participantsIds.ToList(),
            };
            var request = new PostCreateChatRequest(_chatConfig.ApiUrl, _jsonSerializer, dto);
            return _httpClient.GetModelAsync<ChatSummaryModel, ChatSummaryDto>(request, _logger, Mapper.DtoToChatSummary);
        }

        public async Task<IList<ChatSummaryModel>> GetChatsHeadersAsync()
        {
            var request = new GetChatsListRequest(_chatConfig.ApiUrl);
            var result = await _httpClient.GetModelOrExceptionAsync<IList<ChatSummaryModel>, IList<ChatSummaryDto>>(request, _logger,
                                                                                                                    x => x.Select(Mapper.DtoToChatSummary).ToList())
                                          .ConfigureAwait(false);
            return result.Model;
        }

        public async Task<IList<ChatMessageModel>> GetOlderMessagesAsync(string chatId,
                                                                         string messageFromId = null,
                                                                         DateTimeOffset? messageFromDateTime = null,
                                                                         int? count = null)
        {
            var request = new GetOlderMessagesRequest(_chatConfig.ApiUrl, chatId, messageFromId, messageFromDateTime, count);
            var result = await _httpClient.GetPagingModelAsync<ChatMessageModel, ChatMessageDto>(request, _logger, Mapper.DtoToChatMessage)
                                          .ConfigureAwait(false);
            return result?.Data;
        }

        public async Task<IList<ChatMessageModel>> GetLatestMessagesAsync(string chatId)
        {
            var request = new GetLatestMessagesRequest(_chatConfig.ApiUrl, chatId);
            var response = await _httpClient.GetPagingModelAsync<ChatMessageModel, ChatMessageDto>(request, _logger, Mapper.DtoToChatMessage)
                                            .ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<IList<ChatMessageModel>> GetMessagesFromAsync(string chatId,
                                                                        string messageFromId,
                                                                        DateTimeOffset messageFromDateTime,
                                                                        int? count = null)
        {
            var request = new GetMessagesRequest(_chatConfig.ApiUrl, chatId, messageFromId, messageFromDateTime, count);
            var response = await _httpClient.GetPagingModelAsync<ChatMessageModel, ChatMessageDto>(request, _logger, Mapper.DtoToChatMessage)
                                            .ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<IList<ChatMessageModel>> GetAllMessagesAsync(string chatId)
        {
            var request = new GetMessagesRequest(_chatConfig.ApiUrl, chatId);
            var response = await _httpClient.GetPagingModelAsync<ChatMessageModel, ChatMessageDto>(request, _logger, Mapper.DtoToChatMessage)
                                            .ConfigureAwait(false);
            return response?.Data;
        }

        public async Task MarkMessageAsReadAsync(string chatId, string messageId)
        {
            var request = new PostMarkAsReadRequest(_chatConfig.ApiUrl, chatId, messageId);
            await _httpClient.TrySendAsync(request, _logger).ConfigureAwait(false);
        }

        public Task<ChatMessageModel> SendMessageAsync(string chatId, string messageBody)
        {
            var request = new PostSendMessageRequest(_chatConfig.ApiUrl, chatId, _jsonSerializer, new SendMessageDto { Body = messageBody });
            return _httpClient.GetModelAsync<ChatMessageModel, ChatMessageDto>(request, _logger, Mapper.DtoToChatMessage);
        }

        public async Task<PagingModel<ChatUserModel>> GetContactsAsync(string nameFilter, int pageSize, int pageNumber)
        {
            var request = new GetMembersRequest(_chatConfig.ApiUrl, nameFilter, pageSize, pageNumber);
            var result = await _httpClient.TrySendAndDeserializeAsync<PagingModelDto<ChatUserDto>>(request, _logger).ConfigureAwait(false);

            return result == null
                ? null
                : Mapper.PagedMembersDtoToPagingModel(result);
        }

        public async Task<PagingModel<ChatUserModel>> GetContactsForInviteAsync(string chatId, string nameFilter, int pageSize, int pageNumber)
        {
            var request = new GetMembersForInviteRequest(_chatConfig.ApiUrl, chatId, nameFilter, pageSize, pageNumber);
            var result = await _httpClient.TrySendAndDeserializeAsync<PagingModelDto<ChatUserDto>>(request, _logger).ConfigureAwait(false);

            return result == null
                ? null
                : Mapper.PagedMembersDtoToPagingModel(result);
        }
    }
}
