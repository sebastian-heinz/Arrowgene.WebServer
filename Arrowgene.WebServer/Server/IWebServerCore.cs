using System.Threading.Tasks;

namespace Arrowgene.WebServer.Server
{
    /// <summary>
    ///     Defines web server
    /// </summary>
    public interface IWebServerCore
    {
        void SetHandler(IWebServerHandler handler);
        Task Start();
        Task Stop();
    }
}