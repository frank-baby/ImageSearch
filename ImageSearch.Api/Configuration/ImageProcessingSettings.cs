namespace ImageSearch.Api.Configuration
{
    public class ImageProcessingSettings
    {
        public const string SectionName = "ImageProcessing";
        public string OutputDirectory { get; set; } = string.Empty;
        public int MaxConcurrency { get; set; }
        public int SmallImageDimension { get; set; }
        public int ThumbnailDimension { get; set; }
    }
}
