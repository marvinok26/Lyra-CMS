using Lyra.AiPageBuilder.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace Lyra.AiPageBuilder.Settings;

public sealed class AiPageBuilderSettingsDisplayDriver(
    IAuthorizationService authorizationService,
    IHttpContextAccessor httpContextAccessor) : SiteDisplayDriver<AiPageBuilderSettings>
{
    public const string GroupId = "Lyra.AiPageBuilder";

    protected override string SettingsGroupId => GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, AiPageBuilderSettings settings, BuildEditorContext context)
    {
        if (!await authorizationService.AuthorizeAsync(httpContextAccessor.HttpContext?.User, Permissions.ManageSettings))
            return null;

        return Initialize<AiPageBuilderSettingsViewModel>("AiPageBuilderSettings_Edit", model =>
        {
            model.ActiveProvider = settings.ActiveProvider;
            model.ApiKey = settings.ApiKey;
            model.Model = settings.Model;
            model.OllamaBaseUrl = settings.OllamaBaseUrl;
        }).Location("Content:5").OnGroup(GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AiPageBuilderSettings settings, UpdateEditorContext context)
    {
        if (!await authorizationService.AuthorizeAsync(httpContextAccessor.HttpContext?.User, Permissions.ManageSettings))
            return null;

        var model = new AiPageBuilderSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.ActiveProvider = string.IsNullOrWhiteSpace(model.ActiveProvider) ? null : model.ActiveProvider;
        settings.ApiKey = string.IsNullOrWhiteSpace(model.ApiKey) ? null : model.ApiKey;
        settings.Model = string.IsNullOrWhiteSpace(model.Model) ? null : model.Model;
        settings.OllamaBaseUrl = string.IsNullOrWhiteSpace(model.OllamaBaseUrl) ? null : model.OllamaBaseUrl;

        return await EditAsync(site, settings, context);
    }
}
