namespace Lyra.AiPageBuilder.ViewModels;

public sealed class PreviewPlanViewModel
{
    public string ContentItemId { get; set; } = string.Empty;
    public string PageTitle { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public List<string> WidgetHtmlBlocks { get; set; } = [];
}
