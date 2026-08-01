using Lyra.Onboarding.Options;
using Lyra.Onboarding.Services;
using Lyra.Onboarding.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Setup.Services;

namespace Lyra.Onboarding.Controllers;

/// <summary>
/// A public "create your store" wizard wrapping tenant creation + setup in one request — the
/// built-in /Admin/Tenants screen (used in Phases 1 and 3) requires platform-admin access, which
/// is right for an operator provisioning tenants by hand but wrong for letting anyone sign up.
/// Only meaningful on the Default tenant, mirroring how OrchardCore.Tenants' own TenantApiController
/// gates itself.
/// </summary>
[Route("signup")]
public sealed class SignupController(
    ShellSettings currentShellSettings,
    IShellHost shellHost,
    IShellSettingsManager shellSettingsManager,
    ISetupService setupService,
    TenantDatabaseInitializer databaseInitializer,
    StoreProvisioner storeProvisioner,
    IOptions<OnboardingOptions> options) : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        if (!currentShellSettings.IsDefaultShell()) return NotFound();

        return View(new SignupViewModel());
    }

    [HttpPost(""), ActionName(nameof(Index))]
    public async Task<IActionResult> IndexPost(SignupViewModel model)
    {
        if (!currentShellSettings.IsDefaultShell()) return NotFound();

        if (!ModelState.IsValid) return View(nameof(Index), model);

        var tenantName = "lyra" + new string(model.Subdomain.Where(char.IsLetterOrDigit).ToArray());
        if (shellHost.TryGetShellContext(tenantName, out _) || shellHost.GetAllSettings().Any(s => s.Name == tenantName))
        {
            ModelState.AddModelError(nameof(model.Subdomain), "That name is already taken — try another.");
            return View(nameof(Index), model);
        }

        var settings = options.Value;
        var databaseName = "Lyra" + char.ToUpperInvariant(tenantName[4]) + tenantName[5..];
        var connectionString = string.Format(settings.ConnectionStringTemplate, databaseName);

        await databaseInitializer.CreateDatabaseAsync(databaseName);

        using (var shellSettings = shellSettingsManager.CreateDefaultSettings().AsUninitialized().AsDisposable())
        {
            shellSettings.Name = tenantName;
            shellSettings.RequestUrlPrefix = model.Subdomain;
            shellSettings["ConnectionString"] = connectionString;
            shellSettings["DatabaseProvider"] = "SqlConnection";
            shellSettings["Secret"] = Guid.NewGuid().ToString();
            shellSettings["RecipeName"] = settings.RecipeName;
            await shellHost.UpdateShellSettingsAsync(shellSettings);
        }

        if (!shellHost.TryGetSettings(tenantName, out var savedSettings))
        {
            ModelState.AddModelError(string.Empty, "Could not create the tenant. Please try again.");
            return View(nameof(Index), model);
        }

        var recipes = await setupService.GetSetupRecipesAsync();
        var recipe = recipes.FirstOrDefault(r => string.Equals(r.Name, settings.RecipeName, StringComparison.OrdinalIgnoreCase));
        if (recipe is null)
        {
            ModelState.AddModelError(string.Empty, $"Setup recipe '{settings.RecipeName}' was not found.");
            return View(nameof(Index), model);
        }

        var setupContext = new SetupContext
        {
            ShellSettings = savedSettings,
            EnabledFeatures = null,
            Errors = new Dictionary<string, string>(),
            Recipe = recipe,
            Properties = new Dictionary<string, object>
            {
                ["SiteName"] = model.StoreName,
                ["AdminUsername"] = model.AdminUsername,
                ["AdminEmail"] = model.AdminEmail,
                ["AdminPassword"] = model.AdminPassword,
                ["SiteTimeZone"] = settings.SiteTimeZone,
                ["DatabaseProvider"] = "SqlConnection",
                ["DatabaseConnectionString"] = connectionString,
                ["DatabaseTablePrefix"] = "",
                ["DatabaseSchema"] = "",
            },
        };

        await setupService.SetupAsync(setupContext);

        if (setupContext.Errors.Count > 0)
        {
            foreach (var error in setupContext.Errors) ModelState.AddModelError(error.Key, error.Value);
            return View(nameof(Index), model);
        }

        await storeProvisioner.ProvisionAsync(tenantName);

        var storeUrl = $"{Request.Scheme}://{Request.Host}/{model.Subdomain}";
        return View("Success", storeUrl);
    }
}
