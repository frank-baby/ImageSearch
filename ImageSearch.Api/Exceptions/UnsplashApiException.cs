namespace ImageSearch.Api.Exceptions;

public class UnsplashApiException:Exception
{
    public UnsplashApiException(string message) : base(message) { }
    public UnsplashApiException(string message, Exception innerException) : base(message, innerException) { }  
}