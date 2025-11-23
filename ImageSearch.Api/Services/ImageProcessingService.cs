using System.Collections.Concurrent;
using ImageSearch.Api.Configuration;
using ImageSearch.Api.Domain;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace ImageSearch.Api.Services
{
    public class ImageProcessingService : IImageProcessingService
    {
        private readonly int _maxConcurrency;
        private readonly int _smallImageDimension;
        private readonly int _thumbNailDimension;
        private readonly HttpClient _httpClient;
        private readonly IStorageService _storageService;
        private readonly ILogger<ImageProcessingService> _logger;

        public ImageProcessingService(HttpClient httpClient,
            IStorageService storageService,
            IOptions<ImageProcessingSettings> options,
            ILogger<ImageProcessingService> logger)
        {
            _maxConcurrency = options.Value.MaxConcurrency > 0 ? options.Value.MaxConcurrency : 3;
            _smallImageDimension = options.Value.SmallImageDimension > 0 ? options.Value.SmallImageDimension : 1024;
            _thumbNailDimension = options.Value.ThumbnailDimension > 0 ? options.Value.ThumbnailDimension : 256;

            _httpClient = httpClient;
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<List<ProcessedImageResult>> ProcessImagesAsync(List<UnsplashPhoto> photos,
            CancellationToken cancellationToken)
        {
            var results = new ConcurrentBag<ProcessedImageResult>();

            await Parallel.ForEachAsync(photos,
                new ParallelOptions
                { MaxDegreeOfParallelism = _maxConcurrency, CancellationToken = cancellationToken },
                async (photo, ct) =>
                {
                    var result = await ProcessSingleImageAsync(photo, ct);
                    results.Add(result);
                });

            return results.ToList();
        }

        private async Task<ProcessedImageResult> ProcessSingleImageAsync(UnsplashPhoto photo,
            CancellationToken cancellationToken)
        {
            try
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(
                    photo.Urls.Raw,
                    cancellationToken
                );

                cancellationToken.ThrowIfCancellationRequested();

                var smallImagePath = await ResizeAndSaveAsync(
                    imageBytes,
                    photo.Id,
                    _smallImageDimension,
                    "small",
                    cancellationToken
                );

                var thumbNailPath = await ResizeAndSaveAsync(
                    imageBytes,
                    photo.Id,
                    _thumbNailDimension,
                    "thumb",
                    cancellationToken
                );

                return new ProcessedImageResult
                {
                    Success = true,
                    ImageId = photo.Id,
                    AltDescription = photo.AltDescription,
                    Description = photo.Description,
                    SmallImageUrl = smallImagePath,
                    ThumbnailUrl = thumbNailPath,
                };
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Processing cancelled for {PhotoId}", photo.Id);
                return new ProcessedImageResult
                {
                    Success = false,
                    ImageId = photo.Id,
                    ErrorMessage = "Processing cancelled"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process {PhotoId}", photo.Id);
                return new ProcessedImageResult
                {
                    Success = false,
                    ImageId = photo.Id,
                    ErrorMessage = ex.Message
                };
            }
        }

        private async Task<string> ResizeAndSaveAsync(
            byte[] imageBytes,
            string imageId,
            int maxDimension,
            string sizeSuffix,
            CancellationToken cancellationToken)
        {
            using var inputStream = new MemoryStream(imageBytes);
            using var image = await Image.LoadAsync(inputStream, cancellationToken);

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(maxDimension, maxDimension),
                Mode = ResizeMode.Max
            }));

            using var outputStream = new MemoryStream();
            await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 85 }, cancellationToken);
            outputStream.Position = 0;

            var fileName = $"{imageId}_{sizeSuffix}.jpg";
            var imageUrl = await _storageService.SaveImageAsync(outputStream, fileName, cancellationToken);

            return imageUrl;
        }
    }
}
