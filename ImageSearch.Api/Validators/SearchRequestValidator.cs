using FluentValidation;
using ImageSearch.Api.Domain;

namespace ImageSearch.Api.Validators
{
    public class SearchRequestValidator : AbstractValidator<SearchRequest>
    {
        public SearchRequestValidator()
        {
            RuleFor(x => x.SearchQuery)
                .NotEmpty()
                .WithMessage("Search query is required")
                .MinimumLength(2)
                .WithMessage("Search query must be at least 2 characters")
                .MaximumLength(100)
                .WithMessage("Search query must not exceed 100 characters")
                .Matches(@"^[a-zA-Z0-9\s\-_.,!?&]+$")
                .WithMessage("Search query contains invalid characters.");
        }
    }
}
