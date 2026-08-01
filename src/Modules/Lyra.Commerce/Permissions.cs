using OrchardCore.Security.Permissions;

namespace Lyra.Commerce;

public sealed class Permissions : IPermissionProvider
{
    public static readonly Permission ManageProducts = new("ManageLyraProducts", "Manage products");

    public Task<IEnumerable<Permission>> GetPermissionsAsync() =>
        Task.FromResult<IEnumerable<Permission>>([ManageProducts]);

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype { Name = "Administrator", Permissions = [ManageProducts] },
    ];
}
