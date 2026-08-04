namespace SharedKernel.Authentication.Options;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public string Authority
    {
        get;
        set;
    } = string.Empty;

    public string Audience
    {
        get;
        set;
    } = string.Empty;

    public bool RequireHttpsMetadata
    {
        get;
        set;
    } = true;

    public string MetadataAddress
    {
        get;
        set;
    } = string.Empty;
}