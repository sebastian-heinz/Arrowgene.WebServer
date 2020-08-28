using System.Threading.Tasks;

namespace Arrowgene.WebServer.Middleware
{
    public delegate Task<WebResponse> WebMiddlewareDelegate(WebRequest request);
}