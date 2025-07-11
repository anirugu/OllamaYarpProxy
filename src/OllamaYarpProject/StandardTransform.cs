using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
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
            .Select(v => v.Address) //#16250
            .ToHashSet();

        context.AddRequestTransform(async transformContext =>
        {
            var context = transformContext.HttpContext;


            if (context.Request.Path == "/api/tags")
            {
                //we need to change the request to the endpoint models
                transformContext.Path = "/models";
                _logger.LogInformation("Proxy: Request path rewritten from /api/tags to /api/models");
            }
            else if (context.Request.Path == "/v1/chat/completions")
            {
                //we need to change the request to the endpoint models
                transformContext.Path = "/chat/completions";
                _logger.LogInformation("Proxy: Request path rewritten from v1/chat/completions to v1/chat/completions");
            }
            else if (context.Request.Path == "/api/show")
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                string body = await reader.ReadToEndAsync();

                // deserialize in json 
                var json = JsonConvert.DeserializeObject(body) as JObject;
                var model = json.Value<string>("model");

                var response = transformContext.HttpContext.Response;
                response.StatusCode = 200;
                response.ContentType = "application/json";

                GemmaModel answer = new GemmaModel();
                //answer.License = "MIT";
                //answer.Modelfile = "model.gguf";
                answer.Capabilities = new List<string> { "chat" };
                answer.ModelInfo = new ModelInfo();
                answer.ModelInfo.Architecture = model;

                var jsonResponse = JsonConvert.SerializeObject(answer, Formatting.Indented);

                await response.WriteAsync(jsonResponse);
            }
            else if (context.Request.Path == "/api/version") 
            {
                var response = transformContext.HttpContext.Response;
                response.StatusCode = 200;
                response.ContentType = "application/json";
                await response.WriteAsync("{\"version\": \"0.9.6\"}");
            }
            _logger.LogDebug($"Proxy: Request {context.Request.GetDisplayUrl()} method {context.Request.Method} proxied to {transformContext.Path}");
        });

        context.CopyResponseHeaders = true;

        context.AddResponseTransform(async (transformContext) =>
        {
            var context = transformContext.HttpContext;
            var response = transformContext.ProxyResponse;
            if (response.RequestMessage.RequestUri.LocalPath == "/models")
            {
                //I need to grab the original content and then change the schema
                var content = await response.Content.ReadAsStringAsync();
                var source = JsonConvert.DeserializeObject<SourceRoot>(content);

                var ollamaModels = new OllamaRoot
                {
                    models = source.data
                        .Select(m => new OllamaModel
                        {
                            name = m.id,
                            model = m.id,
                            modified_at = "2024-02-24T18:29:19.5508829+01:00",
                            size = 1966917458,
                            digest = Guid.NewGuid().ToString(),
                        })
                        .ToList()
                };
                var ollamaJson = JsonConvert.SerializeObject(ollamaModels, Formatting.Indented);

                transformContext.SuppressResponseBody = true;

                // Convert modified JSON to bytes
                var modifiedBytes = System.Text.Encoding.UTF8.GetBytes(ollamaJson);

                // Update the Content-Length header to match the new content
                transformContext.HttpContext.Response.ContentLength = modifiedBytes.Length;

                // Set the correct content type
                transformContext.HttpContext.Response.ContentType = "application/json";

                // Write the modified content
                await transformContext.HttpContext.Response.Body.WriteAsync(modifiedBytes);
            }

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
