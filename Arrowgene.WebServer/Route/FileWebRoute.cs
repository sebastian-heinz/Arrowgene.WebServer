using Microsoft.Extensions.FileProviders;

namespace Arrowgene.WebServer.Route
{
    public abstract class FileWebRoute : WebRoute
    {
        public FileWebRoute(IFileProvider fileProvider)
        {
            FileProvider = fileProvider;
        }

        protected IFileProvider FileProvider { get; }
    }
}