
using FluentValidation;
using ImageSearch.Api.Configuration;
using ImageSearch.Api.Middleware;
using ImageSearch.Api.Services;
using ImageSearch.Api.Validators;
using Microsoft.Extensions.FileProviders;
using Polly;
using Polly.Extensions.Http;

namespace ImageSearch.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<IStorageService, FileStorageService>();

            builder.Services.AddHttpClient<IUnsplashService, UnsplashService>(client =>
            {
                client.BaseAddress = new Uri("https://api.unsplash.com/");
                client.Timeout = TimeSpan.FromSeconds(30);
            }).AddPolicyHandler(GetRetryPolicy());

            builder.Services.AddHttpClient<IImageProcessingService, ImageProcessingService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(2);
            });

            builder.Services.AddControllers();

            // Add FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<SearchRequestValidator>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.Configure<UnsplashSettings>(builder.Configuration.GetSection(UnsplashSettings.SectionName));
            builder.Services.Configure<ImageProcessingSettings>(builder.Configuration.GetSection(ImageProcessingSettings.SectionName));

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseCors(policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            }
            else
            {
                // To be replaced with production frontend URL(s)
                var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                    ?? new[] { "https://example.com" };

                app.UseCors(policy => policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
            }

            var imagesPath = Path.Combine(
                builder.Environment.ContentRootPath,
                "processed-images"
            );
            Directory.CreateDirectory(imagesPath);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(imagesPath),
                RequestPath = "/api/images",
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append(
                        "Cache-Control",
                        "public,max-age=3600"
                    );
                }
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseExceptionHandler();

            app.MapControllers();

            app.Run();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,
                    retryAttempt)));
        }
    }
}
