using System;
using System.IO;
using System.Threading.Tasks;

namespace Softeq.XToolkit.Chat.Models.Interfaces
{
    public interface IUploadImageService
    {
        Task<string> UploadImageAsync(Stream stream, string extension);

        Task<string> UploadImageAsync(Func<(Task<Stream> GetStreamTask, string Extension)> func);
    }
}