using System.Threading.Tasks;

namespace Arrowgene.WebServer.Route
{
    public interface IWebRouter
    {
        void AddRoute(IWebRoute route);
        Task<WebResponse> Route(WebRequest request);
    }
}