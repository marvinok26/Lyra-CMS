using Lyra.AiPageBuilder.Settings;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace Lyra.AiPageBuilder;

public sealed class AdminMenu(IStringLocalizer<AdminMenu> S) : INavigationProvider
{
    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            return ValueTask.CompletedTask;

        builder.Add(S["AI Page Builder"], "5", item => item
            .AddClass("ai-page-builder")
            .Id("aiPageBuilder")
            .Action("Index", "AiPageBuilderAdmin", new { area = "Lyra.AiPageBuilder" })
            .Permission(Permissions.GeneratePages)
            .LocalNav()
        );

        builder.Add(S["Settings"], settings => settings
            .Add(S["AI Page Builder"], "5", item => item
                .AddClass("ai-page-builder-settings")
                .Id("aiPageBuilderSettings")
                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = AiPageBuilderSettingsDisplayDriver.GroupId })
                .Permission(Permissions.ManageSettings)
                .LocalNav()
            )
        );

        return ValueTask.CompletedTask;
    }
}
