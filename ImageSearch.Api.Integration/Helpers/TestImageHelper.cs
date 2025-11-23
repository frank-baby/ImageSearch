using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageSearch.Api.IntegrationTests.Helpers
{
    public static class TestImageHelper
    {
        public static async Task<byte[]> CreateValidJpegBytesAsync(int width = 1, int height = 1, int quality = 85)
        {
            using var image = new Image<Rgb24>(width, height);
            using var memoryStream = new MemoryStream();
            await image.SaveAsJpegAsync(memoryStream, new JpegEncoder { Quality = quality });

            return memoryStream.ToArray();
        }
    }
}
