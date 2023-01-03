using Microsoft.AspNetCore.Components;

namespace Blazicons.Demo.Components;

public partial class IconCount
{
    private string iconsFilteredCount = string.Empty;

    [Parameter]
    public string IconsFilteredCount
    {
        get
        {
            return iconsFilteredCount;
        }

        set
        {
            if (iconsFilteredCount != value)
            {
                iconsFilteredCount = value;
                _ = InvokeAsync(StateHasChanged);
            }
        }
    }

    [Parameter]
    public string IconsTotalCount { get; set; } = string.Empty;
}
