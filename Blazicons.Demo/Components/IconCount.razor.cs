using Microsoft.AspNetCore.Components;

namespace Blazicons.Demo.Components;

public partial class IconCount
{
    [Parameter]
    public string IconsFilteredCount { get; set; } = string.Empty;

    [Parameter]
    public string IconsTotalCount { get; set; } = string.Empty;
}
