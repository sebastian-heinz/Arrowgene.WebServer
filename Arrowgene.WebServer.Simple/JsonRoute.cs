using System.Threading.Tasks;
using Arrowgene.WebServer.Route;

namespace Arrowgene.WebServer.Simple;

public class JsonRoute : WebRoute
{
    public override string Route => "/json";

    public class JsonClass
    {
        public string Name { get; set; }
    }


    public override async Task<WebResponse> Get(WebRequest request)
    {
        WebResponse response = new WebResponse();
        response.StatusCode = 200;

        JsonClass jsonClass = new JsonClass();
        jsonClass.Name = "Test";

        await response.WriteJsonAsync(jsonClass);
        return response;
    }
}