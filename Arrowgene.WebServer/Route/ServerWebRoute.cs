namespace Arrowgene.WebServer.Route
{
    public abstract class ServerWebRoute : WebRoute
    {
        public ServerWebRoute(WebServer server)
        {
            Server = server;
        }

        protected WebServer Server { get; }
    }
}