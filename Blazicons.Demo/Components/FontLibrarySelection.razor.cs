using Microsoft.AspNetCore.Components;

namespace Blazicons.Demo.Components;

public partial class FontLibrarySelection
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public string Id { get; } = Guid.NewGuid().ToString();

    [Parameter]
    public bool IsSelected { get; set; }

    [Parameter]
    public string Name { get; set; } = string.Empty;

    [CascadingParameter]
    public Pages.Index Parent { get; set; } = default!;

    private void HandleChange(ChangeEventArgs args)
    {
        var value = args.Value as string;
        if (value == "on")
        {
            Parent.LibraryFilter = Name;
        }
    }
}