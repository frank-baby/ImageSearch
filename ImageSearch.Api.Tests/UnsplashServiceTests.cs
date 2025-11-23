using System.Net;
using System.Net.Http.Json;
using ImageSearch.Api.Configuration;
using ImageSearch.Api.Domain;
using ImageSearch.Api.Exceptions;
using ImageSearch.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace ImageSearch.Api.Tests
{
    public class UnsplashServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly UnsplashService _service;

        public UnsplashServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.unsplash.com/")
            };

            var mockOptions = new Mock<IOptions<UnsplashSettings>>();
            mockOptions.Setup(o => o.Value).Returns(new UnsplashSettings { ApiKey = "test-key" });

            var mockLogger = new Mock<ILogger<UnsplashService>>();

            _service = new UnsplashService(httpClient, mockOptions.Object, mockLogger.Object);
        }

        [Fact]
        public async Task SearchPhotosAsync_ReturnsPhotos_WhenApiCallIsSuccessful()
        {
            // Arrange
            var query = "test";
            var expectedPhotos = new List<UnsplashPhoto>
            {
                new UnsplashPhoto
                {
                    Id = "1",
                    Description = "Test Photo",
                    Urls = new UnsplashPhotoUrls("https://example.com/photo1.jpg")
                }
            };
            var searchResponse = new UnsplashSearchResponse { Results = expectedPhotos };

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(searchResponse)
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _service.SearchPhotosAsync(query, 10, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("1", result[0].Id);
        }

        [Fact]
        public async Task SearchPhotosAsync_ThrowsUnsplashApiException_WhenApiReturnsError()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("Unauthorized")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage);

            // Act & Assert
            await Assert.ThrowsAsync<UnsplashApiException>(() =>
                _service.SearchPhotosAsync("test", 10, CancellationToken.None));
        }
    }
}
