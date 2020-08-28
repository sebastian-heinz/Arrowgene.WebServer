using System.IO;
using System.Threading.Tasks;
using Arrowgene.WebServer.Middleware;
using Microsoft.Extensions.FileProviders;

namespace Arrowgene.WebServer.WebMiddleware
{
    public class StaticFileMiddleware : IWebMiddleware
    {
        private readonly IFileProvider _provider;
        private string _root;

        public StaticFileMiddleware(string root, IFileProvider provider)
        {
            _root = root;
            _provider = provider;
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