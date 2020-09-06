using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace demo.DemoApi.Service.Middlewares
{
    /// <summary>
    /// Middleware для проверки, что тело предыдущего ответа не изменилось
    /// </summary>
    public class ETagMiddleware
    {
        private readonly RequestDelegate _next;

        /// <inheritdoc />
        public ETagMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Вызов Middleware
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            var response = context.Response;
            var originalStream = response.Body;

            using (var ms = new MemoryStream())
            {
                response.Body = ms;

                await _next(context);

                if (IsEtagSupported(context, response))
                {
                    string checksum = CalculateChecksum(ms);

                    response.Headers[HeaderNames.ETag] = checksum;

                    if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var etag) && checksum == etag)
                    {
                        response.StatusCode = StatusCodes.Status304NotModified;
                        return;
                    }
                }

                ms.Position = 0;
                await ms.CopyToAsync(originalStream);
            }
        }

        private static bool IsEtagSupported(HttpContext context, HttpResponse response)
        {
            if (!context.Items.ContainsKey("CheckETagEnabled"))
            {
                return false;
            }

            if (response.StatusCode != StatusCodes.Status200OK)
            {
                return false;
            }

            if (response.Headers.ContainsKey(HeaderNames.ETag))
            {
                return false;
            }

            return true;
        }

        private static string CalculateChecksum(MemoryStream ms)
        {
            string checksum;

            using (var algo = SHA1.Create())
            {
                ms.Position = 0;
                byte[] bytes = algo.ComputeHash(ms);
                checksum = $"\"{WebEncoders.Base64UrlEncode(bytes)}\"";
            }

            return checksum;
        }
    }
}
