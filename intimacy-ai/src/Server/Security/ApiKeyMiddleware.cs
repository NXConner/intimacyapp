using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace IntimacyAI.Server.Security
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _expectedKey;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _expectedKey = configuration["Security:ApiKey"] ?? string.Empty;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/swagger") || path.Equals("/health"))
            {
                await _next(context);
                return;
            }

            if (string.IsNullOrEmpty(_expectedKey))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue("X-API-Key", out var provided) || provided != _expectedKey)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await _next(context);
        }
    }
}

