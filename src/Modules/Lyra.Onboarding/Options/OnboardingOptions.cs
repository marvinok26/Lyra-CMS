namespace Lyra.Onboarding.Options;

public sealed class OnboardingOptions
{
    public const string SectionName = "Lyra:Onboarding";

    /// <summary>Connection string used only to run `CREATE DATABASE` — no target Database segment.</summary>
    public string MasterConnectionString { get; set; } = string.Empty;

    /// <summary>Format string for a new tenant's own connection string; {0} is the generated database name.</summary>
    public string ConnectionStringTemplate { get; set; } = string.Empty;

    public string RecipeName { get; set; } = "SaaS";

    public string SiteTimeZone { get; set; } = "Etc/UTC";
}
