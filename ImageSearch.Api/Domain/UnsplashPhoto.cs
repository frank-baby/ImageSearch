using System.Text.Json.Serialization;

namespace ImageSearch.Api.Domain
{
    public class UnsplashPhoto
    {
        public required string Id { get; set; }
        
        [JsonPropertyName("alt_description")]
        public string? AltDescription { get; set; }
        public string? Description { get; set; }
        public required UnsplashPhotoUrls Urls { get; set; }
    }

    public record UnsplashPhotoUrls(string Raw);
}
