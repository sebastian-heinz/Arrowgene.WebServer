using System;
using Arrowgene.Logging;
using Arrowgene.WebServer;
using Arrowgene.WebServer.Server.Kestrel;

LogProvider.OnLogWrite += (sender, eventArgs) => Console.WriteLine(eventArgs.Log);
LogProvider.Start();

WebSetting s = new WebSetting();
WebService service = new WebService(new KestrelWebServer(s));
await service.Start();

LogProvider.Stop();
Console.WriteLine("Done");