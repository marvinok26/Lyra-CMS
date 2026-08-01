using System.ComponentModel.DataAnnotations;

namespace Lyra.Onboarding.ViewModels;

public sealed class SignupViewModel
{
    [Required, StringLength(70, MinimumLength = 2)]
    public string StoreName { get; set; } = string.Empty;

    [Required, StringLength(30, MinimumLength = 3)]
    [RegularExpression("^[a-z][a-z0-9-]*[a-z0-9]$", ErrorMessage = "Use lowercase letters, numbers, and hyphens — start and end with a letter or number.")]
    public string Subdomain { get; set; } = string.Empty;

    [Required, StringLength(64, MinimumLength = 2)]
    public string AdminUsername { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string AdminEmail { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string AdminPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(AdminPassword))]
    public string AdminPasswordConfirmation { get; set; } = string.Empty;
}
