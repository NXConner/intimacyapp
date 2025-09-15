using System.Net.Http.Headers;
using System.Text;
using IntimacyAI.Server;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;

namespace IntimacyAI.Tests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder => { });
    }

    [Fact]
    public async Task Health_Returns_Ok()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Analytics_CRUD_Minimal()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", "dev-key");

        var payload = new StringContent("{\"featureUsed\":\"test\",\"platform\":\"tests\"}", Encoding.UTF8, "application/json");
        var post = await client.PostAsync("/api/analytics", payload);
        post.EnsureSuccessStatusCode();

        var list = await client.GetAsync("/api/analytics?take=1");
        list.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Analyze_EndToEnd_Placeholder()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", "dev-key");

        // 1x1 PNG
        var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAAC0lEQVR4nGP4DwABAQEAGN2NHQAAAABJRU5ErkJggg==");
        var form = new MultipartFormDataContent();
        var file = new ByteArrayContent(png);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, "file", "1x1.png");

        var resp = await client.PostAsync("/api/analyze", form);
        Assert.Equal(System.Net.HttpStatusCode.Accepted, resp.StatusCode);
    }

    [Fact]
    public async Task Analyze_FullFlow_Returns_Result()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-API-Key", "dev-key");

        // Submit image
        var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAAC0lEQVR4nGP4DwABAQEAGN2NHQAAAABJRU5ErkJggg==");
        using var form = new MultipartFormDataContent();
        using var file = new ByteArrayContent(png);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, "file", "1x1.png");
        var accept = await client.PostAsync("/api/analyze", form);
        Assert.Equal(System.Net.HttpStatusCode.Accepted, accept.StatusCode);
        var acceptJson = await accept.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var acceptObj = JsonSerializer.Deserialize<AcceptResponse>(acceptJson, options);
        Assert.NotNull(acceptObj);
        Assert.False(string.IsNullOrWhiteSpace(acceptObj!.SessionId));

        // Poll for completion
        AnalysisResult? result = null;
        for (int i = 0; i < 40; i++)
        {
            await Task.Delay(250);
            var r = await client.GetAsync($"/api/analysis/{acceptObj.SessionId}");
            if (r.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var json = await r.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<AnalysisResult>(json, options);
                if (result != null)
                {
                    break;
                }
            }
        }

        Assert.NotNull(result);
        Assert.Equal(acceptObj.SessionId, result!.SessionId);
        Assert.NotNull(result.Scores);
        Assert.NotNull(result.Metadata);
    }

    private sealed class AcceptResponse
    {
        public string SessionId { get; set; } = string.Empty;
    }

    private sealed class AnalysisResult
    {
        public string SessionId { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty;
        public Dictionary<string, object> Scores { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();
        public DateTime CreatedAtUtc { get; set; }
    }
}

