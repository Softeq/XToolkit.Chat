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
using Softeq.XToolkit.RemoteData;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient
{
    public class HttpChatAdapter : IHttpChatAdapter
    {
        private readonly IRestHttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IJsonSerializer _jsonSerializer;

        public HttpChatAdapter(IRestHttpClient httpClient,
                               ILogManager logManager,
                               IJsonSerializer jsonSerializer)
        {
            _httpClient = httpClient;
            _logger = logManager.GetLogger<HttpChatAdapter>();
            _jsonSerializer = jsonSerializer;
        }

        public Task<ChatUserModel> GetUserSummaryAsync()
        {
            var request = new GetUserSummaryRequest();
            return _httpClient.GetModelAsync<ChatUserModel, ChatUserDto>(request, _logger, Mapper.DtoToChatUser);
        }

        public Task<IList<ChatUserModel>> GetChatMembersAsync(string chatId)
        {
            var request = new GetChatMembersRequest(chatId);
            return _httpClient.GetModelAsync<IList<ChatUserModel>, IList<ChatUserDto>>(request, _logger, x =>
                                                                                       x.Select(Mapper.DtoToChatUser).ToList());
        }

        public Task<bool> CloseChatAsync(string chatId)
        {
            var request = new PutCloseChatRequest(chatId);
            return _httpClient.TrySendAsync(request, _logger);
        }

        public Task<ChatSummaryModel> CreateChatAsync(IEnumerable<string> participantsIds)
        {
            var dto = new CreateChatDto
            {
                AllowedMembers = participantsIds.ToList(),
            };
            var request = new PostCreateChatRequest(_jsonSerializer, dto);
            return _httpClient.GetModelAsync<ChatSummaryModel, ChatSummaryDto>(request, _logger, Mapper.DtoToChatSummary);
        }

        public async Task<IList<ChatSummaryModel>> GetChatsHeadersAsync()
        {
            var request = new GetChatsListRequest();
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
            var request = new GetOlderMessagesRequest(chatId, messageFromId, messageFromDateTime, count);
            var result = await _httpClient.GetPagingModelAsync<ChatMessageModel, ChatMessageDto>(request, _logger, Mapper.DtoToChatMessage)
                                          .ConfigureAwait(false);
            return result?.Data;
        }

        public async Task<IList<ChatMessageModel>> GetLatestMessagesAsync(string chatId)
        {
            var request = new GetLatestMessagesRequest(chatId);
            var response = await _httpClient.GetPagingModelAsync<ChatMessageModel, ChatMessageDto>(request, _logger, Mapper.DtoToChatMessage)
                                            .ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<IList<ChatMessageModel>> GetMessagesFromAsync(string chatId,
                                                                        string messageFromId,
                                                                        DateTimeOffset messageFromDateTime,
                                                                        int? count = null)
        {
            var request = new GetMessagesRequest(chatId, messageFromId, messageFromDateTime, count);
            var response = await _httpClient.GetPagingModelAsync<ChatMessageModel, ChatMessageDto>(request, _logger, Mapper.DtoToChatMessage)
                                            .ConfigureAwait(false);
            return response?.Data;
        }

        public async Task<IList<ChatMessageModel>> GetAllMessagesAsync(string chatId)
        {
            var request = new GetMessagesRequest(chatId);
            var response = await _httpClient.GetPagingModelAsync<ChatMessageModel, ChatMessageDto>(request, _logger, Mapper.DtoToChatMessage)
                                            .ConfigureAwait(false);
            return response?.Data;
        }


        public async Task MarkMessageAsReadAsync(string chatId, string messageId)
        {
            var request = new PostMarkAsReadRequest(chatId, messageId);
            await _httpClient.TrySendAsync(request, _logger).ConfigureAwait(false);
        }

        public Task<ChatMessageModel> SendMessageAsync(string chatId, string messageBody)
        {
            var request = new PostSendMessageRequest(chatId, _jsonSerializer, new SendMessageDto { Body = messageBody });
            return _httpClient.GetModelAsync<ChatMessageModel, ChatMessageDto>(request, _logger, Mapper.DtoToChatMessage);
        }

        public Task<IList<ChatUserModel>> GetContactsAsync()
        {
            var request = new GetMembersRequest();
            return _httpClient.GetModelAsync<IList<ChatUserModel>, IList<ChatUserDto>>(request, _logger,
                                            x => x.Where(y => y.AvatarUrl != null || !string.IsNullOrEmpty(y.UserName))
                                            .Select(Mapper.DtoToChatUser)
                                            .ToList());
        }
    }
}
