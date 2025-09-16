using IntimacyAI.Server.Security;
using IntimacyAI.Server.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace IntimacyAI.Tests;

public class UnitTest1
{
    [Fact]
    public void Encryption_RoundTrip()
    {
        var inMemory = new Dictionary<string, string?>
        {
            ["Security:EncryptionKey"] = Convert.ToBase64String(new byte[32])
        };
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(inMemory!).Build();
        var enc = new EncryptionService(cfg);
        var text = "hello world";
        var c = enc.Encrypt(text);
        var p = enc.Decrypt(c);
        Assert.Equal(text, p);
    }

    [Fact]
    public void AnalysisQueue_Works()
    {
        IAnalysisQueue q = new AnalysisQueue();
        var req = new AnalysisRequest { AnalysisType = "image", Data = new byte[] { 1, 2, 3 } };
        q.Enqueue(req);
        Assert.True(q.TryDequeue(out var outReq));
        Assert.NotNull(outReq);
        Assert.Equal("image", outReq!.AnalysisType);
    }
}

public class ApiKeyMiddlewareTests
{
    private static DefaultHttpContext CreateContext(string? path = "/", string? headerValue = null, string? authHeader = null, string? accessToken = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = path;
        if (headerValue is not null) ctx.Request.Headers["X-API-Key"] = headerValue;
        if (authHeader is not null) ctx.Request.Headers["Authorization"] = authHeader;
        if (accessToken is not null) ctx.Request.QueryString = new QueryString($"?access_token={accessToken}");
        return ctx;
    }

    private static ApiKeyMiddleware CreateMiddleware(string expectedKey, bool isDevelopment)
    {
        var next = new RequestDelegate(_ => Task.CompletedTask);
        var cfg = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Security:ApiKey"] = expectedKey }).Build();
        var env = new FakeEnv(isDevelopment);
        return new ApiKeyMiddleware(next, cfg, env);
    }

    private sealed class FakeEnv : IHostEnvironment
    {
        public FakeEnv(bool isDev) { EnvironmentName = isDev ? Environments.Development : Environments.Production; }
        public string ApplicationName { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; }
    }

    [Fact]
    public async Task Allows_Swagger_And_Health_Without_Key()
    {
        var mw = CreateMiddleware("secret", false);
        foreach (var p in new[] { "/swagger/index.html", "/health", "/healthz" })
        {
            var ctx = CreateContext(p);
            await mw.InvokeAsync(ctx);
            Assert.NotEqual(StatusCodes.Status401Unauthorized, ctx.Response.StatusCode);
        }
    }

    [Fact]
    public async Task Accepts_XApiKey_Header()
    {
        var mw = CreateMiddleware("secret", false);
        var ctx = CreateContext("/api/analytics", headerValue: "secret");
        await mw.InvokeAsync(ctx);
        Assert.NotEqual(StatusCodes.Status401Unauthorized, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task Accepts_Bearer_Token()
    {
        var mw = CreateMiddleware("secret", false);
        var ctx = CreateContext("/api/analytics", authHeader: "Bearer secret");
        await mw.InvokeAsync(ctx);
        Assert.NotEqual(StatusCodes.Status401Unauthorized, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task Accepts_Access_Token_Query()
    {
        var mw = CreateMiddleware("secret", false);
        var ctx = CreateContext("/hubs/analysis", accessToken: "secret");
        await mw.InvokeAsync(ctx);
        Assert.NotEqual(StatusCodes.Status401Unauthorized, ctx.Response.StatusCode);
    }
}