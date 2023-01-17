using Microsoft.AspNetCore.Components;

namespace Blazicons.Demo.Components;

public partial class FontLibrarySelection
{
    public string AreaClass => "btn btn-outline-light btn-sm text-start border-secondary font-library-selection w-100 " + (IsSelected ? "filter-selected" : "filter-not-selected");

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    public string Id { get; } = Guid.NewGuid().ToString();

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
            IsSelected = true;
            Parent.LibraryFilter = Name;
        }
        else
        {
            IsSelected = false;
        }
    }

    public void ParentFilterChanged(string? name)
    {
        Parent.AreaFiltersExpanded = false;
        IsSelected = name == Name;
        if (IsSelected)
        {
            Parent.LibraryFilterContent = ChildContent;
        }
    }

    public void HandleClick()
    {
        Parent.AreaFiltersExpanded = false;
    }

    protected override void OnInitialized()
    {
        if (!Parent.Filters.Contains(this))
        {
            Parent.Filters.Add(this);
        }

        ParentFilterChanged(Parent.LibraryFilter);

        base.OnInitialized();
    }
}