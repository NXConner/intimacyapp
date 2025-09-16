using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace IntimacyAI.Server.Security
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _expectedKey;
        private readonly IHostEnvironment _env;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, IHostEnvironment env)
        {
            _next = next;
            _expectedKey = configuration["Security:ApiKey"] ?? string.Empty;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            if (path.StartsWith("/swagger") || path.Equals("/health") || path.Equals("/healthz"))
            {
                await _next(context);
                return;
            }

            if (string.IsNullOrEmpty(_expectedKey))
            {
                if (_env.IsDevelopment())
                {
                    await _next(context);
                    return;
                }
                context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                await context.Response.WriteAsync("API key not configured");
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

