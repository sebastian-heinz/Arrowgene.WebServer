using System.Threading.Tasks;

namespace Arrowgene.WebServer.Server
{
    public interface IWebServerHandler
    {
        Task<WebResponse> Handle(WebRequest request);
    }
}