namespace WebApp1.Domain
{
    public class AuthResult
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = null!;
    }
}
