using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Arrowgene.WebServer
{
    public class WebResponse
    {
        public WebResponse()
        {
            Body = new MemoryStream();
            Header = new WebCollection<string, string>();
            RouteFound = false;
        }

        public Stream Body { get; set; }
        public int StatusCode { get; set; }
        public bool RouteFound { get; set; }

        public WebCollection<string, string> Header { get; }

        public static async Task<WebResponse> NotFound()
        {
            var notFound = new WebResponse();
            notFound.StatusCode = 404;
            await notFound.WriteAsync("404 - route not found");
            return notFound;
        }

        public static async Task<WebResponse> InternalServerError()
        {
            var internalServerError = new WebResponse();
            internalServerError.StatusCode = 500;
            await internalServerError.WriteAsync("500 - an internal error occured");
            return internalServerError;
        }

        public static async Task<WebResponse> Redirect(string location)
        {
            var redirect = new WebResponse();
            redirect.StatusCode = 301;
            redirect.Header.Add("location", location);
            await redirect.WriteAsync("301 - Moved Permanently");
            return redirect;
        }

        public Task WriteAsync(IFileInfo fileInfo, bool contentLength = true)
        {
            if (contentLength) Header.Add("content-length", $"{fileInfo.Length}");

            return fileInfo.CreateReadStream().CopyToAsync(Body);
        }

        public Task WriteAsync(string text, bool contentLength = true)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            return WriteAsync(text, Encoding.UTF8, contentLength);
        }

        public async Task WriteJsonAsync<T>(T obj, bool contentLength = true)
        {
            long pos = Body.Position;
            await JsonSerializer.SerializeAsync(Body, obj);
            if (contentLength)
            {
                Header.Add("content-length", $"{Body.Length - pos}");
            }
        }

        public Task WriteAsync(string text, Encoding encoding, bool contentLength = true)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            var bytes = encoding.GetBytes(text);
            if (contentLength) Header.Add("content-length", $"{bytes.Length}");

            return Body.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}