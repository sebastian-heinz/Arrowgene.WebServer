using System.Threading.Tasks;

namespace Arrowgene.WebServer.Route
{
    /// <summary>
    ///     Implementation of Kestrel server as backend
    /// </summary>
    public abstract class WebRoute : IWebRoute
    {
        public abstract string Route { get; }

        public virtual Task<WebResponse> Get(WebRequest request)
        {
            return WebResponse.NotFound();
        }

        public virtual Task<WebResponse> Post(WebRequest request)
        {
            return WebResponse.NotFound();
        }

        public virtual Task<WebResponse> Put(WebRequest request)
        {
            return WebResponse.NotFound();
        }

        public virtual Task<WebResponse> Delete(WebRequest request)
        {
            return WebResponse.NotFound();
        }

        public virtual Task<WebResponse> Head(WebRequest request)
        {
            return WebResponse.NotFound();
        }
    }
}