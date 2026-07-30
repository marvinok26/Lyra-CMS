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

        return ValueTask.CompletedTask;
    }
}
