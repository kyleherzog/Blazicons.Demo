using Microsoft.AspNetCore.Components;

namespace Blazicons.Demo.Components;

public partial class FilterDisplay
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
