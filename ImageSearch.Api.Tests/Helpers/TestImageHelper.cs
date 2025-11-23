using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageSearch.Api.Tests.Helpers
{
    public static class TestImageHelper
    {
        public static byte[] CreateValidJpegBytes(int width = 1, int height = 1, int quality = 85)
        {
            using var image = new Image<Rgb24>(width, height);
            using var ms = new MemoryStream();
            image.SaveAsJpeg(ms, new JpegEncoder { Quality = quality });

            return ms.ToArray();
        }
    }
}
