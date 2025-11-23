using ImageSearch.Api.Configuration;
using Microsoft.Extensions.Options;

namespace ImageSearch.Api.Services;

public class FileStorageService : IStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _outputDirectory;

    public FileStorageService(IOptions<ImageProcessingSettings> options,
        ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _outputDirectory = options.Value.OutputDirectory ?? "processed-images";

        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }
    }

    public async Task<string> SaveImageAsync(
        Stream imageStream,
        string fileName,
        CancellationToken cancellationToken)
    {
        var sanitizedFileName = Path.GetFileName(fileName);
        if (string.IsNullOrWhiteSpace(sanitizedFileName) || sanitizedFileName != fileName)
        {
            throw new ArgumentException($"Invalid file name: {fileName}", nameof(fileName));
        }

        var filePath = Path.Combine(_outputDirectory, sanitizedFileName);

        try
        {
            await using var fileStream = File.Create(filePath);
            await imageStream.CopyToAsync(fileStream, cancellationToken);

            _logger.LogInformation("Saved image to {FilePath}", filePath);

            return $"/images/{sanitizedFileName}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image {FileName}", sanitizedFileName);
            throw;
        }
    }
}
