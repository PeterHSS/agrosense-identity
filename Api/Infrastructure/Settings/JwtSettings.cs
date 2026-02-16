namespace Api.Infrastructure.Settings;

public sealed class JwtSettings
{
    public const string SectionName = nameof(JwtSettings);

    public string Secret { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; }
}