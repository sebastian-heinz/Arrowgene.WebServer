using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Arrowgene.WebServer.Middleware;
using Microsoft.Extensions.FileProviders;

namespace Arrowgene.WebServer.WebMiddleware
{
    public class StaticFileMiddleware : IWebMiddleware
    {
        private readonly IFileProvider _provider;

        public StaticFileMiddleware(IFileProvider provider)
        {
            _provider = provider;
        }

        public List<IFileInfo> GetServingFiles()
        {
            List<IFileInfo> files = new List<IFileInfo>();
            GetServingFiles("", files);
            return files;
        }

        public void GetServingFiles(string folder, List<IFileInfo> files)
        {
            IDirectoryContents directoryContents = _provider.GetDirectoryContents(folder);
            foreach (IFileInfo fileInfo in directoryContents)
            {
                if (fileInfo.IsDirectory)
                {
                    string nextFolder = Path.Combine(folder, fileInfo.Name);
                    GetServingFiles(nextFolder, files);
                }
                else
                {
                    files.Add(fileInfo);
                }
            }
        }


        public async Task<WebResponse> Handle(WebRequest request, WebMiddlewareDelegate next)
        {
            WebResponse response = await next(request);
            if (!response.RouteFound && !string.IsNullOrEmpty(request.Path))
            {
                var file = _provider.GetFileInfo(request.Path);
                if (file.Exists)
                {
                    response.RouteFound = true;
                    response = new WebResponse();
                    response.StatusCode = 200;
                    string mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(file.Name));
                    response.Header.Add("content-type", mimeType);
                    await response.WriteAsync(file);
                }
            }

            return response;
        }
    }
}