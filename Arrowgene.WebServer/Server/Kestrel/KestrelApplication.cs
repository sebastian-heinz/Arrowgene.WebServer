using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Arrowgene.WebServer.Server.Kestrel;

public class KestrelApplication : IHttpApplication<HttpContext>
{
    private readonly IHttpContextFactory _httpContextFactory;
    private readonly IWebServerHandler _handler;

    public KestrelApplication(IWebServerHandler handler, IHttpContextFactory httpContextFactory)
    {
        _handler = handler;
        _httpContextFactory = httpContextFactory;
    }

    public HttpContext CreateContext(IFeatureCollection contextFeatures)
    {
        return _httpContextFactory.Create(contextFeatures);
    }

    public void DisposeContext(HttpContext context, Exception exception)
    {
        _httpContextFactory.Dispose(context);
    }

    public async Task ProcessRequestAsync(HttpContext context)
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
        foreach (var key in context.Request.Headers.Keys)
        {
            request.Header.Add(key, context.Request.Headers[key]);
        }

        foreach (var key in context.Request.Query.Keys)
        {
            request.QueryParameter.Add(key, context.Request.Query[key]);
        }

        foreach (var key in context.Request.Cookies.Keys)
        {
            request.Cookies.Add(key, context.Request.Cookies[key]);
        }

        await context.Request.Body.CopyToAsync(request.Body);
        request.Body.Position = 0;
        var response = await _handler.Handle(request);
        context.Response.StatusCode = response.StatusCode;
        foreach (var key in response.Header.Keys)
        {
            context.Response.Headers.Add(key, response.Header[key]);
        }

        response.Body.Position = 0;
        await response.Body.CopyToAsync(context.Response.Body);
    }
}