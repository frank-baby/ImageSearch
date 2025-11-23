namespace ImageSearch.Api.Services;

public interface IStorageService
{
    Task<string> SaveImageAsync(Stream image, string fileName, CancellationToken cancellationToken);
}