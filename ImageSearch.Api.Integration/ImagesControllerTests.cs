using System.Net;
using System.Net.Http.Json;
using ImageSearch.Api.Domain;
using ImageSearch.Api.Exceptions;
using Moq;

namespace ImageSearch.Api.IntegrationTests
{
    public class ImagesControllerTests : IntegrationTestBase
    {
        public ImagesControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task Search_WithEmptyQuery_ReturnsBadRequest()
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = "" };

            // Act
            var response = await Client.PostAsJsonAsync("/api/images/search", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Search_WithValidQuery_ReturnsOkAndImages()
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = "cars" };

            var mockPhotos = Enumerable.Range(1, 10).Select(i => new UnsplashPhoto
            {
                Id = $"car{i}",
                AltDescription = $"Photo of {i}",
                Urls = new UnsplashPhotoUrls($"https://example.com/photo{i}.jpg")
            }).ToList();

            MockUnsplashService.Setup(s => s.SearchPhotosAsync("cars", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockPhotos);

            // Act
            var response = await Client.PostAsJsonAsync("/api/images/search", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<SearchResponse>();
            Assert.NotNull(result);
            Assert.Equal("cars", result.SearchQuery);
            Assert.Equal(10, result.TotalProcessed);
            Assert.Equal(0, result.TotalFailed);
            Assert.Equal(10, result.ProcessedImages.Count);

            Assert.All(result.ProcessedImages, img =>
            {
                Assert.NotNull(img.SmallImageUrl);
                Assert.NotNull(img.ThumbnailUrl);
            });
        }

        [Fact]
        public async Task Search_WhenUnsplashFails_ReturnsServiceUnavailable()
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = "cars" };

            MockUnsplashService.Setup(s => s.SearchPhotosAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnsplashApiException("API Error"));

            // Act
            var response = await Client.PostAsJsonAsync("/api/images/search", request);

            // Assert
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        }
    }
}
