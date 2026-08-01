using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace Lyra.Commerce;

public sealed class AdminMenu(IStringLocalizer<AdminMenu> S) : INavigationProvider
{
    public ValueTask BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            return ValueTask.CompletedTask;

        builder.Add(S["Commerce"], "3", commerce => commerce
            .AddClass("commerce")
            .Id("commerce")
            .Add(S["Products"], "1", products => products
                .Action("Index", "ProductAdmin", new { area = "Lyra.Commerce" })
                .Permission(Permissions.ManageProducts)
                .LocalNav()));

        return ValueTask.CompletedTask;
    }
}
