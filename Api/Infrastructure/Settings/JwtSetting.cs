namespace Api.Infrastructure.Settings;

public sealed class JwtSetting
{
    public const string SectionName = nameof(JwtSetting);

    public string Secret { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; }
}