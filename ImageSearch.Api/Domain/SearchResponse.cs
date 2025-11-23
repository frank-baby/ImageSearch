namespace ImageSearch.Api.Domain
{
    public class SearchResponse
    {
        public required string SearchQuery { get; set; }
        public int TotalNumber { get; set; }
        public int TotalProcessed { get; set; }
        public int TotalFailed { get; set; }
        public required List<ProcessedImageResult> ProcessedImages { get; set; }
    }
}
