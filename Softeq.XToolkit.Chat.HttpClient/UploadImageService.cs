// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.IO;
using System.Threading.Tasks;
using Softeq.XToolkit.Chat.HttpClient.Requests;
using Softeq.XToolkit.Chat.Models.Interfaces;
using Softeq.XToolkit.Common.Interfaces;
using Softeq.XToolkit.RemoteData;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient
{
    public class UploadImageService : IUploadImageService
    {
        private readonly IChatConfiguration _chatConfiguration;
        private readonly IRestHttpClient _httpClient;
        private readonly ILogger _logger;

        public UploadImageService(
            IChatConfiguration chatConfiguration,
            IRestHttpClient httpClient,
            ILogManager logManager)
        {
            _chatConfiguration = chatConfiguration;
            _httpClient = httpClient;
            _logger = logManager.GetLogger<UploadImageService>();
        }
        
        public async Task<string> UploadImageAsync(Stream image, string extension)
        {
            var getTokenRequest = new GetAzureTokenRequest(_chatConfiguration.ApiUrl);
            var token = default(string);
            try
            {
                token = await _httpClient.SendAndGetResponseAsync(getTokenRequest).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            if (token == null)
            {
                return null;
            }

            var uploadRequest = new UploadImageRequest(_chatConfiguration.BlobUrl, token.Replace("\"", string.Empty), image, extension);
            var uploadResult = await _httpClient.TrySendAsync(uploadRequest, _logger).ConfigureAwait(false);

            return uploadResult ? uploadRequest.FilePath : null;
        }
    }
}