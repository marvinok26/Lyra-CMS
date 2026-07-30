using OrchardCore.Security.Permissions;

namespace Lyra.AiPageBuilder;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission GeneratePages = new("GenerateAiPages", "Generate pages using AI");

    public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
        Task.FromResult<IEnumerable<Permission>>([GeneratePages]);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype { Name = "Administrator", Permissions = [GeneratePages] },
    ];
}
