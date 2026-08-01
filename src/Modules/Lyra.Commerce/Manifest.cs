using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Lyra.Commerce",
    Author = "Marvin Okongo",
    Website = "https://github.com/marvinok26/Lyra-CMS",
    Version = "0.0.1",
    Description = "Adds a Product content type with its own Commerce admin area (list, create, edit, "
        + "delete, price, stock) and a storefront widget that renders the latest products — the "
        + "reference example of a plugin bringing its own admin CRUD, per Lyra's extensibility model.",
    Category = "Commerce"
)]
