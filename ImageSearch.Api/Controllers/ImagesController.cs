using FluentValidation;
using ImageSearch.Api.Domain;
using ImageSearch.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImageSearch.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        const int MaxSearchResults = 10;

        private readonly IUnsplashService _unsplashService;
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IValidator<SearchRequest> _validator;

        public ImagesController(
            IUnsplashService unsplashService,
            IImageProcessingService imageProcessingService,
            IValidator<SearchRequest> validator)
        {
            _unsplashService = unsplashService;
            _imageProcessingService = imageProcessingService;
            _validator = validator;
        }

        [HttpPost("search")]
        public async Task<ActionResult<SearchResponse>> Search([FromBody] SearchRequest request,
            CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    errors = validationResult.Errors.Select(e => new
                    {
                        property = e.PropertyName,
                        message = e.ErrorMessage
                    })
                });
            }

            var searchResults = await _unsplashService.SearchPhotosAsync(
                request.SearchQuery,
                count: MaxSearchResults,
                cancellationToken
            );

            var processedImages = await _imageProcessingService.ProcessImagesAsync(
                searchResults,
                cancellationToken
            );

            return Ok(new SearchResponse
            {
                SearchQuery = request.SearchQuery,
                TotalProcessed = processedImages.Count(i => i.Success),
                TotalFailed = processedImages.Count(i => !i.Success),
                ProcessedImages = processedImages.Where(i => i.Success).ToList()
            });
        }
    }
}
