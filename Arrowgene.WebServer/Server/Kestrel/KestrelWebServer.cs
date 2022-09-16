using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Arrowgene.Logging;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using ILogger = Arrowgene.Logging.ILogger;

namespace Arrowgene.WebServer.Server.Kestrel
{
    /// <summary>
    ///     Implementation of Kestrel server as backend
    /// </summary>
    public class KestrelWebServer : IWebServerCore
    {
        private static readonly ILogger Logger = LogProvider.Logger(typeof(KestrelLogger));


        private IHostApplicationLifetime _applicationLifetime;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private IServer _server;
        private readonly WebSetting _setting;
        private readonly int _shutdownTimeout = 10000;

        public KestrelWebServer(WebSetting setting)
        {
            _setting = setting;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        WebSetting IWebServerCore.Setting => _setting;

        public async Task Start(IWebServerHandler handler)
        {
            try
            {
                if (handler == null)
                {
                    throw new Exception("IWebServerHandler is null");
                }

                ServiceCollection services = new ServiceCollection();
                services.AddLogging();
                services.Configure<KestrelServerOptions>(options =>
                {
                    foreach (WebEndPoint webEndPoint in _setting.WebEndpoints)
                    {
                        if (webEndPoint.IsHttps)
                        {
                            options.Listen(
                                webEndPoint.IpAddress,
                                webEndPoint.Port,
                                listenOptions =>
                                {
                                    var cert = new X509Certificate2(
                                        webEndPoint.HttpsCertPath,
                                        webEndPoint.HttpsCertPw
                                    );
                                    listenOptions.UseHttps(new HttpsConnectionAdapterOptions
                                    {
                                        ServerCertificate = cert,
                                        SslProtocols = webEndPoint.SslProtocols
                                    });
                                }
                            );
                        }
                        else
                        {
                            options.Listen(webEndPoint.IpAddress, webEndPoint.Port);
                        }
                    }

                    options.AddServerHeader = false;
                });

                ILoggerFactory loggerFactory = new LoggerFactory();
                loggerFactory.AddProvider(new KestrelLoggerProvider());

                services.AddSingleton(loggerFactory);
                services.AddSingleton<IServer, KestrelServer>();
                services.AddSingleton<IConnectionListenerFactory, SocketTransportFactory>();
                services.AddSingleton<IWebServerHandler>(handler);
                services.AddSingleton<IHostApplicationLifetime, ApplicationLifetime>();
                services.AddTransient<IHttpContextFactory, DefaultHttpContextFactory>();
                services.AddTransient<KestrelApplication>();


                ServiceProvider serviceProvider = services.BuildServiceProvider();
                _server = serviceProvider.GetRequiredService<IServer>();
                _applicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
                KestrelApplication application = serviceProvider.GetRequiredService<KestrelApplication>();

                await _server.StartAsync(application, _cancellationTokenSource.Token);

                _cancellationTokenSource.Token.Register(
                    state => ((IHostApplicationLifetime)state).StopApplication(),
                    _applicationLifetime
                );
                var completionSource = new TaskCompletionSource<object>(
                    TaskCreationOptions.RunContinuationsAsynchronously
                );
                _applicationLifetime.ApplicationStopping.Register(
                    obj => ((TaskCompletionSource<object>)obj).TrySetResult(null),
                    completionSource
                );
                await completionSource.Task;
                await _server.StopAsync(new CancellationToken());
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                throw;
            }
        }

        public async Task Stop()
        {
            var token = new CancellationTokenSource(_shutdownTimeout).Token;
            _applicationLifetime?.StopApplication();
            if (_server != null)
            {
                await _server.StopAsync(token).ConfigureAwait(false);
            }
        }
    }
}