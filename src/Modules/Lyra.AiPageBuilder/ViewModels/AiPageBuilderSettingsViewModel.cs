namespace Lyra.AiPageBuilder.ViewModels;

// Not sealed: this view model is passed through Initialize<TModel>() (the shape factory), which
// builds a Castle DynamicProxy of it — a sealed class throws TypeLoadException at that point.
public class AiPageBuilderSettingsViewModel
{
    public string? ActiveProvider { get; set; }
    public string? ApiKey { get; set; }
    public string? Model { get; set; }
    public string? OllamaBaseUrl { get; set; }
}
