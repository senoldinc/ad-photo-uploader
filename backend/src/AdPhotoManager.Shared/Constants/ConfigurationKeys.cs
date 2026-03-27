namespace AdPhotoManager.Shared.Constants;

public static class ConfigurationKeys
{
    // Connection Strings
    public const string DefaultConnection = "ConnectionStrings:DefaultConnection";
    public const string HangfireConnection = "Hangfire:ConnectionString";

    // Active Directory
    public const string AdServer = "ActiveDirectory:Server";
    public const string AdPort = "ActiveDirectory:Port";
    public const string AdUseSsl = "ActiveDirectory:UseSsl";
    public const string AdBaseDn = "ActiveDirectory:BaseDn";
    public const string AdServiceAccount = "ActiveDirectory:ServiceAccount";
    public const string AdServicePassword = "ActiveDirectory:ServicePassword";
    public const string AdAllowedOrganizations = "ActiveDirectory:AllowedOrganizations";
    public const string AdSyncIntervalHours = "ActiveDirectory:SyncIntervalHours";

    // Azure AD / OIDC
    public const string AzureAdInstance = "AzureAd:Instance";
    public const string AzureAdTenantId = "AzureAd:TenantId";
    public const string AzureAdClientId = "AzureAd:ClientId";
    public const string AzureAdClientSecret = "AzureAd:ClientSecret";
    public const string AzureAdCallbackPath = "AzureAd:CallbackPath";

    // JWT
    public const string JwtIssuer = "Jwt:Issuer";
    public const string JwtAudience = "Jwt:Audience";
    public const string JwtSecretKey = "Jwt:SecretKey";
    public const string JwtExpirationMinutes = "Jwt:ExpirationMinutes";

    // Photo constraints
    public const int MaxSourcePhotoSizeBytes = 500 * 1024; // 500 KB
    public const int MaxOutputPhotoSizeBytes = 100 * 1024; // 100 KB
    public const int PhotoDimension = 300; // 300x300 px
    public const int MinPhotoQuality = 30; // 30%
    public const int MaxPhotoQuality = 100; // 100%
    public const int DefaultPhotoQuality = 85; // 85%

    // Pagination
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}
