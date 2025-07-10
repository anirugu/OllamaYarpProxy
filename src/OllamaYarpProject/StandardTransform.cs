using Microsoft.AspNetCore.Http.Extensions;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;

namespace OllamaYarpProject;

public class StandardTransform : ITransformProvider
{
    private readonly ILogger<StandardTransform> _logger;

    public StandardTransform(ILogger<StandardTransform> logger)
    {
        _logger = logger;
    }

    public void Apply(TransformBuilderContext context)
    {
        context.UseDefaultForwarders = true;
        var destinationsUrl = context
            .Cluster
            .Destinations
            .Values
            .Select(v =>v.Address) //#16250
            .ToHashSet();

        context.AddRequestTransform(async transformContext =>
        {
            var context = transformContext.HttpContext;

            ////Proxy is normally propagating every header to the destination, but for NTLM (negotiate) you should
            ////remove the header or the calling service will immediately respond with a 401, without even call
            ////your auth module.
            ////remember that bypass authentication should not remove auty handler
            //bool isBypassUser = context.User?.Identity?.Name == BypassAuthenticationHandler.BypassUserName;
            //bool isBypassJarvisTokenUser = context.User?.Identity?.Name == JarvisTokenBypassAuthenticationHandler.UserName;
            //if (!isBypassUser && transformContext.ProxyRequest.Headers.Contains("Authorization"))
            //{
            //    transformContext.ProxyRequest.Headers.Remove("Authorization");
            //}

            //var automationPaths = _options.CurrentValue.AutomationPaths;

            //var tokenOk = await AddAuthTokenToRequest(transformContext, context, isBypassUser, isBypassJarvisTokenUser, automationPaths);
            //if (!tokenOk)
            //{
            //    //Something is not ok, we logout from the proxy.
            //    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //    _logger.Warning("token is not ok, user {0} logged out", context.User?.Identity?.Name ?? "anonymous");
            //}
            _logger.LogDebug("Proxy: Request {0} proxied to {1}", context.Request.GetDisplayUrl(), transformContext.Path);
        });

        context.CopyResponseHeaders = true;

        context.AddResponseTransform(async (transformContext) =>
        {
            var context = transformContext.HttpContext;
            var response = context.Response;

            //if (transformContext.ProxyResponse != null)
            //{
            //    LogUtils.Debug(
            //        "Proxy: Response: {2} for request:\n {0} remote server proxied url {1}",
            //        () => new object[] {
            //                context.Request.Dump(),
            //                transformContext.ProxyResponse.RequestMessage.RequestUri.AbsoluteUri,
            //                transformContext.ProxyResponse.StatusCode
            //        });

            //    //we do want to log every response that is not successful and it is not a 404 because it is too noise.
            //    var intStatusCode = (int)transformContext.ProxyResponse.StatusCode;
            //    if (intStatusCode >= 400 && !_options.CurrentValue.RemoteErrorNotToLog.Contains(intStatusCode))
            //    {
            //        //I have some non success status code.
            //        string responseContent = String.Empty;
            //        if (transformContext.ProxyResponse.Content != null)
            //        {
            //            responseContent = await transformContext.ProxyResponse.Content.ReadAsStringAsync();
            //        }
            //        _logger.Error(
            //           "Proxy: Error in remote response, code=[{0}] for request:\n {1} answered {2}",
            //                transformContext.ProxyResponse.StatusCode,
            //                transformContext.ProxyResponse.RequestMessage.RequestUri.AbsoluteUri,
            //                responseContent);
            //    }
            //}

            ////when you proxy automation/kisters/etc you have problem if the first url does not ends with / because
            ////of relative path, we want to automatically redirect, if we have no value in route value and we have a path
            ////that does not ends with /, we redirect to a route endings with /
            //if (response.StatusCode == 200 //other party responded ok
            //    && context.Request.RouteValues?.Values?.Count(v => v != null) == 0 //we have no route values, is a first level call like /kisters
            //    && _options.CurrentValue.RedirectionThatNeedsEndSlash.Contains(context.Request.Path))
            //{
            //    context.Response.StatusCode = (int)HttpStatusCode.Redirect;
            //    context.Response.Headers.Add("Location", context.Request.Path.Value.TrimEnd('/') + '/');
            //    return;
            //}

            //if (response.StatusCode == (int)HttpStatusCode.Redirect
            //   || response.StatusCode == (int)HttpStatusCode.MovedPermanently
            //   || response.StatusCode == (int)HttpStatusCode.PermanentRedirect)
            //{
            //    //ok we need to remove the location if we need to rewrite the real proxy.
            //    var location = response.Headers["Location"].First();
            //    if (location.Contains("Account/login"))
            //    {
            //        context.Response.Headers.Remove("Location");
            //        context.Response.Headers.Add("Location", Constants.LoginUrl);
            //        LogUtils.Information(
            //            "Proxy-Auth: Redirect fix, detect redirection to login page from jarvis, {location} proxy fixed to {newUri}",
            //            location,
            //            Constants.LoginUrl);
            //    }
            //    else if (destinationsUrl.Any(d => location.IndexOf(d, StringComparison.OrdinalIgnoreCase) == 0))
            //    {
            //        //ok we are redirecting on a address that is the destination address, I need to rewrite the header using 
            //        //the real request we need to check the real path, for now we rewrite the host domain, but this should be fixed.
            //        var builder = new UriBuilder(location);
            //        var port = context.Request.Host.Port;
            //        if (port == null)
            //        {
            //            if (context.Request.Scheme == "https")
            //            {
            //                port = 443;
            //            }
            //            else
            //            {
            //                port = 80;
            //            }
            //        }
            //        builder.Port = port.Value;
            //        builder.Host = context.Request.Host.Host;
            //        builder.Scheme = context.Request.Scheme;
            //        context.Response.Headers.Remove("Location");

            //        LogUtils.Information(
            //            "Proxy-Auth: Redirect fix, original {uri} redirected to {location} proxy fixed to {newUri}",
            //            context.Request.GetUri().AbsoluteUri,
            //            location,
            //            builder.Uri.AbsoluteUri);

            //        context.Response.Headers.Add("Location", builder.Uri.AbsoluteUri);
            //    }
            //    else
            //    {
            //        LogUtils.Information(
            //            "Proxy-Auth: Redirect not needed, page {request} will be redirected to {location}",
            //            context.Request.GetUri().AbsoluteUri,
            //            location);
            //    }
            //}

            _logger.LogDebug("Proxy: Request {0} proxied", context.Request.GetDisplayUrl());
        });
    }

    public void ValidateCluster(TransformClusterValidationContext context)
    {
        _logger.LogInformation("StandardTransform.ValidateCluster called");
    }

    public void ValidateRoute(TransformRouteValidationContext context)
    {
        _logger.LogInformation("StandardTransform.ValidateRoute called");
    }
}
