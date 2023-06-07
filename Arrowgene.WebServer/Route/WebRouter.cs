using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        public List<string> GetServingRoutes(WebEndPoint webEndPoint)
        {
            List<string> routes = new List<string>();
            foreach (string routeKey in _routes.Keys)
            {
                IWebRoute route = _routes[routeKey];
                List<WebRequestMethod> methods = route.GetMethods();
                foreach (WebRequestMethod method in methods)
                {
                    routes.Add(
                        $"[{method}] {webEndPoint.GetUrl()}{route.Route}");
                }
            }

            return routes;
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

            IWebRoute route = RouteMatcher(request);
            if (route == null)
            {
                return await WebResponse.NotFound();
            }

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
                Logger.Info($"Request method: {request.Method} not supported for request: {request}");
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

        private IWebRoute RouteMatcher(WebRequest request)
        {
            if (_routes.ContainsKey(request.Path))
            {
                return _routes[request.Path];
            }

            foreach (string key in _routes.Keys)
            {
                Match m = Regex.Match(request.Path, key, RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    return _routes[key];
                }
            }

            return null;
        }
    }
}