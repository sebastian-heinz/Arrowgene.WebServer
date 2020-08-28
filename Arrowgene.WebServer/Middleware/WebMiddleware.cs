using System.Threading.Tasks;

namespace Arrowgene.WebServer.Middleware
{
    public abstract class WebMiddleware : IWebMiddleware
    {
        public abstract Task<WebResponse> Handle(WebRequest request, WebMiddlewareDelegate next);
    }
}