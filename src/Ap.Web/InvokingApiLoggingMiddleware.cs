using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Ap.Web;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder {
    public static class RequestResponseInterceptingAppBuilderExtensions {
        public static IApplicationBuilder UseReqeustResponseIntercepting (this IApplicationBuilder app) {
            return app.UseMiddleware<InvokingApiLoggingQueryMiddleware> ()
                .UseMiddleware<InvokingApiLoggingInterceptorMiddleware> ();
        }
    }

    public class InvokingApiLoggingQueryMiddleware {
        private readonly RequestDelegate next;
        private readonly IInvokingApiLoggingStore store;
        public InvokingApiLoggingQueryMiddleware (RequestDelegate next, IInvokingApiLoggingStore store) {
            this.store = store;
            this.next = next;
        }

        public async Task Invoke (HttpContext context) {
            if (context.Request.Method != HttpMethods.Get || !context.Request.Path.StartsWithSegments ("/invoking-loggings")) {
                await this.next (context);
                return;
            }

            if (context.Request.Query.TryGetValue ("traceId", out Microsoft.Extensions.Primitives.StringValues value)) {
                var traceId = value[0];
                var entry = await store.GetAsync (traceId);
                if (entry is null) {
                    await WriteNotFoundResponseAsync (context.Response);
                } else {
                    await WriteEntryResponseAsync (context.Response, entry);
                }

                return;
            }

            await WriteNotFoundResponseAsync (context.Response);
        }

        private async Task WriteNotFoundResponseAsync (HttpResponse response) {
            response.ContentType = "text/html";
            response.StatusCode = 200;
            await response.WriteAsync ("<h3>Not found</h3>");
        }

        private async Task WriteEntryResponseAsync (HttpResponse response, InvokingApiLoggingEntry entry) {
            response.ContentType = "text/html";
            response.StatusCode = 200;

            var bodyBuilder = new StringBuilder ()
                .AppendLine ($"<h3>Found: {entry.TraceId}</h3>")
                .AppendLine ($"<p>User: {entry.LoggedUser}</p>")
                .AppendLine ($"<p>Url: {entry.Url}</p>")
                .AppendLine ($"<p>Request: {entry.RequestBody}</p>")
                .AppendLine ($"<p>Status Code: {entry.StatusCode}</p>")
                .AppendLine ($"<p>Response: {entry.ResponseBody}</p>")
                .AppendLine ($"<p>Duartion(ms): {entry.DurationInMs}</p>")
                .ToString ();
            await response.WriteAsync (bodyBuilder);
        }
    }

    public class InvokingApiLoggingInterceptorMiddleware {
        private readonly RequestDelegate next;
        private readonly IInvokingApiLoggingStore store;
        public InvokingApiLoggingInterceptorMiddleware (RequestDelegate next, IInvokingApiLoggingStore store) {
            this.next = next;
            this.store = store;
        }

        public async Task Invoke (HttpContext context) {
            using (var newRequestBody = new MemoryStream ())
            using (var newResponseBody = new MemoryStream ())
            using (var newResponseBodyReader = new StreamReader (newResponseBody, Encoding.UTF8)) {

                var traceId = context.TraceIdentifier;
                var url = ExtractUrl (context.Request);
                var requestBodyInString = await ReadAndReplaceRequestBodyAsync (context.Request, newRequestBody);

                context.Response.Headers.Add ("TraceId", traceId);
                var originalResponseBody = context.Response.Body;
                context.Response.Body = newResponseBody;

                var stopWatacher = new Stopwatch ();
                stopWatacher.Start ();
                await this.next (context);
                stopWatacher.Stop ();

                newResponseBody.Seek (0, SeekOrigin.Begin);
                var responseBodyInString = await newResponseBodyReader.ReadToEndAsync ();

                newResponseBody.Seek (0, SeekOrigin.Begin);
                await newResponseBody.CopyToAsync (originalResponseBody);

                var statusCode = context.Response.StatusCode;

                await this.store.PutAsync (new InvokingApiLoggingEntry (traceId, string.Empty, url, requestBodyInString, statusCode, responseBodyInString, stopWatacher.ElapsedMilliseconds));
            }
        }

        private string ExtractUrl (HttpRequest request) {
            return $"{request.Scheme}://{request.Host}{request.Path}?{request.QueryString}";
        }

        private async ValueTask<string> ReadAndReplaceRequestBodyAsync (HttpRequest request, MemoryStream newBody) {
            await request.Body.CopyToAsync (newBody);
            newBody.Seek (0, System.IO.SeekOrigin.Begin);

            var buffer = new byte[newBody.Length];
            newBody.Read (buffer, 0, buffer.Length);
            newBody.Seek (0, SeekOrigin.Begin);
            var bodyInString = Encoding.UTF8.GetString (buffer);

            request.Body = newBody;

            return bodyInString;
        }
    }

}