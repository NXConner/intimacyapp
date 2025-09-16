using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace IntimacyAI.Server.Services
{
	public sealed class HttpInferenceOptions
	{
		public string? BaseUrl { get; set; }
		public string? ApiKeyHeader { get; set; } = "X-API-Key";
		public string? ApiKey { get; set; }
		public string AnalyzeEndpointPath { get; set; } = "/v1/analyze";
		public int TimeoutSeconds { get; set; } = 30;
	}

	public sealed class HttpModelInferenceService : IModelInferenceService
	{
		private readonly HttpClient _httpClient;
		private readonly HttpInferenceOptions _options;

		public HttpModelInferenceService(HttpClient httpClient, IOptions<HttpInferenceOptions> options)
		{
			_httpClient = httpClient;
			_options = options.Value;
			if (!string.IsNullOrWhiteSpace(_options.BaseUrl))
			{
				_httpClient.BaseAddress = new Uri(_options.BaseUrl);
			}
			_httpClient.Timeout = TimeSpan.FromSeconds(Math.Clamp(_options.TimeoutSeconds, 5, 300));
		}

		public async Task<ModelInferenceResult> AnalyzeImageAsync(byte[] imageData, IDictionary<string, string>? metadata, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(_options.BaseUrl))
			{
				throw new InvalidOperationException("HttpInferenceOptions.BaseUrl is not configured");
			}

			using var form = new MultipartFormDataContent();
			var fileContent = new ByteArrayContent(imageData);
			fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
			form.Add(fileContent, "file", metadata != null && metadata.TryGetValue("filename", out var fname) ? fname : "image.png");

			if (metadata != null)
			{
				foreach (var kvp in metadata)
				{
					form.Add(new StringContent(kvp.Value ?? string.Empty), $"meta_{kvp.Key}");
				}
			}

			var request = new HttpRequestMessage(HttpMethod.Post, _options.AnalyzeEndpointPath)
			{
				Content = form
			};
			if (!string.IsNullOrWhiteSpace(_options.ApiKey))
			{
				var headerName = string.IsNullOrWhiteSpace(_options.ApiKeyHeader) ? "X-API-Key" : _options.ApiKeyHeader!;
				request.Headers.TryAddWithoutValidation(headerName, _options.ApiKey);
			}

			var response = await _httpClient.SendAsync(request, cancellationToken);
			response.EnsureSuccessStatusCode();

			var payload = await response.Content.ReadFromJsonAsync<HttpAnalyzeResponse>(cancellationToken: cancellationToken)
				?? throw new InvalidOperationException("Empty response from inference backend");

			return new ModelInferenceResult
			{
				Scores = payload.Scores ?? new Dictionary<string, object>(),
				Metadata = payload.Metadata ?? new Dictionary<string, string>()
			};
		}

		private sealed class HttpAnalyzeResponse
		{
			public Dictionary<string, object>? Scores { get; set; }
			public Dictionary<string, string>? Metadata { get; set; }
		}
	}

	// Lightweight mock for local development when an external HTTP inference backend is not available.
	// Map under /mock-inference/analyze to accept multipart file and return synthetic scores.
	public static class HttpInferenceMockEndpoint
	{
		public static void MapHttpInferenceMock(this WebApplication app)
		{
			app.MapPost("/mock-inference/analyze", async (HttpRequest request) =>
			{
				if (!request.HasFormContentType) return Results.BadRequest("multipart/form-data required");
				var form = await request.ReadFormAsync();
				var file = form.Files.GetFile("file");
				if (file is null || file.Length == 0) return Results.BadRequest("file is required");
				// Return deterministic fake scores
				var payload = new
				{
					Scores = new Dictionary<string, object>
					{
						["arousal"] = 0.42,
						["engagement"] = 0.73
					},
					Metadata = new Dictionary<string, string>
					{
						["filename"] = file.FileName
					}
				};
				return Results.Ok(payload);
			}).WithOpenApi();
		}
	}
}