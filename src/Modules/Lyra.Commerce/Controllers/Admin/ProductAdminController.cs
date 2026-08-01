using Lyra.Commerce.Services;
using Lyra.Commerce.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace Lyra.Commerce.Controllers.Admin;

/// <summary>
/// A domain-specific "Products" list (Name / SKU / Price / Stock) that the Commerce module owns
/// end to end, proving the "a plugin brings its own admin CRUD" pattern this project exists to
/// demonstrate. Create and Edit deliberately hand off to Orchard Core's own, already-verified
/// content item editor rather than reimplementing field-by-field form handling — this controller
/// owns the list and delete experience, which is where a domain-specific view genuinely helps.
/// </summary>
[Admin]
public sealed class ProductAdminController(
    ISession session,
    IContentManager contentManager,
    IAuthorizationService authorizationService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!await authorizationService.AuthorizeAsync(User, Permissions.ManageProducts))
            return Forbid();

        var products = await session.Query<ContentItem, ContentItemIndex>(x =>
                x.ContentType == "Product" && x.Latest)
            .OrderByDescending(x => x.CreatedUtc)
            .ListAsync();

        var summaries = products.Select(ProductProjection.ToSummary).ToList();

        return View(new ProductListViewModel { Products = summaries });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string contentItemId)
    {
        if (!await authorizationService.AuthorizeAsync(User, Permissions.ManageProducts))
            return Forbid();

        var product = await contentManager.GetAsync(contentItemId, VersionOptions.Latest);
        if (product is not null) await contentManager.RemoveAsync(product);

        return RedirectToAction(nameof(Index));
    }
}
