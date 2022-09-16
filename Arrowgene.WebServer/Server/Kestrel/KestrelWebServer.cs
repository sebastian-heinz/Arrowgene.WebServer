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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ILogger = Arrowgene.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Arrowgene.WebServer.Server.Kestrel
{
    /// <summary>
    ///     Implementation of Kestrel server as backend
    /// </summary>
    public class KestrelWebServer : IWebServerCore
    {
        private static readonly ILogger Logger = LogProvider.Logger(typeof(KestrelWebServer));

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
            if (handler == null)
            {
                throw new Exception("Missing Handler - Call SetHandler()");
            }

            ServiceCollection services = new ServiceCollection();
            services.AddLogging(b =>
            {
                b.AddConsole();
                b.AddFilter("Microsoft.AspNetCore.Server.Kestrel", LogLevel.Warning);
            });

            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new KestrelLoggerProvider());
            services.AddSingleton(loggerFactory);

            services.Configure<KestrelServerOptions>(options =>
            {
                foreach (uint httpPort in _setting.HttpPorts)
                {
                    options.ListenAnyIP((int)httpPort);
                }

                if (_setting.HttpsEnabled)
                    options.ListenAnyIP(_setting.HttpsPort,
                        listenOptions =>
                        {
                            var cert = new X509Certificate2(_setting.HttpsCertPath,
                                _setting.HttpsCertPw);
                            listenOptions.UseHttps(new HttpsConnectionAdapterOptions
                            {
                                ServerCertificate = cert
                                //  SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls
                            });
                        });
                // kestrelServerOptions.Value.ListenAnyIP(_setting.WebSetting.HttpsPort);

                options.AddServerHeader = false;
            });


            services.AddSingleton<IServer, KestrelServer>();
            services.AddSingleton<IConnectionListenerFactory, SocketTransportFactory>();
            services.AddSingleton<IWebServerHandler>(handler);
            // services.AddSingleton<ITransportFactory, LibuvTransportFactory>();
            // services.AddSingleton<Microsoft.Extensions.Hosting.IHostApplicationLifetime, GenericWebHostApplicationLifetime>();
            services.AddTransient<IHttpContextFactory, DefaultHttpContextFactory>();
            services.AddTransient<KestrelApplication>();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            _server = serviceProvider.GetRequiredService<IServer>();
            KestrelApplication application = serviceProvider.GetRequiredService<KestrelApplication>();

           // await server.StartAsync(application, default).ConfigureAwait(false);


            IOptions<SocketTransportOptions> socketTransportOptions = Options.Create(new SocketTransportOptions());
            //  _applicationLifetime = new ApplicationLifetime(
            //      loggerFactory.CreateLogger<ApplicationLifetime>()
            //  );
            //IConnectionListenerFactory transportFactory = new SocketTransportFactory(
           //     socketTransportOptions, loggerFactory
           //);


            var kestrelStartup = _server.StartAsync(application, _cancellationTokenSource.Token);
            await kestrelStartup;
            _cancellationTokenSource.Token.Register(
                state => ((IHostApplicationLifetime)state).StopApplication(),
                _applicationLifetime
            );
            var completionSource = new TaskCompletionSource<object>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
          //_applicationLifetime.ApplicationStopping.Register(
          //    obj => ((TaskCompletionSource<object>)obj).TrySetResult(null),
          //    completionSource
          //);
            var kestrelCompleted = completionSource.Task;
            var kestrelCompletedResult = await kestrelCompleted;
            var kestrelShutdown = _server.StopAsync(new CancellationToken());
            await kestrelShutdown;
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

        /// <summary>
        ///     Called whenever a web request arrives.
        ///     - Maps Kestrel HttpRequest/HttpResponse to WebRequest/WebResponse
        ///     - Calls router to handle the request
        /// </summary>
        private async Task RequestDelegate(HttpContext context)
        {
         
        }

        private IServiceProvider GetProviderFromFactory(IServiceCollection collection)
        {
            var provider = collection.BuildServiceProvider();
            var service =
                provider.GetService<IServiceProviderFactory<IServiceCollection>>();
            if (service == null || service is DefaultServiceProviderFactory) return provider;

            using (provider)
            {
                return service.CreateServiceProvider(service.CreateBuilder(collection));
            }
        }
    }
}