namespace ImageSearch.Api.Exceptions
{
    public class RateLimitExceededException : Exception
    {
        public TimeSpan? RetryAfter { get; }

        public RateLimitExceededException(string message, TimeSpan? retryAfter = null)
            : base(message)
        {
            RetryAfter = retryAfter;
        }

        public RateLimitExceededException(string message, Exception innerException, TimeSpan? retryAfter = null)
            : base(message, innerException)
        {
            RetryAfter = retryAfter;
        }
    }
}
