// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using Softeq.XToolkit.RemoteData.HttpClient;

namespace Softeq.XToolkit.Chat.HttpClient.Requests
{
    internal class UploadImageRequest : BaseRestRequest
    {
        private readonly string _apiUrl;
        private readonly string _urlParams;
        private readonly Stream _stream;
        private readonly string _fileName;
        private readonly string _fileMimeType;

        public UploadImageRequest(string apiUrl, string urlParams, Stream stream, string extension)
        {
            _apiUrl = apiUrl;
            _urlParams = urlParams;
            _stream = stream;
            _fileName = $"{Guid.NewGuid()}{extension}";
            _fileMimeType = ConvertToMimeType(extension);
        }

        public override HttpMethod Method => HttpMethod.Put;

        public override string EndpointUrl => $"{FilePath}{_urlParams}";

        public override bool UseOriginalEndpoint => true;

        public string FilePath => $"{_apiUrl}/{_fileName}";

        public override IList<(string Header, string Value)> CustomHeaders => new List<(string Header, string Value)>
        {
            ("x-ms-version", "2018-03-28"),
            ("x-ms-date", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)),
            ("x-ms-blob-type", "BlockBlob"),
            ("x-ms-blob-content-disposition", $"attachment; filename=\"{_fileName}\""),
            ("x-ms-blob-content-type", _fileMimeType)
        };

        public override bool HasCustomHeaders => true;

        public override HttpContent GetContent()
        {
            _stream.Seek(0, SeekOrigin.Begin);
            return new StreamContent(_stream);
        }

        private string ConvertToMimeType(string extension)
        {
            var imageType = extension
                .Replace(".", "")
                .Replace("jpg", "jpeg");

            return $"image/{imageType}";
        }

    }
}