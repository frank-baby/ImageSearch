using FluentValidation.TestHelper;
using ImageSearch.Api.Domain;
using ImageSearch.Api.Validators;

namespace ImageSearch.Api.Tests
{
    public class SearchRequestValidatorTests
    {
        private readonly SearchRequestValidator _validator;

        public SearchRequestValidatorTests()
        {
            _validator = new SearchRequestValidator();
        }

        [Fact]
        public async Task Validate_WithValidQuery_ShouldNotHaveValidationError()
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = "cars" };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("ab")]
        [InlineData("test")]
        [InlineData("beautiful mountains")]
        [InlineData("nature & wildlife")]
        [InlineData("coffee, tea")]
        public async Task Validate_WithValidQueries_ShouldNotHaveValidationError(string query)
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = query };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithEmptyQuery_ShouldHaveValidationError()
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = "" };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchQuery)
                .WithErrorMessage("Search query is required");
        }

        [Fact]
        public async Task Validate_WithNullQuery_ShouldHaveValidationError()
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = null };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchQuery);
        }

        [Fact]
        public async Task Validate_WithWhitespaceQuery_ShouldHaveValidationError()
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = "   " };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchQuery)
                .WithErrorMessage("Search query is required");
        }

        [Fact]
        public async Task Validate_WithQueryTooShort_ShouldHaveValidationError()
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = "a" };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchQuery)
                .WithErrorMessage("Search query must be at least 2 characters");
        }

        [Fact]
        public async Task Validate_WithQueryTooLong_ShouldHaveValidationError()
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = new string('a', 101) };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchQuery)
                .WithErrorMessage("Search query must not exceed 100 characters");
        }

        [Theory]
        [InlineData("test<script>")]
        [InlineData("alert('xss')")]
        [InlineData("test@abc.com")]
        [InlineData("test{script}")]
        [InlineData("test[xss]")]
        public async Task Validate_WithInvalidCharacters_ShouldHaveValidationError(string query)
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = query };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SearchQuery)
                .WithErrorMessage(
                    "Search query contains invalid characters.");
        }

        [Fact]
        public async Task Validate_WithMaxLengthQuery_ShouldNotHaveValidationError()
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = new string('a', 100) };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithMinLengthQuery_ShouldNotHaveValidationError()
        {
            // Arrange
            var request = new SearchRequest { SearchQuery = "ab" };

            // Act
            var result = await _validator.TestValidateAsync(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
