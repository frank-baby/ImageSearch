using ImageSearch.Api.Services;
using Moq;

namespace ImageSearch.Api.IntegrationTests
{
    public class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly HttpClient Client;
        protected readonly Mock<IUnsplashService> MockUnsplashService;

        protected IntegrationTestBase(CustomWebApplicationFactory factory)
        {
            Client = factory.CreateClient();
            MockUnsplashService = factory.MockUnsplashService;

            MockUnsplashService.Reset();
        }
    }
}
