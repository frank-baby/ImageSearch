using System.Net;
using ImageSearch.Api.IntegrationTests.Helpers;
using ImageSearch.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ImageSearch.Api.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IUnsplashService> MockUnsplashService { get; } = new Mock<IUnsplashService>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing IUnsplashService registration
                var unsplashDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUnsplashService));

                if (unsplashDescriptor != null)
                {
                    services.Remove(unsplashDescriptor);
                }

                services.AddSingleton(MockUnsplashService.Object);

                // Replace HttpClient for ImageProcessingService with a test client
                services.AddHttpClient<IImageProcessingService, ImageProcessingService>()
                    .ConfigurePrimaryHttpMessageHandler(() => new TestImageHttpMessageHandler());
            });
        }
    }

    public class TestImageHttpMessageHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var imageBytes = await TestImageHelper.CreateValidJpegBytesAsync();

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(imageBytes) };
        }
    }
}
