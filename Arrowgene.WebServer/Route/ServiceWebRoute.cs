namespace Arrowgene.WebServer.Route
{
    public abstract class ServiceWebRoute<T> : WebRoute
    {
        public ServiceWebRoute(T service)
        {
            Service = service;
        }

        protected T Service { get; }
    }
}