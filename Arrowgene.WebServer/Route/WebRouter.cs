using System.Collections.Generic;
using System.Threading.Tasks;
using Arrowgene.Logging;

namespace Arrowgene.WebServer.Route
{
    /// <summary>
    ///     Parses routes and calls the route
    /// </summary>
    public class WebRouter : IWebRouter
    {
        private static readonly ILogger Logger = LogProvider.Logger(typeof(WebRouter));

        private readonly Dictionary<string, IWebRoute> _routes;
        private readonly WebSetting _setting;

        public WebRouter(WebSetting setting)
        {
            _setting = setting;
            _routes = new Dictionary<string, IWebRoute>();
        }
        
        /// <summary>
        ///     Adds a handler for a specific route.
        /// </summary>
        public void AddRoute(IWebRoute route)
        {
            _routes.Add(route.Route, route);
        }

        /// <summary>
        ///     Passes incoming requests to the correct route
        /// </summary>
        public async Task<WebResponse> Route(WebRequest request)
        {
            Logger.Info($"Request: {request}");
            if (request.Path == null)
            {
                Logger.Error("Request path not set, please check sever request mapping implementation");
                return await WebResponse.InternalServerError();
            }

            if (_routes.ContainsKey(request.Path))
            {
                var route = _routes[request.Path];
                Task<WebResponse> responseTask = null;
                switch (request.Method)
                {
                    case WebRequestMethod.Get:
                        responseTask = route.Get(request);
                        break;
                    case WebRequestMethod.Post:
                        responseTask = route.Post(request);
                        break;
                    case WebRequestMethod.Put:
                        responseTask = route.Put(request);
                        break;
                    case WebRequestMethod.Delete:
                        responseTask = route.Delete(request);
                        break;
                    case WebRequestMethod.Head:
                        responseTask = route.Head(request);
                        break;
                }

                if (responseTask == null)
                {
                    Logger.Info($"Request method: {request.Method} not supported for requested path: {request.Path}");
                    return await WebResponse.InternalServerError();
                }

                var response = await responseTask;
                response.RouteFound = true;
                if (!string.IsNullOrEmpty(_setting.ServerHeader))
                {
                    response.Header.Add("Server", _setting.ServerHeader);
                }
                
                return response;
            }

            return await WebResponse.NotFound();
        }
    }
}