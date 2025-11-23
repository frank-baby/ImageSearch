using ImageSearch.Api.Domain;

namespace ImageSearch.Api.Services
{
    public interface IUnsplashService
    {
        Task<List<UnsplashPhoto>> SearchPhotosAsync(string query, int count, CancellationToken cancellationToken);
    }
}
