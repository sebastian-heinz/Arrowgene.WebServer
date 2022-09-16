using System.Threading.Tasks;

namespace Arrowgene.WebServer.Server
{
    /// <summary>
    ///     Defines web server
    /// </summary>
    public interface IWebServerCore
    {
        Task Start(IWebServerHandler handler);
        Task Stop();
        WebSetting Setting { get; }
    }
}