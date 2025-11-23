using ImageSearch.Api.Configuration;
using ImageSearch.Api.Domain;
using ImageSearch.Api.Exceptions;
using Microsoft.Extensions.Options;

namespace ImageSearch.Api.Services
{
    public class UnsplashService : IUnsplashService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UnsplashService> _logger;
        private readonly string _apiKey;

        public UnsplashService(HttpClient httpClient, IOptions<UnsplashSettings> options, ILogger<UnsplashService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _apiKey = options.Value.ApiKey ?? throw new InvalidOperationException("UnsplashApiKey is missing");
        }

        public async Task<List<UnsplashPhoto>> SearchPhotosAsync(string query, int count,
            CancellationToken cancellationToken)
        {
            var url = $"search/photos?client_id={_apiKey}&page=1&per_page={count}&query={Uri.EscapeDataString(query)}";

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync(cancellationToken);

                    _logger.LogWarning("Unsplash API returned {ResponseStatusCode}: {Error}", response.StatusCode, error);

                    // Handle 50 requests/hour rate limit for free tier
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        TimeSpan? retryAfter = null;

                        if (response.Headers.RetryAfter?.Delta.HasValue == true)
                        {
                            retryAfter = response.Headers.RetryAfter.Delta.Value;
                        }
                        else if (response.Headers.RetryAfter?.Date.HasValue == true)
                        {
                            retryAfter = response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
                        }

                        var retryMessage = retryAfter.HasValue
                            ? $"Please try again in {retryAfter.Value.TotalMinutes:F0} minutes"
                            : "Please try again later (free tier: 50 requests/hour)";

                        throw new RateLimitExceededException(
                            $"Unsplash API rate limit exceeded. {retryMessage}",
                            retryAfter);
                    }

                    throw new UnsplashApiException($"API error: {response.StatusCode}");
                }

                var result = await response.Content.ReadFromJsonAsync<UnsplashSearchResponse>(
                    cancellationToken: cancellationToken
                );

                return result?.Results ?? [];
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Network error calling Unsplash API");
                throw new UnsplashApiException("Failed to reach image search service", ex);
            }
        }
    }
}
