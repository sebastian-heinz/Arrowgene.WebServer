using System;
using System.Collections.Generic;
using Arrowgene.Logging;
using Arrowgene.WebServer;
using Arrowgene.WebServer.Route;
using Arrowgene.WebServer.Server.Kestrel;
using Arrowgene.WebServer.Simple;
using Arrowgene.WebServer.WebMiddleware;
using Microsoft.Extensions.FileProviders;

LogProvider.OnLogWrite += (sender, eventArgs) => Console.WriteLine(eventArgs.Log);
LogProvider.Start();
WebSetting s = new WebSetting();
WebService service = new WebService(new KestrelWebServer(s));

IWebRoute indexRoute = new IndexRoute();
List<WebRequestMethod> methods = indexRoute.GetMethods();
service.AddRoute(indexRoute);
service.AddRoute(new JsonRoute());

StaticFileMiddleware staticFiles = new StaticFileMiddleware(new PhysicalFileProvider("C:\\Users\\railgun\\dev\\Arrowgene.WebServer"));
List<string> files = staticFiles.GetServingFilesPath();
service.AddMiddleware(staticFiles);

List<string> servRoutes = service.GetServingRoutes(s.WebEndpoints[0]);
List<string> servStatic = staticFiles.GetServingFilesUrl(s.WebEndpoints[0]);

await service.Start();
LogProvider.Stop();
Console.WriteLine("Done");