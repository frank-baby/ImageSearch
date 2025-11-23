using FluentValidation;
using FluentValidation.Results;
using ImageSearch.Api.Controllers;
using ImageSearch.Api.Domain;
using ImageSearch.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace ImageSearch.Api.Tests
{
    public class ImagesControllerTests
    {
        private readonly Mock<IUnsplashService> _mockUnsplashService;
        private readonly Mock<IImageProcessingService> _mockImageProcessingService;
        private readonly Mock<IValidator<SearchRequest>> _mockValidator;
        private readonly ImagesController _controller;

        public ImagesControllerTests()
        {
            _mockUnsplashService = new Mock<IUnsplashService>();
            _mockImageProcessingService = new Mock<IImageProcessingService>();
            _mockValidator = new Mock<IValidator<SearchRequest>>();

            _controller = new ImagesController(
                _mockUnsplashService.Object,
                _mockImageProcessingService.Object,
                _mockValidator.Object
            );
        }

        [Fact]
        public async Task Search_WithValidQuery_ReturnsOkResult()
        {
            // Arrange
            var query = "cars";
            var request = new SearchRequest { SearchQuery = query };
            var unsplashPhotos = new List<UnsplashPhoto>
            {
                new UnsplashPhoto { Id = "1", Urls = new UnsplashPhotoUrls("https://example.com/photo1.jpg") }
            };
            var processedImages = new List<ProcessedImageResult>
            {
                new ProcessedImageResult { Success = true, ImageId = "1", SmallImageUrl = "/api/images/small.jpg" }
            };

            _mockValidator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUnsplashService
                .Setup(s => s.SearchPhotosAsync(query, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(unsplashPhotos);

            _mockImageProcessingService
                .Setup(s => s.ProcessImagesAsync(unsplashPhotos, It.IsAny<CancellationToken>()))
                .ReturnsAsync(processedImages);

            // Act
            var result = await _controller.Search(request, CancellationToken.None);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<SearchResponse>(actionResult.Value);
            Assert.Equal(query, response.SearchQuery);
            Assert.Equal(1, response.TotalProcessed);
            Assert.Equal(0, response.TotalFailed);
            Assert.DoesNotContain(response.ProcessedImages, i => !i.Success);
        }

        [Fact]
        public async Task Search_WhenValidationFails_ReturnsBadRequestWithErrors()
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = "" };

            var validationFailures = new List<ValidationFailure>
            {
                new ValidationFailure("SearchQuery", "Validation error")
            };

            _mockValidator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailures));

            // Act
            var result = await _controller.Search(request, CancellationToken.None);

            // Assert
            var actionResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.NotNull(actionResult.Value);

            var errors = actionResult.Value.GetType().GetProperty("errors")?.GetValue(actionResult.Value);
            Assert.NotNull(errors);
        }

        [Fact]
        public async Task Search_WithSuccessAndFailure_ReturnsOnlySuccessfulImages()
        {
            // Arrange
            var query = "test";
            var request = new SearchRequest { SearchQuery = query };
            var unsplashPhotos = new List<UnsplashPhoto>
            {
                new UnsplashPhoto { Id = "1", Urls = new UnsplashPhotoUrls("https://example.com/photo1.jpg") },
                new UnsplashPhoto { Id = "2", Urls = new UnsplashPhotoUrls("https://example.com/photo1.jpg") }
            };
            var processedImages = new List<ProcessedImageResult>
            {
                new ProcessedImageResult
                {
                    Success = true, ImageId = "1", SmallImageUrl = "/api/images/1_small.jpg"
                },
                new ProcessedImageResult { Success = false, ImageId = "2", ErrorMessage = "Processing failed" }
            };

            _mockValidator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockUnsplashService
                .Setup(s => s.SearchPhotosAsync(query, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(unsplashPhotos);

            _mockImageProcessingService
                .Setup(s => s.ProcessImagesAsync(unsplashPhotos, It.IsAny<CancellationToken>()))
                .ReturnsAsync(processedImages);

            // Act
            var result = await _controller.Search(request, CancellationToken.None);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<SearchResponse>(actionResult.Value);
            Assert.Equal(1, response.TotalProcessed);
            Assert.Equal(1, response.TotalFailed);
            Assert.Single(response.ProcessedImages);
            Assert.Equal("1", response.ProcessedImages[0].ImageId);
        }

        [Fact]
        public async Task Search_InvokesServicesInCorrectOrder()
        {
            // Arrange
            var query = "test";
            var request = new SearchRequest { SearchQuery = query };
            var callOrder = new List<string>();

            _mockValidator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .Callback(() => callOrder.Add("validate"))
                .ReturnsAsync(new ValidationResult());

            _mockUnsplashService
                .Setup(s => s.SearchPhotosAsync(query, 10, It.IsAny<CancellationToken>()))
                .Callback(() => callOrder.Add("unsplash"))
                .ReturnsAsync(new List<UnsplashPhoto>());

            _mockImageProcessingService
                .Setup(s => s.ProcessImagesAsync(It.IsAny<List<UnsplashPhoto>>(), It.IsAny<CancellationToken>()))
                .Callback(() => callOrder.Add("process"))
                .ReturnsAsync(new List<ProcessedImageResult>());

            // Act
            await _controller.Search(request, CancellationToken.None);

            // Assert
            Assert.Equal(new[] { "validate", "unsplash", "process" }, callOrder);
        }

        [Fact]
        public async Task Search_PropagatesSameCancellationTokenToAllServices()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var request = new SearchRequest { SearchQuery = "test" };
            var photos = new List<UnsplashPhoto>();

            _mockValidator
                .Setup(v => v.ValidateAsync(request, cts.Token))
                .ReturnsAsync(new ValidationResult());

            _mockUnsplashService
                .Setup(s => s.SearchPhotosAsync(It.IsAny<string>(), It.IsAny<int>(), cts.Token))
                .ReturnsAsync(photos);

            _mockImageProcessingService
                .Setup(s => s.ProcessImagesAsync(photos, cts.Token))
                .ReturnsAsync(new List<ProcessedImageResult>());

            // Act
            await _controller.Search(request, cts.Token);

            // Assert
            _mockValidator.Verify(v => v.ValidateAsync(request, cts.Token), Times.Once);
            _mockUnsplashService.Verify(s => s.SearchPhotosAsync(It.IsAny<string>(),
                    It.IsAny<int>(),
                    cts.Token),
                Times.Once);
            _mockImageProcessingService.Verify(s => s.ProcessImagesAsync(It.IsAny<List<UnsplashPhoto>>(),
                    cts.Token),
                Times.Once);
        }
    }
}
