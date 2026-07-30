using Lyra.AiPageBuilder.Services;
using Lyra.AiPageBuilder.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.Flows.Models;
using OrchardCore.Title.Models;

namespace Lyra.AiPageBuilder.Controllers.Admin;

[Admin]
public sealed class AiPageBuilderAdminController(
    PageGenerationOrchestrator orchestrator,
    ContentPlanApplier applier,
    IContentManager contentManager,
    IAuthorizationService authorizationService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!await authorizationService.AuthorizeAsync(User, Permissions.GeneratePages))
            return Forbid();

        return View(new GeneratePageViewModel());
    }

    [HttpPost, ActionName(nameof(Index))]
    public async Task<IActionResult> IndexPost(GeneratePageViewModel model)
    {
        if (!await authorizationService.AuthorizeAsync(User, Permissions.GeneratePages))
            return Forbid();

        if (string.IsNullOrWhiteSpace(model.Prompt))
        {
            ModelState.AddModelError(nameof(model.Prompt), "Describe the page you want first.");
            return View(nameof(Index), model);
        }

        var plan = await orchestrator.GenerateAsync(model.Prompt);
        var page = await applier.ApplyAsync(plan);

        return RedirectToAction(nameof(Preview), new { contentItemId = page.ContentItemId });
    }

    [HttpGet]
    public async Task<IActionResult> Preview(string contentItemId)
    {
        if (!await authorizationService.AuthorizeAsync(User, Permissions.GeneratePages))
            return Forbid();

        var page = await contentManager.GetAsync(contentItemId, VersionOptions.Latest);
        if (page is null) return NotFound();

        return View(ToViewModel(page));
    }

    [HttpPost]
    public async Task<IActionResult> Publish(string contentItemId)
    {
        if (!await authorizationService.AuthorizeAsync(User, Permissions.GeneratePages))
            return Forbid();

        var page = await contentManager.GetAsync(contentItemId, VersionOptions.Latest);
        if (page is null) return NotFound();

        await contentManager.PublishAsync(page);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Discard(string contentItemId)
    {
        if (!await authorizationService.AuthorizeAsync(User, Permissions.GeneratePages))
            return Forbid();

        var page = await contentManager.GetAsync(contentItemId, VersionOptions.Latest);
        if (page is not null) await contentManager.RemoveAsync(page);

        return RedirectToAction(nameof(Index));
    }

    private static PreviewPlanViewModel ToViewModel(ContentItem page)
    {
        var flow = page.As<FlowPart>();
        return new PreviewPlanViewModel
        {
            ContentItemId = page.ContentItemId,
            PageTitle = page.As<TitlePart>()?.Title ?? page.DisplayText,
            Path = page.As<OrchardCore.Autoroute.Models.AutoroutePart>()?.Path ?? string.Empty,
            WidgetHtmlBlocks = flow?.Widgets.Cast<ContentItem>().Select(ExtractHtml).ToList() ?? [],
        };
    }

    /// <summary>ContentItem.Content is exposed as `dynamic` over the underlying JSON — indexing into
    /// it by a runtime-known part name (the content type name) returns another dynamic node whose
    /// value can't be read via JsonNode's GetValue&lt;T&gt;() (that API belongs to a different, static
    /// JSON type), only via a plain cast once we're down to the leaf value.</summary>
    private static string ExtractHtml(ContentItem widget)
    {
        dynamic? part = widget.Content[widget.ContentType];
        if (part is null) return string.Empty;

        dynamic? html = part.Content?.Html;
        return html is null ? string.Empty : (string)html;
    }
}
