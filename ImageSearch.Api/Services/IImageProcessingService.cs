using ImageSearch.Api.Domain;

namespace ImageSearch.Api.Services
{
    public interface IImageProcessingService
    {
        Task<List<ProcessedImageResult>> ProcessImagesAsync(List<UnsplashPhoto> photos,
            CancellationToken cancellationToken);
    }
}
