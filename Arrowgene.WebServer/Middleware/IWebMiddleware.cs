using System.Threading.Tasks;

namespace Arrowgene.WebServer.Middleware
{
    /// <summary>
    ///     Defines a middleware
    /// </summary>
    public interface IWebMiddleware
    {
        Task<WebResponse> Handle(WebRequest request, WebMiddlewareDelegate next);
    }
}