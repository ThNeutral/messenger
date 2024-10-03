namespace server.internals.helpers
{
    public class ErrorResponse
    {
        public string errorMessage { get; set; }
    }
    public class TokenResponse
    {
        public string token { get; set; }
        public int expiresIn { get; set; }
    }
    public static class ResponseHelpers
    {
    }
}
