using System.Reactive.Linq;
using Blazicons.Demo.Components;
using Blazicons.Demo.Models;
using Blazor.Analytics;
using CodeCasing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace Blazicons.Demo.Pages;

public partial class Index : IDisposable
{
    private readonly List<IconEntry> filteredIcons = new();
    private string? activeQuery;
    private bool areaFiltersExpanded;
    private bool hasDisposed;
    private string libraryFilter = string.Empty;
    private RenderFragment? libraryFilterContent;
    private IDisposable? queryChangedSubscription;

    public Index()
    {
        Search = new IconSearchModel();
        SubscribeToChanges();
    }

    ~Index()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public IconEntry ActiveIcon { get; set; } = new IconEntry();

    public string ActiveIconCopyExampleLink => $"javascript:navigator.clipboard.writeText('{ActiveIconExample}');";

    public string ActiveIconCopyNameLink => $"javascript:navigator.clipboard.writeText('{ActiveIcon.Code}');";

    public string ActiveIconCopyPackageLink => $"javascript:navigator.clipboard.writeText('{ActiveIcon.Assembly}');";

    public string ActiveIconDisplayName => ActiveIcon?.Name.ExpandToTitleCase() ?? string.Empty;

    public string ActiveIconExample => $"<Blazicon Svg=\"{ActiveIcon.Code}\" />";

    public string ActiveIconNugetAddress => $"https://www.nuget.org/packages/{ActiveIcon.Assembly}";

    public string? ActiveQuery
    {
        get
        {
            return activeQuery;
        }

        set
        {
            if (activeQuery != value)
            {
                activeQuery = value;
                LoadFilteredIcons();
                _ = InvokeAsync(StateHasChanged);
            }
        }
    }

    [Inject]
    public IAnalytics? Analytics { get; set; }

    public bool AreaFiltersExpanded
    {
        get
        {
            return areaFiltersExpanded;
        }

        set
        {
            if (areaFiltersExpanded != value)
            {
                areaFiltersExpanded = value;
                _ = InvokeAsync(StateHasChanged);
            }
        }
    }

    public string FilterAreaClass => AreaFiltersExpanded ? "mt-1 mt-md-3" : "d-none d-md-block mt-1 mt-md-3";

    public string FilterAreaTogglerClass => AreaFiltersExpanded ? "d-none" : "d-md-none";

    public IList<IconEntry> FilteredIcons
    {
        get
        {
            return filteredIcons;
        }
    }

    public IList<FontLibrarySelection> Filters { get; } = new List<FontLibrarySelection>();

    public IList<IconEntry> Icons { get; } = new List<IconEntry>();

    public string? IconsFilteredCount => filteredIcons.Count.ToString("N0");

    public string IconsTotalCount => Icons.Count.ToString("N0");

    public bool IsShowingModal { get; set; }

    public string LibraryFilter
    {
        get
        {
            return libraryFilter;
        }

        set
        {
            if (libraryFilter != value)
            {
                libraryFilter = value;
                foreach (var filter in Filters)
                {
                    filter.ParentFilterChanged(value);
                }

                LoadFilteredIcons();
                StateHasChanged();
            }
        }
    }

    public RenderFragment? LibraryFilterContent
    {
        get
        {
            return libraryFilterContent;
        }

        set
        {
            if (libraryFilterContent != value)
            {
                libraryFilterContent = value;
                _ = InvokeAsync(StateHasChanged);
            }
        }
    }

    public IconSearchModel Search { get; }

    public Virtualize<IconEntry>? VirtualizedIcons { get; set; }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!hasDisposed)
        {
            if (disposing)
            {
                UnsubsribeFromChanges();
            }

            hasDisposed = true;
        }
    }

    protected override Task OnInitializedAsync()
    {
        AddLibraryIcons(typeof(MdiIcon));
        AddLibraryIcons(typeof(FontAwesomeRegularIcon));
        AddLibraryIcons(typeof(FontAwesomeSolidIcon));
        AddLibraryIcons(typeof(BootstrapIcon));
        AddLibraryIcons(typeof(GoogleMaterialOutlinedIcon));
        AddLibraryIcons(typeof(GoogleMaterialFilledIcon));
        AddLibraryIcons(typeof(GoogleMaterialRoundIcon));
        AddLibraryIcons(typeof(GoogleMaterialSharpIcon));
        AddLibraryIcons(typeof(GoogleMaterialTwoToneIcon));
        AddLibraryIcons(typeof(Ionicon));
        AddLibraryIcons(typeof(FluentUiIcon));
        AddLibraryIcons(typeof(FluentUiFilledIcon));

        LoadFilteredIcons();

        return base.OnInitializedAsync();
    }

    private void AddLibraryIcons(Type type)
    {
        var properties = type.GetProperties();

        properties = properties.OrderBy(x => x.Name).ToArray();

        foreach (var property in properties)
        {
            var icon = (SvgIcon?)property.GetValue(null);
            if (icon is not null)
            {
                Icons.Add(new IconEntry
                {
                    Name = property.Name,
                    Icon = icon,
                    Library = type.Name,
                    Assembly = type.Assembly?.GetName().Name ?? string.Empty,
                });
            }
        }
    }

    private void HandleFilterExpandToggle()
    {
        AreaFiltersExpanded = !AreaFiltersExpanded;
    }

    private void HideModal()
    {
        IsShowingModal = false;
    }

    private void LoadFilteredIcons()
    {
        var result = Icons.AsEnumerable();
        if (!string.IsNullOrEmpty(ActiveQuery))
        {
            var queryWords = ActiveQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            result = Icons.Where(x => queryWords.All(w => x.Name.Contains(w, StringComparison.OrdinalIgnoreCase)));
        }

        if (!string.IsNullOrEmpty(LibraryFilter))
        {
            result = result.Where(x => x.Library == LibraryFilter);
        }

        filteredIcons.Clear();
        filteredIcons.AddRange(result);
    }

    private void ShowIconDetails(IconEntry entry)
    {
        ActiveIcon = entry;
        ShowModal();

        if (Analytics is not null)
        {
            _ = Analytics.TrackEvent("select_content", new { content_type = "icon", item_id = entry.Code }).ConfigureAwait(true);
        }
    }

    private void ShowModal()
    {
        IsShowingModal = true;
    }

    private void SubscribeToChanges()
    {
        queryChangedSubscription = Search.WhenPropertyChanged.Throttle(TimeSpan.FromMilliseconds(400)).Subscribe(x => ActiveQuery = Search.Query);
    }

    private void UnsubsribeFromChanges()
    {
        queryChangedSubscription?.Dispose();
    }
}