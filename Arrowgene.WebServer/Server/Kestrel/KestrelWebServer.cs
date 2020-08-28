﻿using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Arrowgene.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Abstractions.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ILogger = Arrowgene.Logging.ILogger;

namespace Arrowgene.WebServer.Server.Kestrel
{
    /// <summary>
    ///     Implementation of Kestrel server as backend
    /// </summary>
    public class KestrelWebServer : IWebServerCore
    {
        private static readonly ILogger Logger = LogProvider.Logger(typeof(KestrelWebServer));
        
        private ApplicationLifetime _applicationLifetime;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private IWebServerHandler _handler;
        private IServer _server;
        private readonly WebSetting _setting;
        private readonly int _shutdownTimeout = 10000;

        public KestrelWebServer(WebSetting setting)
        {
            _setting = setting;
            _cancellationTokenSource = new CancellationTokenSource();
        }
        
        WebSetting IWebServerCore.Setting => _setting;

        public void SetHandler(IWebServerHandler handler)
        {
            _handler = handler;
        }

        public async Task Start()
        {
            IHttpApplication<HostingApplication.Context> app;
            try
            {
                if (_handler == null) throw new Exception("Missing Handler - Call SetHandler()");

                ILoggerFactory loggerFactory = new LoggerFactory();
                loggerFactory.AddProvider(new KestrelLoggerProvider());
                var services = new ServiceCollection();
                services.AddSingleton(loggerFactory);
                services.AddLogging();

                var serviceProvider = GetProviderFromFactory(services);
                var kestrelServerOptions = Options.Create(new KestrelServerOptions());
                kestrelServerOptions.Value.ApplicationServices = serviceProvider;

                foreach (uint httpPort in _setting.HttpPorts) kestrelServerOptions.Value.ListenAnyIP((int) httpPort);

                if (_setting.HttpsEnabled)
                    kestrelServerOptions.Value.ListenAnyIP(_setting.HttpsPort,
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

                kestrelServerOptions.Value.AddServerHeader = false;

                var socketTransportOptions = Options.Create(new SocketTransportOptions());
                _applicationLifetime = new ApplicationLifetime(
                    loggerFactory.CreateLogger<ApplicationLifetime>()
                );
                ITransportFactory transportFactory = new SocketTransportFactory(
                    socketTransportOptions, _applicationLifetime, loggerFactory
                );


                _server = new KestrelServer(kestrelServerOptions, transportFactory, loggerFactory);
                var diagnosticListener = new DiagnosticListener("a");
                var formOptions = Options.Create(new FormOptions());
                IHttpContextFactory httpContextFactory = new HttpContextFactory(formOptions);
                app = new HostingApplication(
                    RequestDelegate,
                    loggerFactory.CreateLogger<KestrelWebServer>(),
                    diagnosticListener,
                    httpContextFactory
                );
            }
            catch (Exception ex)
            {
                Logger.Exception(ex);
                return;
            }

            var kestrelStartup = _server.StartAsync(app, _cancellationTokenSource.Token);
            await kestrelStartup;
            _cancellationTokenSource.Token.Register(
                state => ((IApplicationLifetime) state).StopApplication(),
                _applicationLifetime
            );
            var completionSource = new TaskCompletionSource<object>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            _applicationLifetime.ApplicationStopping.Register(
                obj => ((TaskCompletionSource<object>) obj).TrySetResult(null),
                completionSource
            );
            var kestrelCompleted = completionSource.Task;
            var kestrelCompletedResult = await kestrelCompleted;
            var kestrelShutdown = _server.StopAsync(new CancellationToken());
            await kestrelShutdown;
        }

        public async Task Stop()
        {
            var token = new CancellationTokenSource(_shutdownTimeout).Token;
            _applicationLifetime?.StopApplication();
            if (_server != null) await _server.StopAsync(token).ConfigureAwait(false);

            _applicationLifetime?.NotifyStopped();
            HostingEventSource.Log.HostStop();
        }
        
        /// <summary>
        ///     Called whenever a web request arrives.
        ///     - Maps Kestrel HttpRequest/HttpResponse to WebRequest/WebResponse
        ///     - Calls router to handle the request
        /// </summary>
        private async Task RequestDelegate(HttpContext context)
        {
            var request = new WebRequest();
            request.Host = context.Request.Host.Host;
            request.Port = context.Request.Host.Port;
            request.Method = WebRequest.ParseMethod(context.Request.Method);
            request.Path = context.Request.Path;
            request.Scheme = context.Request.Scheme;
            request.ContentType = context.Request.ContentType;
            request.QueryString = context.Request.QueryString.Value;
            request.ContentLength = context.Request.ContentLength;
            foreach (var key in context.Request.Headers.Keys) request.Header.Add(key, context.Request.Headers[key]);

            foreach (var key in context.Request.Query.Keys) request.QueryParameter.Add(key, context.Request.Query[key]);

            foreach (var key in context.Request.Cookies.Keys) request.Cookies.Add(key, context.Request.Cookies[key]);

            await context.Request.Body.CopyToAsync(request.Body);
            request.Body.Position = 0;
            var response = await _handler.Handle(request);
            context.Response.StatusCode = response.StatusCode;
            foreach (var key in response.Header.Keys) context.Response.Headers.Add(key, response.Header[key]);

            response.Body.Position = 0;
            await response.Body.CopyToAsync(context.Response.Body);
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