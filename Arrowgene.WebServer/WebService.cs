using System.Threading.Tasks;
using Arrowgene.Logging;
using Arrowgene.WebServer.Middleware;
using Arrowgene.WebServer.Route;
using Arrowgene.WebServer.Server;

namespace Arrowgene.WebServer
{
    /// <summary>
    ///     Manages web requests
    /// </summary>
    public class WebService : IWebServerHandler
    {
        private static readonly ILogger Logger = LogProvider.Logger(typeof(WebService));

        private readonly WebMiddlewareStack _middlewareStack;
        private readonly WebRouter _router;
        private readonly IWebServerCore _serverCore;

        public WebService(IWebServerCore serverCore)
        {
            _serverCore = serverCore;
            _router = new WebRouter(serverCore.Setting);
            _middlewareStack = new WebMiddlewareStack(_router.Route);
        }

        public async Task<WebResponse> Handle(WebRequest request)
        {
            WebResponse response = await _middlewareStack.Start(request);
            if (!response.RouteFound)
            {
                Logger.Info($"No route or middleware registered for requested Path: {request.Path}");
            }

            return response;
        }

        public async Task Start()
        {
            await _serverCore.Start(this);
        }

        public async Task Stop()
        {
            await _serverCore.Stop();
        }

        public void AddRoute(IWebRoute route)
        {
            _router.AddRoute(route);
        }

        public void AddMiddleware(IWebMiddleware middleware)
        {
            _middlewareStack.Use(next => req => middleware.Handle(req, next));
            // middleware.Use(
            //     next => req =>
            //     {
            //         return next(req);
            //     }
            // );
        }
    }
}