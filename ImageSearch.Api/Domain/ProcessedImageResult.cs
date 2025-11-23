namespace ImageSearch.Api.Domain
{
    public class ProcessedImageResult
    {
        public bool Success { get; set; }
        public required string ImageId { get; set; }
        public string? AltDescription { get; set; }
        public string? SmallImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? Description { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
