namespace AuthLab.Settings
{
    /// <summary>
    /// JWT configuration settings class to hold the necessary parameters for generating and validating JWT tokens.
    /// This class can be used to bind configuration values from appsettings.json or other configuration sources, making it easier to manage JWT settings in a centralized manner.
    /// </summary>
    public class JwtConfig
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; }
    }
}
