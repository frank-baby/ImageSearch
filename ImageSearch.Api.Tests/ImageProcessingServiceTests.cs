using System.Net;
using ImageSearch.Api.Configuration;
using ImageSearch.Api.Domain;
using ImageSearch.Api.Services;
using ImageSearch.Api.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace ImageSearch.Api.Tests
{
    public class ImageProcessingServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IStorageService> _mockStorageService;
        private readonly ImageProcessingService _service;

        public ImageProcessingServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            _mockStorageService = new Mock<IStorageService>();
            var mockOptions = new Mock<IOptions<ImageProcessingSettings>>();
            mockOptions.Setup(o => o.Value).Returns(new ImageProcessingSettings
            {
                MaxConcurrency = 3,
                SmallImageDimension = 1024,
                ThumbnailDimension = 256,
                OutputDirectory = "test-images"
            });

            var mockLogger = new Mock<ILogger<ImageProcessingService>>();

            _service = new ImageProcessingService(
                httpClient,
                _mockStorageService.Object,
                mockOptions.Object,
                mockLogger.Object
            );
        }

        [Fact]
        public async Task ProcessImagesAsync_ReturnsSuccessfulResults_WhenAllImagesProcessSuccessfully()
        {
            // Arrange
            var photos = new List<UnsplashPhoto>
            {
                new UnsplashPhoto
                {
                    Id = "photo1",
                    AltDescription = "Test Photo 1",
                    Urls = new UnsplashPhotoUrls("https://example.com/photo1.jpg")
                }
            };

            SetupSuccessfulImageDownload();
            _mockStorageService
                .Setup(s => s.SaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("/images/test.jpg");

            // Act
            var results = await _service.ProcessImagesAsync(photos, CancellationToken.None);

            // Assert
            Assert.Single(results);
            Assert.True(results[0].Success);
            Assert.Equal("photo1", results[0].ImageId);
            Assert.NotNull(results[0].SmallImageUrl);
            Assert.NotNull(results[0].ThumbnailUrl);
        }

        [Fact]
        public async Task ProcessImagesAsync_WhenCancelled_Raises_OperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var photos = new List<UnsplashPhoto>
            {
                new UnsplashPhoto { Id = "photo1", Urls = new("https://example.com/photo1.jpg") }
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback(() => cts.Cancel())
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await _service.ProcessImagesAsync(photos, cts.Token);
            });
        }

        [Fact]
        public async Task ProcessImagesAsync_WithOneFailure_ProcessesOthers()
        {
            // Arrange
            var photos = new List<UnsplashPhoto>
            {
                new UnsplashPhoto { Id = "photo1", Urls = new UnsplashPhotoUrls("https://example.com/photo1.jpg") },
                new UnsplashPhoto { Id = "photo2", Urls = new UnsplashPhotoUrls("https://example.com/photo2.jpg") },
                new UnsplashPhoto { Id = "photo3", Urls = new UnsplashPhotoUrls("https://example.com/photo3.jpg") }
            };

            // Mock First & Third success, Second failure
            var callCount = 0;
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    callCount++;

                    if (callCount == 2)
                    {
                        throw new HttpRequestException("Network error");
                    }

                    return CreateSuccessfulImageResponse();
                });

            _mockStorageService
                .Setup(s => s.SaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("/images/test.jpg");

            // Act
            var results = await _service.ProcessImagesAsync(photos, CancellationToken.None);

            // Assert
            Assert.Equal(3, results.Count);
            Assert.Equal(2, results.Count(r => r.Success));
            Assert.Single(results, r => !r.Success);

            var failedImage = results.First(r => !r.Success);
            Assert.NotNull(failedImage.ErrorMessage);
        }

        [Fact]
        public async Task ProcessImagesAsync_HandlesSaveFailure()
        {
            // Arrange
            var photos = new List<UnsplashPhoto>
            {
                new UnsplashPhoto { Id = "photo1", Urls = new UnsplashPhotoUrls("https://example.com/photo1.jpg") }
            };

            SetupSuccessfulImageDownload();
            _mockStorageService
                .Setup(s => s.SaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Disk full"));

            // Act
            var results = await _service.ProcessImagesAsync(photos, CancellationToken.None);

            // Assert
            Assert.Single(results);
            Assert.False(results[0].Success);
            Assert.Contains("Disk full", results[0].ErrorMessage);
        }

        [Fact]
        public async Task ProcessImagesAsync_HandlesInvalidImageData()
        {
            // Arrange
            var photos = new List<UnsplashPhoto>
            {
                new UnsplashPhoto { Id = "photo1", Urls = new UnsplashPhotoUrls("https://example.com/photo1.jpg") }
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(new byte[] { 0x00 }) // Invalid image data
                });

            // Act
            var results = await _service.ProcessImagesAsync(photos, CancellationToken.None);

            // Assert
            Assert.Single(results);
            Assert.False(results[0].Success);
            Assert.NotNull(results[0].ErrorMessage);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public async Task ProcessImagesAsync_HandlesVariousPhotoCount(int photoCount)
        {
            // Arrange
            var photos = Enumerable.Range(1, photoCount).Select(i => new UnsplashPhoto
            {
                Id = $"photo{i}",
                AltDescription = $"Photo {i}",
                Urls = new UnsplashPhotoUrls($"https://example.com/photo{i}.jpg")
            }).ToList();

            SetupSuccessfulImageDownload();
            _mockStorageService
                .Setup(s => s.SaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("/images/test.jpg");

            // Act
            var results = await _service.ProcessImagesAsync(photos, CancellationToken.None);

            // Assert
            Assert.Equal(photoCount, results.Count);
            Assert.All(results, r => Assert.True(r.Success));
        }

        private void SetupSuccessfulImageDownload()
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(CreateSuccessfulImageResponse);
        }

        private HttpResponseMessage CreateSuccessfulImageResponse()
        {
            var imageBytes = TestImageHelper.CreateValidJpegBytes();

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(imageBytes) };
        }
    }
}
