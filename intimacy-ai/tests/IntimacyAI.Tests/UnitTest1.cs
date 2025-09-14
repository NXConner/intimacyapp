using IntimacyAI.Server.Security;
using IntimacyAI.Server.Services;
using Microsoft.Extensions.Configuration;
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